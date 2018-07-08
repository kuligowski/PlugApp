using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PluginCore;
using PluginShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace PluginApp
{
    public enum PluginOption
    {
        PrintDescriptions,
        ExecutePlugin,
        Interactive
    }

    class PluginAppLogger : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    
    class Program
    {
        private static Dictionary<string, MethodInfo> availableFunctions;
        static void Main()
        {
            try
            {
                var dependencyProvider = new ServiceCollection()
                    .AddLogging()
                    .AddSingleton<IPluginProvider, PluginManager>()
                    .BuildServiceProvider();
                var logginProvider = dependencyProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
                //logginProvider.
                using (var pluginMgr = dependencyProvider.GetService<IPluginProvider>())
                {
                    var pluginsInfo = pluginMgr.GetPluginsInfo();
                    while (true)
                    {
                        var selectedOption = ConsoleManager.GetSelectedTopLevelOption();
                        switch (selectedOption)
                        {
                            case PluginOption.PrintDescriptions:
                                ConsoleManager.PrintPluginsDescriptions(pluginsInfo.Select(pi => pi.Description).ToArray());
                                break;
                            case PluginOption.ExecutePlugin:
                                var availablePlugins = pluginsInfo.OrderBy(pi => pi.Id).ToList();
                                var selectedPluginId = ConsoleManager.GetSelectedOption(availablePlugins.Select(ap => ap.Description).ToArray());

                                Console.WriteLine("Provide string:");
                                var result = pluginMgr.ExecutePlugin(availablePlugins[selectedPluginId].PluginType, Console.ReadLine());
                                Console.WriteLine($"Result: {result}");
                                Console.ReadKey();
                                break;
                            case PluginOption.Interactive:
                                Interactive(pluginsInfo, pluginMgr);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("Unsupported option");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured, contact admin - {e.Message}");
                //Logger.Exception(e);
            }
        }

        static Program()
        {
            availableFunctions = typeof(IPluginProvider)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(methodInfo => new { methodInfo.Name, methodInfo })
                .ToDictionary(anonymous => anonymous.Name, anonymous => anonymous.methodInfo);
        }

        //TODO: Use https://www.nuget.org/packages/CommandDotNet
        static void Interactive(IEnumerable<PluginDecorator> plugins, IPluginProvider pluginMgr)
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
                string command = Console.ReadLine().Trim(new char [] {' ', ';', ')'});
                string[] commandTokens = command.Split(new char[] { '(', ',' });
                string userCommand = commandTokens.First();
                string[] stringArguments = commandTokens.Skip(1).Where(ar => !string.IsNullOrWhiteSpace(ar)).ToArray();
                
                if (command.ToLower().Contains(exitFun.ToLower()))
                {
                    break;
                }

                if (availableFunctions.ContainsKey(userCommand))
                {
                    var selectedFunction = availableFunctions[userCommand];
                    var arguments = PrepareArguments(stringArguments, plugins);
                    
                    //add optional parameter  - no compiler magic in reflection unfortunately
                    if (selectedFunction.ReturnType == typeof(PluginsInfo) && arguments.Length == 1)
                    {
                        arguments.Concat(new object[] { false });
                    }

                    var output = selectedFunction.Invoke(pluginMgr, arguments);

                    if (selectedFunction.ReturnType == typeof(string))
                    {
                        Console.WriteLine($"$ {output}");
                    }
                    else if (selectedFunction.ReturnType == typeof(IPlugin))
                    {
                        userPlugins.Add((IPlugin)output);
                        Console.WriteLine($"$ Plugin created");
                    }
                    else if (selectedFunction.ReturnType == typeof(PluginsInfo))
                    {
                        Console.WriteLine($"$ \n{output}");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown command {userCommand}");
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
            if (passedArguments.Length == 1)
            {
                return new object[] { plugin.PluginType };
            }

            bool singleton;
            if (bool.TryParse(passedArguments[1], out singleton))
            {
                return new object[] {plugin.PluginType, singleton};
            }
                
            return new object[] {plugin.PluginType, passedArguments[1]};
        }
    }
}
