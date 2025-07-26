namespace SharedStorage.Models;

public record MediaReference(string OriginalBlobName, string ThumbnailBlobName, string CdnUrl, string ThumbnailCdnUrl);