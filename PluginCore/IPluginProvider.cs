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
        
        IEnumerable<PluginDecorator> GetPluginsInfo();

        string ExecutePlugin(Type pluginType, string input);
    }
}