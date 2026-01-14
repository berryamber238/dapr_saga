using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nacos.V2;
using System.Text.Json;

namespace DaprSaga.Shared.Services;

public class NacosConfigUploader : IHostedService
{
    private readonly INacosConfigService _configService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NacosConfigUploader> _logger;

    public NacosConfigUploader(INacosConfigService configService, IConfiguration configuration, ILogger<NacosConfigUploader> logger)
    {
        _configService = configService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Only run if specifically enabled to avoid overwriting production config accidentally
        // or just try to seed if not exists
        var serviceName = _configuration["nacos:ServiceName"];
        var group = "DEFAULT_GROUP";

        if (string.IsNullOrEmpty(serviceName)) return;

        try 
        {
            // 1. Check if config already exists
            var existingConfig = await _configService.GetConfig(serviceName, group, 5000);
            
            if (string.IsNullOrWhiteSpace(existingConfig))
            {
                _logger.LogInformation("No existing Nacos config found for {ServiceName}. Uploading local settings...", serviceName);
                
                // 2. Extract relevant sections to upload
                // We don't want to upload the entire configuration because it contains environment variables and local paths
                // We'll focus on business settings like RetryPolicy
                
                var retryPolicy = new 
                {
                    RetryPolicy = new 
                    {
                        MaxRetries = _configuration.GetValue<int>("RetryPolicy:MaxRetries", 3),
                        InitialDelayMilliseconds = _configuration.GetValue<int>("RetryPolicy:InitialDelayMilliseconds", 1000)
                    }
                };

                var json = JsonSerializer.Serialize(retryPolicy, new JsonSerializerOptions { WriteIndented = true });
                
                // 3. Publish to Nacos
                var result = await _configService.PublishConfig(serviceName, group, json);
                
                if (result)
                {
                    _logger.LogInformation("Successfully uploaded initial configuration to Nacos for {ServiceName}", serviceName);
                }
                else
                {
                    _logger.LogWarning("Failed to upload configuration to Nacos for {ServiceName}", serviceName);
                }
            }
            else
            {
                _logger.LogInformation("Nacos configuration already exists for {ServiceName}. Skipping upload.", serviceName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking/uploading Nacos configuration");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
