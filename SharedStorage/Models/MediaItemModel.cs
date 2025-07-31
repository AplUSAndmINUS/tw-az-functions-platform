namespace SharedStorage.Models;

public class MediaItemModel : BaseContentModel
{
    public string? OriginalFileName { get; set; }
    public string? BlobName { get; set; }
    public string? ThumbnailBlobName { get; set; }
    public string? ContainerName { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? CdnUrl { get; set; }
    public string? ThumbnailCdnUrl { get; set; }
    public string? ProcessingStatus { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? Checksum { get; set; }
}