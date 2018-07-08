using System;
using System.Text;
using Microsoft.Extensions.Logging; 
using PluginLog;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class ExtensionMethods
    {
        public static ILoggerFactory AddLogger(this ILoggerFactory factory, IMongoDatabase mongoDb)
        {
            factory.AddProvider(new LoggerProvider(mongoDb));
            return factory;
        }

        // public static string PluginsToString(this IEnumerable<PluginDecorator> pluginsInfo)
        // {
        //     // StringBuilder sb = new StringBuilder();
        //     // foreach (var pi in pluginsInfo)
        //     // {
        //     //     sb.
        //     //     var info = $"Description {pi.Description} Assembly {pi.Assembly.FullName}";
        //     // }

        //     // return sb.ToString();
        //     return string.Join("\n", pluginsInfo.Select(pi => $"Description {pi.Description} Assembly {pi.Assembly.FullName}"));
        // }
    }
}
