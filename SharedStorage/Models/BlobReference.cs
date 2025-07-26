namespace SharedStorage.Models;

public record BlobReference(string FileName, string CdnUrl, DateTimeOffset? LastModified = null, long? Size = null)
{
    public BlobReference(string fileName, string cdnUrl)
        : this(fileName, cdnUrl, null, null) { }
}