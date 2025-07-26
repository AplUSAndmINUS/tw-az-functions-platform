namespace SharedStorage.Models;

public class MediaItemDTO
{
    public string? Id { get; set; }
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
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public string? Metadata { get; set; }

    // Media type specific properties
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Duration { get; set; }
    public string? Format { get; set; }
    public int? Pages { get; set; }
}