using Microsoft.Extensions.DependencyInjection;
using SharedStorage.Models;
using SharedStorage.Services.Media.Handlers;

namespace SharedStorage.Services.Media;

public static class MediaServiceExtensions
{
    public static IServiceCollection AddMediaServices(this IServiceCollection services)
    {
        // Register core media services
        services.AddSingleton<IMediaService, MediaService>();
        services.AddSingleton<IMediaItemService, MediaItemService>();
        services.AddSingleton<IDocumentConversionService, DocumentConversionService>();
        
        // Register handlers
        services.AddSingleton<IDocumentHandler, DocumentHandler>();
        services.AddSingleton<IImageHandler, ImageHandler>();
        services.AddSingleton<IVideoHandler, VideoHandler>();
        
        // Register mappers
        services.AddSingleton<MediaItemMapper>();
        
        return services;
    }

    public static IServiceCollection AddMediaHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentHandler, DocumentHandler>();
        services.AddSingleton<IImageHandler, ImageHandler>();
        services.AddSingleton<IVideoHandler, VideoHandler>();
        
        return services;
    }

    public static IServiceCollection AddMediaMappers(this IServiceCollection services)
    {
        services.AddSingleton<MediaItemMapper>();
        
        return services;
    }
}