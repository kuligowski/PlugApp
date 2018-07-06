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
    public class PluginManager : IDisposable, IPluginProvider
    {
        private static readonly Lazy<PluginManager> instance = new Lazy<PluginManager>(() => new PluginManager());
        private static Lazy<ConcurrentDictionary<string, PluginDecorator>> plugins;
        private FileSystemWatcher watcher;
        private string pluginLoadPath;

        private PluginManager()
        {
            this.pluginLoadPath = Path.Combine(
                                    Path.GetDirectoryName(
                                        Assembly.GetExecutingAssembly().Location), "Plugins").Trim().ToLower().ToString();
            this.InitWatcher();
            plugins = new Lazy<ConcurrentDictionary<string, PluginDecorator>>(
                                    () => new ConcurrentDictionary<string, PluginDecorator>());
            this.LoadPlugins();
        }

        private void InitWatcher()
        {
            this.watcher = new FileSystemWatcher(this.pluginLoadPath, "*.dll");
            this.watcher.Created += new FileSystemEventHandler(WatcherOnCreated);
            this.watcher.EnableRaisingEvents = true;
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            PluginManager.Instance.LoadPlugin(e.FullPath.Trim().ToLower());
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

        public static PluginManager Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public IPlugin RequestPlugin(Type pluginType)
        {
            var pluginclass = GetPluginsInfo().FirstOrDefault(p => p.PluginType.FullName == pluginType.FullName);
            return (IPlugin)Activator.CreateInstance(pluginclass.PluginType);
        }

        public string ExecutePlugin(Type pluginType, string input)
        {
            return this.RequestPlugin(pluginType)?.Execute(input);
        }

        //IEnumerable for auto dicovery
        public IEnumerable<PluginDecorator> GetPluginsInfo()
        {
            foreach (var p in plugins.Value.Values)
            {
                yield return p;
            }
        }

        public void Dispose()
        {
            this.watcher.Dispose();
        }
    }
}