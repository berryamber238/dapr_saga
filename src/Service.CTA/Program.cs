using DaprDemo.Shared.Repositories;
using MongoDB.Driver;
using Service.CTA.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});

builder.Services.AddScoped<BusinessDataRepository>();
builder.Services.AddScoped<EventStoreRepository>();
builder.Services.AddScoped<CompensateLogRepository>();
builder.Services.AddScoped<ICtaService, CtaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.MapGet("/", () => "CTA Service Running");

app.Run();
