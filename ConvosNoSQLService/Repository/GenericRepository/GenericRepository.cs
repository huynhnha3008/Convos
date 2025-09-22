using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.DatabaseSettings;

namespace Repository.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public GenericRepository(ConvosDbContext context, string collectionName)
    {
        _collection = context.GetCollection<T>(collectionName); 
    }

    public async Task<T> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        await _collection.ReplaceOneAsync(filter, entity);
        return entity;
    }

    public async Task<T> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        await _collection.DeleteOneAsync(filter);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
}