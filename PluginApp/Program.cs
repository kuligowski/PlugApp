using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PluginCore;
using PluginShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Extensions;
using PluginLog;
using MongoDB.Driver;
using DbProvider;
using CommandDotNet;
using CommandDotNet.IoC.MicrosoftDependencyInjection;

namespace PluginApp
{
    public enum PluginOption
    {
        PrintDescriptions,
        ExecutePlugin,
        Interactive
    }
    
    class Program
    {
        private static (IPluginProvider PluginProvider , ILogger Logger, ServiceProvider dependencyProvider) ConfigureIoCContainer()
        {
            Func<IServiceProvider, IMongoDatabase> dbSupplier = p => MongoDbProvider.GetDatabaseHandle();
            var dependencyProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IPluginProvider, PluginManager>()
                .AddScoped<IMongoDatabase>(dbSupplier)
                .BuildServiceProvider();
            var logger = dependencyProvider.GetService<ILoggerFactory>()
                        .AddLogger(dependencyProvider.GetService<IMongoDatabase>())
                        .CreateLogger("PluginApp");
            var pluginMgr = dependencyProvider.GetService<IPluginProvider>();

            // set logger to null if no loging/no mongoDb configured
            return (pluginMgr, null /* logger */, dependencyProvider);
        }

        static void Main(string[] args)
        {
            (var pluginMgr, var logger, var dependencyProvider) = ConfigureIoCContainer();
            try
            {
                AppRunner<InteractiveConsoleManager> appRunner =
                    new AppRunner<InteractiveConsoleManager>().UseMicrosoftDependencyInjection(dependencyProvider);
                appRunner.Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured, contact admin - {e.Message}");
                logger?.LogCritical(e, $"An error occured, contact admin - {e.Message}");
            }
        }
    }
}
