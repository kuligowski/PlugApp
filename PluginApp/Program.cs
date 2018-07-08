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
        private static Dictionary<string, MethodInfo> availableFunctions;

        private static (IPluginProvider PluginProvider , ILogger Logger) ConfigureIoCContainer()
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
            return (pluginMgr, logger /* null */);            
        }

        static void Main()
        {
            (var pluginMgr, var logger) = ConfigureIoCContainer();
            try
            {
                using (pluginMgr)
                {
                    var pluginsInfo = pluginMgr.GetPluginsInfo();
                    while (true)
                    {
                        var selectedOption = ConsoleManager.GetSelectedTopLevelOption();
                        switch (selectedOption)
                        {
                            case PluginOption.PrintDescriptions:
                                logger?.LogInformation("Print plugin descriptions called");
                                ConsoleManager.PrintPluginsDescriptions(pluginsInfo.Select(pi => pi.Description).ToArray());
                                break;
                            case PluginOption.ExecutePlugin:
                                logger?.LogInformation("User tries to execute plugin");
                                var availablePlugins = pluginsInfo.OrderBy(pi => pi.Id).ToList();
                                if (availablePlugins.Count == 0)
                                {
                                    Console.WriteLine("No pluggins available");
                                    logger?.LogInformation("No pluggins available");
                                    Console.ReadKey();
                                    break;
                                }

                                var selectedPluginId = ConsoleManager.GetSelectedOption(availablePlugins.Select(ap => ap.Description).ToArray());

                                Console.WriteLine("Provide string:");
                                logger?.LogInformation($"Executing {selectedPluginId} plugin");
                                var result = pluginMgr.ExecutePlugin(availablePlugins[selectedPluginId].PluginType, Console.ReadLine());
                                Console.WriteLine($"Result: {result}");
                                logger?.LogInformation($"{selectedPluginId} result {result}");
                                Console.ReadKey();
                                break;
                            case PluginOption.Interactive:
                                logger?.LogInformation("Interactive mode started");
                                Interactive(pluginsInfo, pluginMgr, logger);
                                break;
                            default:
                                logger?.LogCritical($"Unknown top menu level option <{selectedOption}>");
                                throw new ArgumentOutOfRangeException("Unsupported option");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured, contact admin - {e}");
                logger?.LogCritical(e, $"An error occured, contact admin - {e.Message}");
            }
        }

        static Program()
        {
            availableFunctions = typeof(IPluginProvider)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(methodInfo => new { methodInfo.Name, methodInfo })
                .ToDictionary(anonymous => anonymous.Name, anonymous => anonymous.methodInfo);
        }

        //TODO: Use https://www.nuget.org/packages/CommandDotNet instead of below closely coupled implementation
        static void Interactive(IEnumerable<PluginDecorator> plugins, IPluginProvider pluginMgr, ILogger logger)
        {
            Console.Clear();
            Console.WriteLine("=== Interactive mode ===");
            Console.WriteLine("Usage just like in c# code, ie. functionName(argument1, argument2)");
            Console.WriteLine("Available functions:");
            foreach (var fun in availableFunctions)
            {
                var methodArgs = string.Join(", ", fun.Value.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"    {fun.Value.ReturnType.Name} {fun.Key}({methodArgs})");
            }

            var exitFun = "Exit";
            Console.WriteLine($"    {exitFun}");
            var userPlugins = new List<IPlugin>();
            while (true)
            {
                Console.Write("$ ");
                string commandInput = Console.ReadLine();
                string command = commandInput.Trim(new char [] {' ', ';', ')'});
                string[] commandTokens = command.Split(new char[] { '(', ',' });
                string userCommand = commandTokens.First();
                string[] stringArguments = commandTokens.Skip(1).Where(ar => !string.IsNullOrWhiteSpace(ar)).ToArray();
                
                if (command.ToLower().Contains(exitFun.ToLower()))
                {
                    logger?.LogInformation("User exited interactive mode");
                    break;
                }
                
                logger?.LogInformation($"Running command {commandInput}");

                if (availableFunctions.ContainsKey(userCommand))
                {
                    var selectedFunction = availableFunctions[userCommand];
                    var arguments = PrepareArguments(stringArguments, plugins);
                    if (arguments == null)
                    {
                        Console.WriteLine($"$ Incorrect arguments, try again.");
                        continue;
                    }
                    
                    //add optional parameter  - no compiler magic in reflection unfortunately
                    if (selectedFunction.ReturnType == typeof(IEnumerable<PluginDecorator>) && arguments.Length == 1)
                    {
                        logger?.LogInformation("selectedFunction.ReturnType == typeof(IEnumerable<PluginDecorator>) && arguments.Length == 1");
                        arguments.Concat(new object[] { false });
                    }

                    var output = selectedFunction.Invoke(pluginMgr, arguments);

                    if (selectedFunction.ReturnType == typeof(string))
                    {
                        logger?.LogInformation($"Plugin returned <{output}> string");
                        Console.WriteLine($"$ {output}");
                    }
                    else if (selectedFunction.ReturnType == typeof(IPlugin))
                    {
                        userPlugins.Add((IPlugin)output);
                        logger?.LogInformation($"Plugin returned <{output}> IPlugin");
                        Console.WriteLine($"$ Plugin created");
                    }
                    else if (selectedFunction.ReturnType == typeof(IEnumerable<PluginDecorator>))
                    {
                        logger?.LogInformation($"Plugin returned <{output}> IEnumerable<PluginDecorator>");
                        Console.WriteLine($"$ \n{output}");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown command {userCommand}");
                    logger?.LogError($"Unknown command {userCommand}");
                }
            }
        }

        //Assumption that first parameter is plugin Type
        static object[] PrepareArguments(string[] passedArguments, IEnumerable<PluginDecorator> plugins)
        {
            if (passedArguments.Length == 0)
            {
                return passedArguments;
            }

            var plugin = plugins.FirstOrDefault(p => p.Description == passedArguments[0]);
            if (plugin == null)
            {
                return null;
            }

            if (passedArguments.Length == 1)
            {
                return new object[] { plugin.PluginType };
            }

            if (bool.TryParse(passedArguments[1], out bool singleton))
            {
                return new object[] { plugin.PluginType, singleton };
            }
                
            return new object[] {plugin.PluginType, passedArguments[1]};
        }
    }
}
