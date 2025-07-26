namespace SharedStorage.Models;

public class ImageEntity : MediaEntity
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ColorProfile { get; set; }
    public bool HasTransparency { get; set; }
    public string? Format { get; set; }

    public ImageEntity() : base() { }

    public ImageEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
}