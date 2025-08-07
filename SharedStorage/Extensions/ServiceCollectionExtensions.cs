using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedStorage.Services;
using SharedStorage.Services.BaseServices;
using SharedStorage.Services.Media.Handlers;
using SharedStorage.Services.Media;
using SharedStorage.Services.Media.Platforms;
using SharedStorage.Services.Email;
using SharedStorage.Environment;
using SharedStorage.Models;
using Utils;
using Utils.Services;
using Utils.Validation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;

namespace SharedStorage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure SixLabors.ImageSharp with security settings
        ConfigureImageSharpSecurity(configuration);
        
        // Register image security configuration
        services.Configure<ImageSecurityConfiguration>(options => 
        {
            configuration.GetSection("ImageSecurity").Bind(options);
        });
        
        var storageAccountName = configuration["StorageAccountName"]
            ?? System.Environment.GetEnvironmentVariable("StorageAccountName")
            ?? "{{DEFAULT_STORAGE_ACCOUNT_NAME}}"; // Default value if not set

        var cosmosAccountName = configuration["CosmosAccountName"]
            ?? System.Environment.GetEnvironmentVariable("CosmosAccountName")
            ?? "{{DEFAULT_COSMOS_DB_NAME}}"; // Default value if not set

        // Check if we should use connection string authentication
        var useConnectionString = configuration["USE_CONNECTION_STRING"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
            || System.Environment.GetEnvironmentVariable("USE_CONNECTION_STRING")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        string? connectionString = null;
        if (useConnectionString)
        {
            connectionString = configuration["AZURE_STORAGE_CONNECTION_STRING"]
                ?? System.Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("USE_CONNECTION_STRING is enabled but AZURE_STORAGE_CONNECTION_STRING is not provided.");
        }

        if (string.IsNullOrWhiteSpace(storageAccountName))
            throw new InvalidOperationException("Missing 'StorageAccountName' in environment or config.");

        if (string.IsNullOrWhiteSpace(cosmosAccountName))
            throw new InvalidOperationException("Missing 'CosmosAccountName' in environment or config.");

        // Register core services first so they can be injected into other services
        services.AddSingleton<IImageService, ImageConversionService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IDocumentConversionService, DocumentConversionService>();

        // Register specialized handlers
        services.AddSingleton<IDocumentHandler, DocumentHandler>();
        services.AddSingleton<IImageHandler, ImageHandler>();
        services.AddSingleton<IVideoHandler, VideoHandler>();

        // Register content reference services
        services.AddSingleton<IMediaServiceContentReferences, MediaServiceContentReferences>();

        // Register platform media adapters
        services.AddSingleton<IPlatformMediaAdapter, FacebookPlatformAdapter>();
        services.AddSingleton<IPlatformMediaAdapter, InstagramPlatformAdapter>();
        services.AddSingleton<IPlatformMediaAdapter, LinkedInPlatformAdapter>();
        services.AddSingleton<IPlatformMediaAdapter, PinterestPlatformAdapter>();
        services.AddSingleton<IPlatformMediaAdapter, TikTokPlatformAdapter>();
        services.AddSingleton<IPlatformMediaAdapter, YouTubePlatformAdapter>();


        // Register BlobStorageService
        services.AddSingleton<IBlobStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();
            var imageConversionService = sp.GetRequiredService<IImageService>();
            var thumbnailService = sp.GetRequiredService<IThumbnailService>();

            return new BlobStorageService(storageAccountName!, logger, imageConversionService, thumbnailService, connectionString);
        });

        // Register TableStorageService
        services.AddSingleton<ITableStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TableStorageService>>();
            return new TableStorageService(storageAccountName!, logger, connectionString);
        });

        // Register QueueStorageService
        services.AddSingleton<IQueueStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<QueueStorageService>>();
            return new QueueStorageService(storageAccountName!, logger, connectionString);
        });

        // Register CosmosDbService
        services.AddSingleton<ICosmosDbService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosDbService>>();
            return new CosmosDbService(cosmosAccountName!, logger);
        });

        // Register MediaHandler
        services.AddSingleton<IMediaHandler>(sp =>
        {
            var blobService = sp.GetRequiredService<IBlobStorageService>();
            var imageService = sp.GetRequiredService<IImageService>();
            var thumbnailService = sp.GetRequiredService<IThumbnailService>();
            return new SharedStorage.Services.MediaHandler(blobService, imageService, thumbnailService);
        });

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        // Register environment services
        services.AddScoped<IAppMode, DefaultAppMode>();

        // Register AppInsightsLogger
        services.AddSingleton(typeof(IAppInsightsLogger<>), typeof(AppInsightsLogger<>));

        return services;
    }

    /// <summary>
    /// Adds Key Vault services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddKeyVaultServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Key Vault Service
        services.AddSingleton<IKeyVaultService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KeyVaultService>>();
            var keyVaultUrl = configuration["AZURE_KEY_VAULT_URL"] 
                ?? System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL");
            
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
                throw new InvalidOperationException("Required Key Vault URL 'AZURE_KEY_VAULT_URL' is not set in configuration or environment variables.");
                
            var credential = new Azure.Identity.DefaultAzureCredential();
            return new KeyVaultService(keyVaultUrl, credential, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds API Key validation services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddApiKeyValidation(this IServiceCollection services, IConfiguration configuration)
    {
        // Register APIKeyValidator - use Key Vault validator if AZURE_KEY_VAULT_URL is configured
        var keyVaultUrl = configuration["AZURE_KEY_VAULT_URL"] 
            ?? System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL");
        
        if (!string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            // Ensure Key Vault services are registered
            services.AddKeyVaultServices(configuration);
            
            // Use Key Vault-based API key validator
            services.AddSingleton<IAPIKeyValidator, KeyVaultApiKeyValidator>();
        }
        else
        {
            // Fall back to simple API key validator
            services.AddSingleton<IAPIKeyValidator>(sp =>
            {
                var validApiKey = configuration["{{API_KEY_ENVIRONMENT_VARIABLE}}"]
                    ?? System.Environment.GetEnvironmentVariable("{{API_KEY_ENVIRONMENT_VARIABLE}}");
                if (string.IsNullOrWhiteSpace(validApiKey))
                    throw new InvalidOperationException("Missing {{API_KEY_ENVIRONMENT_VARIABLE}} in configuration.");

                return new ApiKeyValidator(validApiKey);
            });
        }

        return services;
    }
    
    /// <summary>
    /// Configures SixLabors.ImageSharp with security-hardened settings
    /// </summary>
    private static void ConfigureImageSharpSecurity(IConfiguration configuration)
    {
        // Get security configuration or use defaults
        var maxMemoryMB = configuration.GetSection("ImageSecurity:MaxMemoryMB").Get<int>() > 0 
            ? configuration.GetSection("ImageSecurity:MaxMemoryMB").Get<int>() 
            : 256;
        
        // Configure global ImageSharp memory limits to prevent DoS attacks
        Configuration.Default.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions()
        {
            MaximumPoolSizeMegabytes = maxMemoryMB
        });
    }
}