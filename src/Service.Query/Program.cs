using DaprDemo.Shared.Repositories;
using MongoDB.Driver;
using Service.Query.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

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

// Register Repositories
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});
builder.Services.AddScoped<SagaTransactionRepository>();
builder.Services.AddScoped<BusinessDataRepository>();
builder.Services.AddScoped<CompensateLogRepository>();

// Register Service
builder.Services.AddScoped<IQueryService, QueryService>();

var app = builder.Build();

// Enable Swagger in ALL environments for demo purposes
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.MapGet("/", () => "Query Service Running");

app.Run();
