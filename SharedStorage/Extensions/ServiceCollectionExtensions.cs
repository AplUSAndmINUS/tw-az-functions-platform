using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedStorage.Services;
using SharedStorage.Services.Media.Handlers;
using SharedStorage.Services.Media;
using SharedStorage.Services.Email;
using SharedStorage.Environment;
using Utils;

namespace SharedStorage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
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
            return new MediaHandler(blobService, imageService, thumbnailService);
        });

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        // Register environment services
        services.AddScoped<IAppMode, DefaultAppMode>();

        // Register AppInsightsLogger
        services.AddSingleton(typeof(IAppInsightsLogger<>), typeof(AppInsightsLogger<>));

        return services;
    }
}