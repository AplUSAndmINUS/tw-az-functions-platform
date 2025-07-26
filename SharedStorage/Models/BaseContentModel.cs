namespace SharedStorage.Models;

public abstract class BaseContentModel
{
    public string? Id { get; set; }
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
}