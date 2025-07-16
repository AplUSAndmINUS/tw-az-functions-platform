using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedStorage.Services;
using SharedStorage.Services.Media.Handlers;

namespace SharedStorage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        var storageAccountName = configuration["StorageAccountName"]
            ?? System.Environment.GetEnvironmentVariable("StorageAccountName")
            ?? "aztwwebsitestorage"; // Default value if not set

        var cosmosAccountName = configuration["CosmosAccountName"]
            ?? System.Environment.GetEnvironmentVariable("CosmosAccountName")
            ?? "aztwwebsitecosmosdb"; // Default value if not set

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

        // Register BlobStorageService
        services.AddSingleton<IBlobStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();
            var imageConversionService = sp.GetRequiredService<IImageService>();
            var thumbnailService = sp.GetRequiredService<IThumbnailService>();

            return new BlobStorageService(storageAccountName!, logger, imageConversionService, thumbnailService);
        });

        // Register TableStorageService
        services.AddSingleton<ITableStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TableStorageService>>();
            return new TableStorageService(storageAccountName!, logger);
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

        return services;
    }
}