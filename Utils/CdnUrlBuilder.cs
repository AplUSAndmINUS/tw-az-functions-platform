namespace Utils;

using Utils.Constants;

public static class CdnUrlBuilder
{
  public static string ResolveCdnUrl(ContentSections section, AssetType? assetType, string blobName, string? paramsString = null, bool isMockStorage = false)
  {
    if (string.IsNullOrWhiteSpace(blobName))
      throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
    if (blobName.Contains("mock"))
      throw new ArgumentException("Blob name cannot be a mock blob.", nameof(blobName));

    if (isMockStorage)
      return $"{ApiUrls.MockCdnBlobStorageUrl}/{ContentNameResolver.GetBlobContainerName(section, assetType, true)}/{blobName}";

    string containerName = ContentNameResolver.GetBlobContainerName(section, assetType);

    // Build the CDN URL
    var cdnUrl = BuildCdnUrl(section, assetType, containerName, blobName);

    // Append query parameters if provided
    if (!string.IsNullOrWhiteSpace(paramsString))
      cdnUrl += $"?{paramsString.TrimStart('?')}";

    return cdnUrl;
  }

  public static string ResolveThumbnailUrl(ContentSections section, AssetType? assetType, string blobName, string? paramsString = null, bool isMockStorage = false)
  {
    if (string.IsNullOrWhiteSpace(blobName))
      throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));

    // Add thumbnail prefix if not already present
    var thumbnailBlobName = blobName.StartsWith("thumbnails/") ? blobName : $"thumbnails/{blobName}";
    
    return ResolveCdnUrl(section, assetType, thumbnailBlobName, paramsString, isMockStorage);
  }

  public static string ResolveAssetUrl(string containerName, string blobName, AssetType assetType, string? paramsString = null)
  {
    if (string.IsNullOrWhiteSpace(containerName))
      throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));
    if (string.IsNullOrWhiteSpace(blobName))
      throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));

    var cdnUrl = BuildAssetCdnUrl(containerName, blobName, assetType);

    if (!string.IsNullOrWhiteSpace(paramsString))
      cdnUrl += $"?{paramsString.TrimStart('?')}";

    return cdnUrl;
  }

  public static bool IsValidCdnUrl(string url)
  {
    if (string.IsNullOrWhiteSpace(url))
      return false;

    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
      return false;

    // Check if the URL is from one of our CDN endpoints
    var validEndpoints = new[]
    {
      ApiUrls.CdnEndpointDocuments,
      ApiUrls.CdnEndpointImages,
      ApiUrls.CdnEndpointVideos,
      ApiUrls.CdnEndpointMedia,
      ApiUrls.CdnEndpointMusic,
      ApiUrls.MockCdnBlobStorageUrl
    };

    return validEndpoints.Any(endpoint => url.StartsWith(endpoint));
  }

  private static string BuildCdnUrl(ContentSections section, AssetType? assetType, string containerName, string blobName)
  {
    return (section, assetType) switch
    {
      (ContentSections.Documents, _) => $"{ApiUrls.CdnEndpointDocuments}/{containerName}/{blobName}",
      (_, AssetType.Images) => $"{ApiUrls.CdnEndpointImages}/{containerName}/{blobName}",
      (_, AssetType.Video) => $"{ApiUrls.CdnEndpointVideos}/{containerName}/{blobName}",
      (_, AssetType.Media) => $"{ApiUrls.CdnEndpointMedia}/{containerName}/{blobName}",
      (ContentSections.Music, _) => $"{ApiUrls.CdnEndpointMusic}/{containerName}/{blobName}",
      _ => throw new ArgumentException($"No CDN endpoint configured for section {section} with asset type {assetType}", nameof(section))
    };
  }

  private static string BuildAssetCdnUrl(string containerName, string blobName, AssetType assetType)
  {
    return assetType switch
    {
      AssetType.Images => $"{ApiUrls.CdnEndpointImages}/{containerName}/{blobName}",
      AssetType.Video => $"{ApiUrls.CdnEndpointVideos}/{containerName}/{blobName}",
      AssetType.Media => $"{ApiUrls.CdnEndpointMedia}/{containerName}/{blobName}",
      AssetType.Documents => $"{ApiUrls.CdnEndpointDocuments}/{containerName}/{blobName}",
      _ => throw new ArgumentException($"No CDN endpoint configured for asset type {assetType}", nameof(assetType))
    };
  }
}