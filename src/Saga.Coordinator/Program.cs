using DaprDemo.Shared.Repositories;
using MongoDB.Driver;
using Saga.Coordinator.Services;
using Saga.Coordinator.Configuration;
using Serilog;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Saga.Coordinator.Models;

var builder = WebApplication.CreateBuilder(args);

// Register ObjectSerializer to allow serializing complex objects like TransactionRequest in EventData
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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Bind RetryOptions
builder.Services.Configure<RetryOptions>(builder.Configuration.GetSection("RetryPolicy"));

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Register Repository
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});
builder.Services.AddScoped<EventStoreRepository>();
builder.Services.AddScoped<SagaTransactionRepository>();

builder.Services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.MapGet("/", () => "Saga Coordinator Running");

app.Run();
