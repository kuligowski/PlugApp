using System;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections;  
using System.Collections.Generic;
using MongoDB.Driver;

namespace PluginLog
{
    public class Logger : ILogger
    {
        protected IMongoCollection<ActivityLog> DbLog
        {
            get
            {
                return mongoDb.GetCollection<ActivityLog>(nameof(ActivityLog));
            }
        }
        
        //TODO: wrap it into library if database will be used for tasks other than logging
        private IMongoDatabase mongoDb;
        public Logger(IMongoDatabase mongoDb)
        {
            this.mongoDb = mongoDb;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)  
        {
            var message = string.Empty;  
            if (formatter != null)
            {
                message = formatter(state, exception);  
            }
            else if (state != null)  
            {
                StringBuilder sb = new StringBuilder();
                var logValues = state as IReadOnlyList<KeyValuePair<string, object>>;
                foreach (var kvp in logValues)
                {
                    sb.Append(kvp.Key).Append(": "); 
                    sb.Append(kvp.Value);
                }

                message = sb.ToString();
            }
            else
            {
                message = "[Check formatting]";
            }

            var log = new ActivityLog
                {
                    Date = DateTime.UtcNow,
                    Level = logLevel.ToString(),
                    Message = message,
                    Thread = eventId.ToString(),
                    Exception = exception?.ToString()
                };

            DbLog.InsertOne(log);
        }

        //TODO: value should vary depending on log level value
        public bool IsEnabled(LogLevel logLevel)  
        {
            return true; 
        }

        public IDisposable BeginScope<TState>(TState state)  
        {  
            return null;  
        }
    }
}
