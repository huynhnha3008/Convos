using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using BusinessObject.Models;
namespace Repository.DatabaseSettings
{
    public class ConvosDbContext
    {
        private readonly IMongoDatabase _database;

        public ConvosDbContext(IOptions<DatabaseSettings> databaseSettings)
        {
            var client = new MongoClient(databaseSettings.Value.MongoDb);
            _database = client.GetDatabase(databaseSettings.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
        //ex:
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("Messages");
    }
}