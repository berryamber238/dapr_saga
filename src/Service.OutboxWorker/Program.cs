using Dapr.Client;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Service.OutboxWorker;

var builder = Host.CreateApplicationBuilder(args);

// Register ObjectSerializer to handle dynamic/complex payloads in OutboxMessage
BsonSerializer.RegisterSerializer(new ObjectSerializer(type => 
    ObjectSerializer.DefaultAllowedTypes(type) || 
    type.FullName.StartsWith("Saga.Coordinator.Models") ||
    type.FullName.StartsWith("DaprDemo.Shared.Models")
));

// Explicitly register shared class maps to ensure discriminators match
if (!BsonClassMap.IsClassMapRegistered(typeof(DaprDemo.Shared.Models.SharedTransactionRequest)))
{
    BsonClassMap.RegisterClassMap<DaprDemo.Shared.Models.SharedTransactionRequest>();
}

builder.Services.AddSingleton<DaprClient>(new DaprClientBuilder().Build());
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
