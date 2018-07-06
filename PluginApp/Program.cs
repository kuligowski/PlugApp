using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PluginCore;

namespace PluginApp
{
    public enum PluginOption
    {
        PrintDescriptions,
        ExecutePlugin,
        Interactive
    }

    public enum InteractiveOps
    {
        CreateNew,
        Execute
    }

    class Program
    {
        static void Main()
        {
            try
            {
                using (var pluginMgr = PluginManager.Instance)
                {
                    var pluginsInfo = pluginMgr.GetPluginsInfo();
                    while (true) //TODO: Smart end condition, esc key?
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
                                Interactive(pluginsInfo);
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


        //IEnumerable needs to stay, plugins are discovered during runtime
        static void Interactive(IEnumerable<PluginDecorator> plugins)
        {
        }
    }
}
