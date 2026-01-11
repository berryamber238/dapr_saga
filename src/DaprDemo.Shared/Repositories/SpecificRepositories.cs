using Dapr.Client;
using DaprDemo.Shared.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DaprDemo.Shared.Repositories;

public class SagaTransactionRepository : MongoBaseRepository<SagaTransaction>
{
    public SagaTransactionRepository(IConfiguration configuration, DaprClient daprClient) 
        : base(configuration, "SagaTransactions", daprClient)
    {
        var indexKeys = Builders<SagaTransaction>.IndexKeys.Ascending(x => x.TransactionId);
        _collection.Indexes.CreateOne(new CreateIndexModel<SagaTransaction>(indexKeys));
    }

    public async Task<SagaTransaction> GetByTransactionIdAsync(string transactionId)
    {
        return await GetAsync(x => x.TransactionId == transactionId);
    }
}

public class BusinessDataRepository : MongoBaseRepository<BusinessData>
{
    public BusinessDataRepository(IConfiguration configuration, DaprClient daprClient) 
        : base(configuration, "BusinessData", daprClient)
    {
        var indexKeys = Builders<BusinessData>.IndexKeys.Ascending(x => x.BusinessId).Ascending(x => x.ServiceName);
        _collection.Indexes.CreateOne(new CreateIndexModel<BusinessData>(indexKeys));
    }
}

public class CompensateLogRepository : MongoBaseRepository<CompensateLog>
{
    public CompensateLogRepository(IConfiguration configuration, DaprClient daprClient) 
        : base(configuration, "CompensateLogs", daprClient)
    {
        var indexKeys = Builders<CompensateLog>.IndexKeys.Ascending(x => x.TransactionId);
        _collection.Indexes.CreateOne(new CreateIndexModel<CompensateLog>(indexKeys));
    }
}
