using DaprSaga.Shared.Repositories;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using DaprSaga.Shared.Extensions; // Added

var builder = WebApplication.CreateBuilder(args);

// Add Nacos Config
builder.AddNacosConfig(); // Added

// Register ObjectSerializer
BsonSerializer.RegisterSerializer(new ObjectSerializer(type => 
    ObjectSerializer.DefaultAllowedTypes(type) || 
    type.FullName.StartsWith("DaprSaga.Shared.Models")
));

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/", () => "Business Coordinator Running");

app.Run();
