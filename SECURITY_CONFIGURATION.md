# Image Processing Security Configuration

This document describes the security enhancements implemented for SixLabors.ImageSharp image processing.

## Security Features Added

### 1. Memory Limits
- Configurable memory allocation limits to prevent DoS attacks through large image processing
- Default: 256 MB maximum memory allocation per image operation

### 2. File Size Validation
- Input stream size validation before processing
- Default: 50 MB maximum file size
- Prevents processing of extremely large files that could cause memory exhaustion

### 3. Image Dimension Validation
- Maximum width and height limits for processed images
- Default: 8192x8192 pixels maximum
- Prevents processing of images with malicious dimensions

### 4. Processing Timeouts
- Configurable timeout for image processing operations
- Default: 30 seconds per operation
- Prevents hanging operations from consuming resources indefinitely

### 5. EXIF Handling
- Secure handling of EXIF orientation data
- Option to automatically strip EXIF metadata (enabled by default)
- Prevents potential information disclosure through metadata

## Configuration

Add the following section to your `appsettings.json`:

```json
{
  "ImageSecurity": {
    "MaxImageWidth": 8192,
    "MaxImageHeight": 8192,
    "MaxMemoryMB": 256,
    "MaxFileSizeMB": 50,
    "ProcessingTimeoutSeconds": 30,
    "AutoOrient": true,
    "StripExifMetadata": true
  }
}
```

## Security Best Practices

1. **Resource Limits**: Keep memory and file size limits reasonable for your use case
2. **Timeout Configuration**: Set appropriate timeouts based on expected processing times
3. **Dimension Limits**: Set maximum dimensions that make sense for your application
4. **Metadata Handling**: Consider privacy implications when deciding whether to strip EXIF data
5. **Input Validation**: Always validate file types and sources before processing

## Implementation Details

The security enhancements are implemented through:
- `ImageSecurityConfiguration` class for centralized configuration
- Memory allocator configuration in SixLabors.ImageSharp
- Input validation in `ImageConversionService` and `ThumbnailService`
- Timeout handling using `CancellationTokenSource`
- Secure encoder configuration for metadata handling