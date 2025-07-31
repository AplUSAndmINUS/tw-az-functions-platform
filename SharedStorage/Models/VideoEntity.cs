namespace SharedStorage.Models;

public class VideoEntity : MediaEntity
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Duration { get; set; }
    public string? Format { get; set; }
    public string? Codec { get; set; }
    public int? Bitrate { get; set; }
    public double? FrameRate { get; set; }

    public VideoEntity() : base() { }

    public VideoEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
}