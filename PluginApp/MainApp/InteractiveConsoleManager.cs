
using System;
using PluginCore;
using PluginShared;
using System.Linq;
using CommandDotNet.Attributes;

namespace PluginApp
{
    public class InteractiveConsoleManager
    {
        [InjectProperty]
        public IPluginProvider pluginManager {get;set;}

        //TODO: add validation attributes
        public IPlugin GetPlugin(string pluginType)
        {
            if (pluginType == null)
            {
                return null;
            }

            return this.pluginManager.RequestPlugin(Type.GetType(pluginType));
        }

        public string GetPluginsInfo()
        {
            return string.Join("\n", this.pluginManager.GetPluginsInfo().Select(pi => pi.Description));
        }

        public string ExecutePlugin(string pluginType, string input)
        {
            if (pluginType == null)
            {
                return null;
            }

            return this.pluginManager.RequestPlugin(Type.GetType(pluginType))?.Execute(input);
        }
    }
}