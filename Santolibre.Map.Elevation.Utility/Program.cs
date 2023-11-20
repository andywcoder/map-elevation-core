using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Utility.Commands;

namespace Santolibre.Map.Elevation.Utility
{
    class Program
    {
        public static void Main(string[] args)
        {
            var servicesProvider = SetupServices();

            var app = new CommandLineApplication();
            app.Conventions
                .SetAppNameFromEntryAssembly()
                .UseConstructorInjection(servicesProvider);
            RootCommand.Configure(app);
            app.Execute(args);
        }

        private static ServiceProvider SetupServices()
        {
            return new ServiceCollection()
                .AddSingleton<IFileConverter, FileConverter>()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build())
                .AddLogging(config =>
                {
                    config.SetMinimumLevel(LogLevel.Trace);
                    config.AddConsole();
                })
                .AddTransient<ConvertCommand>()
                .BuildServiceProvider();
        }
    }
}
