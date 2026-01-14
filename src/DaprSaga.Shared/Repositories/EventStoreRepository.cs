using DaprSaga.Shared.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DaprSaga.Shared.Repositories;

public class EventStoreRepository
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<EventRecord> _eventCollection;
    private readonly IMongoCollection<OutboxMessage> _outboxCollection;

    public EventStoreRepository(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDB:DatabaseName"];

        _mongoClient = mongoClient;
        _database = _mongoClient.GetDatabase(databaseName);
        _eventCollection = _database.GetCollection<EventRecord>("EventStore");
        _outboxCollection = _database.GetCollection<OutboxMessage>("Outbox");
    }

    public async Task SaveEventWithOutboxAsync(EventRecord eventRecord, OutboxMessage outboxMessage)
    {
        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await _eventCollection.InsertOneAsync(session, eventRecord);
            await _outboxCollection.InsertOneAsync(session, outboxMessage);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task CreateEventAsync(IClientSessionHandle session, EventRecord record)
    {
        await _eventCollection.InsertOneAsync(session, record);
    }

    public async Task CreateOutboxAsync(IClientSessionHandle session, OutboxMessage message)
    {
        await _outboxCollection.InsertOneAsync(session, message);
    }
}
