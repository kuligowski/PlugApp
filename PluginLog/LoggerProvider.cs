using System;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace PluginLog
{
    public class LoggerProvider : ILoggerProvider
    {
        private IMongoDatabase mongoDb;
        public LoggerProvider(IMongoDatabase mongoDb)
        {
            this.mongoDb = mongoDb;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this.mongoDb);
        }

        public void Dispose()
        {
        
        }
    }
}
