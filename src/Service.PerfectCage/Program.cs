using DaprSaga.Shared.Repositories;
using DaprSaga.Shared.Extensions; // Added
using MongoDB.Driver;
using Service.PerfectCage.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Nacos Config
builder.AddNacosConfig(); // Added

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Nacos Service Registration
builder.Services.AddNacosService(builder.Configuration); // Added

builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});

builder.Services.AddScoped<BusinessDataRepository>();
builder.Services.AddScoped<EventStoreRepository>();
builder.Services.AddScoped<CompensateLogRepository>();
builder.Services.AddScoped<IPerfectCageService, PerfectCageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.MapGet("/", () => "Perfect Cage Service Running");

app.Run();
