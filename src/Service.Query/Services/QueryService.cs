using DaprSaga.Shared.Models;
using DaprSaga.Shared.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Service.Query.Services;

public interface IQueryService
{
    Task<object> GetTransactionDetailsAsync(string transactionId);
    Task<object> SearchTransactionsAsync(string? status, string? serviceName, int page, int pageSize);
}

public class QueryService : IQueryService
{
    private readonly SagaTransactionRepository _sagaRepo;
    private readonly BusinessDataRepository _businessRepo;
    private readonly CompensateLogRepository _compensateRepo;
    private readonly IMemoryCache _cache;

    public QueryService(
        SagaTransactionRepository sagaRepo,
        BusinessDataRepository businessRepo,
        CompensateLogRepository compensateRepo,
        IMemoryCache cache)
    {
        _sagaRepo = sagaRepo;
        _businessRepo = businessRepo;
        _compensateRepo = compensateRepo;
        _cache = cache;
    }

    public async Task<object> GetTransactionDetailsAsync(string transactionId)
    {
        var cacheKey = $"tx_details_{transactionId}";
        if (_cache.TryGetValue(cacheKey, out object? cachedResult))
        {
            return cachedResult!;
        }

        var transaction = await _sagaRepo.GetByTransactionIdAsync(transactionId);
        if (transaction == null) return null;

        var businessData = await _businessRepo.GetAllAsync(x => x.TransactionId == transactionId);
        var compensateLogs = await _compensateRepo.GetAllAsync(x => x.TransactionId == transactionId);

        var safeBusinessData = businessData.Select(b => new 
        {
            b.Id,
            b.BusinessId,
            b.TransactionId,
            b.ServiceName,
            Data = ConvertBsonToDotNet(b.Data),
            b.Status,
            b.CreateTime,
            b.UpdateTime
        }).ToList();

        var result = new
        {
            Transaction = transaction,
            BusinessData = safeBusinessData,
            CompensateLogs = compensateLogs
        };

        // Cache for short duration if transaction is completed
        if (transaction.Status == "Completed" || transaction.Status == "Compensated")
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        }

        return result;
    }

    private object? ConvertBsonToDotNet(object? data)
    {
        if (data == null) return null;
        
        if (data is BsonValue bsonValue)
        {
            // MapToDotNetValue converts BsonDocument to Dictionary<string, object>, BsonArray to List<object>, etc.
            return BsonTypeMapper.MapToDotNetValue(bsonValue);
        }

        return data;
    }

    public async Task<object> SearchTransactionsAsync(string? status, string? serviceName, int page, int pageSize)
    {
        // Using Repository's underlying collection for complex query is cleaner if we expose it or add specific method
        // For simplicity, we'll assume we can use GetAllAsync with predicate, but for paging we need more.
        // Let's implement paging manually on top of GetAllAsync or (better) in Repository.
        // Since we defined GetAllAsync returning List, let's assume small dataset for demo or extend repo.
        // NOTE: In production, Repository should expose IQueryable or PagedResult method.
        
        // Let's just filter by status for now as our BaseRepo is simple
        var all = await _sagaRepo.GetAllAsync(x => 
            (string.IsNullOrEmpty(status) || x.Status == status));

        var total = all.Count;
        var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Data = paged
        };
    }
}
