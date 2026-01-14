using DaprSaga.Shared.Extensions;
using Service.Notification.Middleware;
using Service.Notification.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddNacosConfig();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Nacos Service Registration
builder.Services.AddNacosService(builder.Configuration); // Added

// Register WebSocket Service
builder.Services.AddSingleton<WebSocketConnectionManager>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapControllers();

app.MapGet("/", () => "Notification Service Running");

app.Run();
