using DaprSaga.Shared.Repositories;
using MongoDB.Driver;
using Saga.Coordinator.Services;
using Saga.Coordinator.Configuration;
using Serilog;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Saga.Coordinator.Models;
using DaprSaga.Shared.Extensions; // Added
using Nacos.AspNetCore.V2; // Added

var builder = WebApplication.CreateBuilder(args);

// Add Nacos Config
builder.AddNacosConfig(); // Added

// Add Config Upload Service
// builder.Services.AddHostedService<DaprSaga.Shared.Services.NacosConfigUploader>();


// Register ObjectSerializer to allow serializing complex objects like TransactionRequest in EventData
BsonSerializer.RegisterSerializer(new ObjectSerializer(type => 
    ObjectSerializer.DefaultAllowedTypes(type) || 
    (type.FullName?.StartsWith("Saga.Coordinator.Models") ?? false) ||
    (type.FullName?.StartsWith("DaprSaga.Shared.Models") ?? false)
));

// Explicitly register shared class maps to ensure discriminators match
if (!BsonClassMap.IsClassMapRegistered(typeof(DaprSaga.Shared.Models.SharedTransactionRequest)))
{
    BsonClassMap.RegisterClassMap<DaprSaga.Shared.Models.SharedTransactionRequest>();
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

// Add Nacos Service Registration
builder.Services.AddNacosService(builder.Configuration); // Added

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
builder.Services.AddScoped<BuyInSagaOrchestrator>();
builder.Services.AddScoped<CashOutSagaOrchestrator>();

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
