using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Norm.DatabaseRewrite.Contexts;
using Norm.DatabaseRewrite.TypeHandlers;
using NormRewrite.OptionConfigs;
using NormRewrite.Services;
using Npgsql;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        Dapper.SqlMapper.AddTypeHandler(new DapperDateTimeTypeHandler());
        services
            .AddOptions<NormConfig>()
            .Bind(context.Configuration.GetSection(nameof(NormConfig)));
        services.AddDbContext<NormDbContext>((provider, optionsBuilder) =>
        {
            string databaseUrl = provider.GetRequiredService<IConfiguration>()["DATABASE_URL"];
            ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            Uri databaseUri = new(databaseUrl);
            string[] userInfo = databaseUri.UserInfo.Split(':');

            NpgsqlConnectionStringBuilder builder = new()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
            };

            optionsBuilder.UseNpgsql(
                    builder.ToString(),
                    options =>
                    {
                        options.UseNodaTime().MigrationsAssembly("NormRewrite");
                    }
                )
                .UseLoggerFactory(loggerFactory);
        });
        services.AddMediatR(typeof(NormDbContext));
        services.AddHostedService<NormService>();
    })
    .Build();

await host.RunAsync();