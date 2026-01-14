using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nacos.AspNetCore.V2;
using Nacos.Microsoft.Extensions.Configuration; // Added

namespace DaprSaga.Shared.Extensions;

public static class NacosExtensions
{
    public static IServiceCollection AddNacosService(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Nacos Discovery Service
        // This will automatically read "nacos" section from configuration
        services.AddNacosAspNet(configuration);
        
        // Add Nacos Config Service explicitly for injection (INacosConfigService)
        // services.AddNacosV2Config(configuration);
        
        return services;
    }

    public static WebApplicationBuilder AddNacosConfig(this WebApplicationBuilder builder)
    {
        // Add Nacos Configuration Source
        // This reads "nacos" section for connection details
        var section = builder.Configuration.GetSection("nacos");
        // Only add Nacos Configuration provider if the section exists AND Listeners are defined
        if (section.Exists() && section.GetSection("Listeners").Exists())
        {
            // Try AddNacosConfiguration if V2 specific one is not found
            builder.Configuration.AddNacosV2Configuration(section);
        }
        return builder;
    }

    public static IConfigurationBuilder AddNacosConfig(this IConfigurationBuilder builder, IConfiguration configuration)
    {
        var section = configuration.GetSection("nacos");
        // Only add Nacos Configuration provider if the section exists AND Listeners are defined
        if (section.Exists() && section.GetSection("Listeners").Exists())
        {
            builder.AddNacosV2Configuration(section);
        }
        return builder;
    }
}
