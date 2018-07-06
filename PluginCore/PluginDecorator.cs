using System;
using System.Reflection;
using System.Linq;
using PluginShared;

namespace PluginCore
{
    public class PluginDecorator
    {
        public PluginDecorator(string id, Assembly assembly, Type pluginType)
        {
            this.Id = id;
            this.Assembly = assembly;
            var descriptionAttribute = assembly
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                .OfType<AssemblyDescriptionAttribute>()
                .FirstOrDefault();
            this.Description = descriptionAttribute?.Description ?? "Description missing";
            this.PluginType = pluginType;
        }

        public string Id { get; private set; }

        public IPlugin Plugin { get; private set; }

        public Type PluginType { get; private set; }

        public Assembly Assembly { get; private set; }

        public string Description { get; private set; }
    
    }
}