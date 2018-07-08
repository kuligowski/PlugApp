using System;
using MongoDB.Driver;

namespace DbProvider
{
    public static class MongoDbProvider
    {
        public static IMongoDatabase GetDatabaseHandle()
        {
            var connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            return client.GetDatabase("pluginApp");
        }
    }
}
