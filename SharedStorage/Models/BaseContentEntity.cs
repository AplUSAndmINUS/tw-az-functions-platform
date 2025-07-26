using Azure;
using Azure.Data.Tables;

namespace SharedStorage.Models;

public abstract class BaseContentEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public string? Metadata { get; set; }

    protected BaseContentEntity()
    {
        CreatedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    protected BaseContentEntity(string partitionKey, string rowKey) : this()
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }
}