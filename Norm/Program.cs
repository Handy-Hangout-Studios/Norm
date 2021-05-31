using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.TimeZones;
using Norm.Configuration;
using Norm.Database.Contexts;
using Norm.Database.TypeHandlers;
using Norm.Omdb;
using Norm.Services;
using Serilog;
using Serilog.Formatting.Json;
using System;

namespace Norm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .UseSerilog(ConfigureSerilog)
                    .ConfigureHostConfiguration(ConfigureHostConfiguration(args))
                    .ConfigureServices(ConfigureHangfire)
                    .ConfigureServices(ConfigureMediatR)
                    .ConfigureServices(AddBotServices)
                    .UseConsoleLifetime();
        }

        public static Action<IConfigurationBuilder> ConfigureHostConfiguration(string[] args)
        {
            return configuration => configuration
                .AddJsonFile("config.json", optional: false)
                .AddCommandLine(args);
        }

        public static void ConfigureSerilog(HostBuilderContext context, IServiceProvider services, LoggerConfiguration configuration)
        {
            // TODO: switch from hardcoded log file path to configuration based log file path
            configuration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(formatter: new JsonFormatter(renderMessage: true), "../../logs/Norm/log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console();
        }

        public static void ConfigureHangfire(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddOptions<NormHangfireOptions>()
                .Configure<IConfiguration>(
                    (options, configuration)
                        => configuration.Bind(NormHangfireOptions.Section, options
                    )
                 )
                .ValidateDataAnnotations();

            services
                .AddHangfire((serviceProvider, configuration) =>
                    {
                        NormHangfireOptions opts = serviceProvider
                            .GetRequiredService<IOptions<NormHangfireOptions>>().Value;

                        configuration
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UsePostgreSqlStorage(opts.Database.AsNpgsqlConnectionString(), new PostgreSqlStorageOptions
                            {
                                DistributedLockTimeout = TimeSpan.FromMinutes(1),
                            });
                        Dapper.SqlMapper.AddTypeHandler(new DapperDateTimeTypeHandler());
                    })
                .AddHangfireServer(options =>
                    {
                        options.StopTimeout = TimeSpan.FromSeconds(15);
                        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
                        options.WorkerCount = 4;
                    });
        }

        public static void ConfigureMediatR(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddMediatR(typeof(NormDbContext));
        }

        public static void AddBotServices(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddOptions<BotOptions>()
                .Bind(context.Configuration.GetSection(BotOptions.Section))
                .ValidateDataAnnotations();

            services
                .AddOptions<OmdbClientOptions>()
                .Bind(context.Configuration.GetSection("OmdbClient"))
                .ValidateDataAnnotations();

            services
                .AddSingleton<IClock>((p) => SystemClock.Instance)
                .AddSingleton<IDateTimeZoneSource>((p) => TzdbDateTimeZoneSource.Default)
                .AddSingleton<IDateTimeZoneProvider, DateTimeZoneCache>()
                .AddDbContext<NormDbContext>((s, o) =>
                {
                    DatabaseConfig db = s.GetRequiredService<IOptions<BotOptions>>().Value.Database;
                    ILoggerFactory lf = s.GetRequiredService<ILoggerFactory>();
                    o.UseNpgsql(db.AsNpgsqlConnectionString(), o => o.UseNodaTime().MigrationsAssembly("Norm"))
                     .UseLoggerFactory(lf);
                })
                .AddOmdbClient((s, o) =>
                {
                    OmdbClientOptions opts = s.GetRequiredService<IOptions<OmdbClientOptions>>().Value;
                    o.ApiKey = opts.ApiKey;
                    o.Version = opts.Version ?? o.Version;
                })
                .AddTransient<MovieNightService>()
                .AddTransient<LatexRenderService>()
                .AddSingleton<BotService>()
                .AddTransient<AnnouncementService>()
                .AddTransient<EventService>()
                .AddTransient<ModerationService>()
                .AddTransient<NodaTimeConverterService>()
                .AddHostedService<NormHostedService>();
        }
    }
}
