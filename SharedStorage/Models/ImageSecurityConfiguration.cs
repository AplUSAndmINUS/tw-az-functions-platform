namespace SharedStorage.Models;

/// <summary>
/// Security configuration for image processing operations
/// </summary>
public class ImageSecurityConfiguration
{
    /// <summary>
    /// Maximum width allowed for image processing (default: 8192 pixels)
    /// </summary>
    public int MaxImageWidth { get; set; } = 8192;
    
    /// <summary>
    /// Maximum height allowed for image processing (default: 8192 pixels)
    /// </summary>
    public int MaxImageHeight { get; set; } = 8192;
    
    /// <summary>
    /// Maximum memory allocation for image processing in MB (default: 256 MB)
    /// </summary>
    public int MaxMemoryMB { get; set; } = 256;
    
    /// <summary>
    /// Maximum file size allowed for processing in MB (default: 50 MB)
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 50;
    
    /// <summary>
    /// Timeout for image processing operations in seconds (default: 30 seconds)
    /// </summary>
    public int ProcessingTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Whether to automatically orient images based on EXIF data (default: true)
    /// </summary>
    public bool AutoOrient { get; set; } = true;
    
    /// <summary>
    /// Whether to strip EXIF metadata from processed images (default: true for security)
    /// </summary>
    public bool StripExifMetadata { get; set; } = true;
}