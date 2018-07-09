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
    }
}
