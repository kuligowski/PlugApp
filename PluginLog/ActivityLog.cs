using System;
using MongoDB.Bson;

namespace PluginLog
{
    public class ActivityLog
    {
        public ObjectId Id { get; set; }

        public DateTime Date { get; set; }

        public string Thread { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }
    }
}