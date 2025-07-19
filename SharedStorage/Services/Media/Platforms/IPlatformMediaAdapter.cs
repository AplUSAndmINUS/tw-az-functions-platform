using SharedStorage.Services;

namespace SharedStorage.Services.Media.Platforms;

/// <summary>
/// Interface for platform-specific media adapters
/// </summary>
public interface IPlatformMediaAdapter
{
    /// <summary>
    /// The name of the platform this adapter handles
    /// </summary>
    string PlatformName { get; }
    
    /// <summary>
    /// Supported media types for this platform
    /// </summary>
    IEnumerable<string> SupportedMediaTypes { get; }
    
    /// <summary>
    /// Maximum file size allowed by the platform (in bytes)
    /// </summary>
    long MaxFileSizeBytes { get; }
    
    /// <summary>
    /// Validates if media is suitable for this platform
    /// </summary>
    Task<bool> ValidateMediaAsync(Stream content, MediaMetadata metadata);
    
    /// <summary>
    /// Processes media for platform-specific requirements
    /// </summary>
    Task<MediaProcessingResult> ProcessMediaForPlatformAsync(Stream content, MediaMetadata metadata);
    
    /// <summary>
    /// Gets platform-specific media requirements and constraints
    /// </summary>
    Task<PlatformMediaRequirements> GetPlatformRequirementsAsync();
}

/// <summary>
/// Platform-specific media requirements and constraints
/// </summary>
public record PlatformMediaRequirements(
    int? MaxWidth = null,
    int? MaxHeight = null,
    int? MinWidth = null,
    int? MinHeight = null,
    TimeSpan? MaxDuration = null,
    IEnumerable<string>? AllowedFormats = null,
    string? AspectRatioRequirement = null
);