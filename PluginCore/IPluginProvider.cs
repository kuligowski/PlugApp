using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PluginShared;

namespace PluginCore
{
    public interface IPluginProvider : IDisposable
    {
        IPlugin RequestPlugin(Type pluginType, bool singleton = false);
        
        PluginsInfo GetPluginsInfo();

        string ExecutePlugin(Type pluginType, string input);
    }

    public class PluginsInfo : IEnumerable<PluginDecorator>
    {
        private IEnumerable<PluginDecorator> pluginsInfo;

        public PluginsInfo(IEnumerable<PluginDecorator> pluginsInfo)
        {
            this.pluginsInfo = pluginsInfo;
        }
        public IEnumerator<PluginDecorator> GetEnumerator()
        {
            foreach (var pI in pluginsInfo)
            {
                yield return pI;
            }
        }

        public override string ToString()
        {
            return string.Join("\n", pluginsInfo.Select(pi => $"Description {pi.Description} Assembly {pi.Assembly.FullName}"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}