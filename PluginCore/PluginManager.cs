using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PluginShared;

namespace PluginCore
{
    public sealed class PluginManager : IPluginProvider
    {
        private static Lazy<ConcurrentDictionary<string, PluginDecorator>> plugins;

        private ConcurrentDictionary<Type, IPlugin> cache;
        private FileSystemWatcher watcher;
        private string pluginLoadPath;

        private void InitWatcher()
        {
            this.watcher = new FileSystemWatcher(this.pluginLoadPath, "*.dll");
            this.watcher.Created += new FileSystemEventHandler(WatcherOnCreated);
            this.watcher.EnableRaisingEvents = true;
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            this.LoadPlugin(e.FullPath.Trim().ToLower());
        }

        private void LoadPlugins()
        {
            var files = Directory.GetFiles(this.pluginLoadPath, 
                "*.dll",
                SearchOption.TopDirectoryOnly);

            Array.ForEach(files, LoadPlugin);
        }

        private void LoadPlugin(string path)
        {
            var pluginAssembly = Assembly.LoadFile(path);
            Array.ForEach(pluginAssembly.GetExportedTypes(), (type) => 
            {
                if(typeof(IPlugin).IsAssignableFrom(type))
                {
                    plugins.Value.TryAdd(path, new PluginDecorator(path, pluginAssembly, type));
                }
            });
        }

        public PluginManager()
        {
            this.pluginLoadPath = Path.Combine(
                                    Path.GetDirectoryName(
                                        Assembly.GetExecutingAssembly().Location), "Plugins").Trim().ToLower().ToString();
            this.InitWatcher();
            plugins = new Lazy<ConcurrentDictionary<string, PluginDecorator>>(
                                    () => new ConcurrentDictionary<string, PluginDecorator>());
            this.LoadPlugins();
            this.cache = new ConcurrentDictionary<Type, IPlugin>();
        }

        //Caching decision should be made elsewhere, ie. in config or plugin custom Attribute
        public IPlugin RequestPlugin(Type pluginType, bool singleton = false)
        {
            var pluginclass = GetPluginsInfo().FirstOrDefault(p => p.PluginType.FullName == pluginType.FullName);
            if (pluginclass == null)
            {
                //Log.Error($"Missing plugin type {pluginType.FullName}");
                return null;
            }
            
            if (singleton && this.cache.TryGetValue(pluginType, out IPlugin plugin))
            {
                return plugin;
            }

            plugin = (IPlugin)Activator.CreateInstance(pluginclass.PluginType);
            if (singleton)
            {
                this.cache.TryAdd(pluginType, plugin);
            }

            return plugin;
        }

        public string ExecutePlugin(Type pluginType, string input)
        {
            return this.RequestPlugin(pluginType)?.Execute(input);
        }

        //yield for auto dicovery
        public IEnumerable<PluginDecorator> GetPluginsInfo()
        {
            foreach (var v in plugins.Value.Values)
            {
                yield return v;
            }
        }

        public void Dispose()
        {
            this.watcher.Dispose();
        }
    }
}
