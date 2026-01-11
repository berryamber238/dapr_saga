using Dapr.Client;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace DaprDemo.Shared.Repositories;

public interface IRepository<T> where T : class
{
    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
    Task CreateAsync(T entity);
    Task UpdateAsync(Expression<Func<T, bool>> predicate, T entity);
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
}

public class MongoBaseRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly DaprClient _daprClient;

    public MongoBaseRepository(IConfiguration configuration, string collectionName, DaprClient daprClient)
    {
        _daprClient = daprClient;
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(Expression<Func<T, bool>> predicate, T entity)
    {
        await _collection.ReplaceOneAsync(predicate, entity);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await _collection.DeleteOneAsync(predicate);
    }
}
