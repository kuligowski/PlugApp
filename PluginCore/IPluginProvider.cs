using System;
using System.Collections.Generic;
using PluginShared;

namespace PluginCore
{
    //TODO autofac injection for this
    public interface IPluginProvider
    {
        IPlugin RequestPlugin(Type pluginType);
        IEnumerable<PluginDecorator> GetPluginsInfo();
    }
}