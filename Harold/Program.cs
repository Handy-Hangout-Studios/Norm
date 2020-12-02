using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Harold
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
                .Build()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);

        public static Action<IConfigurationBuilder> ConfigureHostConfiguration(string[] args) =>
            configuration => configuration
                .AddJsonFile("config.json", optional: false)
                .AddCommandLine(args);

        public static void ConfigureHangfireServices(HostBuilderContext context, IServiceCollection services)
        {

        }
    }
}
