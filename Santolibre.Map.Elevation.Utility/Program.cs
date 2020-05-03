using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Utility.Commands;
using System;

namespace Santolibre.Map.Elevation.Utility
{
    class Program
    {
        public static void Main(string[] args)
        {
            var servicesProvider = SetupServices();

            try
            {
                var app = new CommandLineApplication();
                app.Conventions
                    .SetAppNameFromEntryAssembly()
                    .UseConstructorInjection(servicesProvider);
                RootCommand.Configure(app);
                app.Execute(args);
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal(e.Message);
            }

            NLog.LogManager.Shutdown();
        }

        private static ServiceProvider SetupServices()
        {
            return new ServiceCollection()
                .AddSingleton<IFileConverter, FileConverter>()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build())
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true
                    });
                })
                .AddTransient<ConvertCommand>()
                .BuildServiceProvider();
        }
    }
}
