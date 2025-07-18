namespace Utils.Constants;

public static class ApiUrls
{
  // Base URL for the API
  public const string MockBaseDevUrl = "https://mock-dev-api.{{YOUR_DOMAIN}}";
  public const string MockBaseTestUrl = "https://mock-tst-api.{{YOUR_DOMAIN}}";
  public const string BaseUrl = "https://api.{{YOUR_DOMAIN}}";

  // CDN endpoints for blob content types
  public const string CdnEndpointDocuments = "https://documents.{{YOUR_DOMAIN}}";
  public const string CdnEndpointImages = "https://images.{{YOUR_DOMAIN}}";
  public const string CdnEndpointMusic = "https://music.{{YOUR_DOMAIN}}";
  public const string CdnEndpointVideos = "https://videos.{{YOUR_DOMAIN}}";
  public const string CdnEndpointMedia = "https://media.{{YOUR_DOMAIN}}";

  // Mock Azure storage URL which point directly to Azure Blob Storage
  public const string MockCdnBlobStorageUrl = "https://{{STORAGE_ACCOUNT_NAME}}.blob.core.windows.net";
  public const string MockCdnTableStorageUrl = "https://{{STORAGE_ACCOUNT_NAME}}.table.core.windows.net";
}