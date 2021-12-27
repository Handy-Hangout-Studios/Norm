using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NormRewrite.OptionConfigs;
using NormRewrite.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .AddOptions<NormConfig>()
            .Bind(context.Configuration.GetSection(nameof(NormConfig)));

        services.AddHostedService<Norm>();
    })
    .Build();

await host.RunAsync();