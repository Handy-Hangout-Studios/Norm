using Hangfire;
using Hangfire.PostgreSql;
using Harold.Configuration;
using Harold.Database;
using Harold.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using System;
using Microsoft.EntityFrameworkCore.Design;

namespace Harold
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
                .Build()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(ConfigureSerilog)
                .ConfigureHostConfiguration(ConfigureHostConfiguration(args))
                .ConfigureServices(ConfigureHangfire)
                .ConfigureServices(ConfigureBotOptions)
                .ConfigureServices(ConfigureBotServices)
                .UseConsoleLifetime();

        public static Action<IConfigurationBuilder> ConfigureHostConfiguration(string[] args) =>
            configuration => configuration
                .AddJsonFile("config.json", optional: false)
                .AddCommandLine(args);

        public static void ConfigureSerilog(HostBuilderContext context, IServiceProvider services, LoggerConfiguration configuration)
        {
            configuration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(formatter: new JsonFormatter(renderMessage: true), "log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
        }

        public static void ConfigureBotOptions(HostBuilderContext context, IServiceCollection services) =>
            services.AddOptions<BotConfig>()
                .Bind(context.Configuration.GetSection(BotConfig.Section))
                .ValidateDataAnnotations();

        public static void ConfigureHangfire(HostBuilderContext context, IServiceCollection services)
        {
            HangfireConfig hangfireConfig = new HangfireConfig();
            context.Configuration.GetSection(HangfireConfig.Section).Bind(hangfireConfig);
            string connString = hangfireConfig.Database.AsNpgsqlConnectionString();

            services.AddHangfire(configuration =>
                    configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UsePostgreSqlStorage(connString, new PostgreSqlStorageOptions
                        {
                            DistributedLockTimeout = TimeSpan.FromMinutes(1),
                        })
                    )
                .AddHangfireServer(options =>
                    {
                        options.StopTimeout = TimeSpan.FromSeconds(15);
                        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
                        options.WorkerCount = 4;
                    });
        }

        public static void ConfigureBotServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IBotService, BotService>()
                .AddTransient<AnnouncementService>()
                .AddTransient<BotPsqlContext>()
                .AddHostedService<HaroldHostedService>();
        }
    }
}
