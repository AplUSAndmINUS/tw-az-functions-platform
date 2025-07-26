using SharedStorage.Models;

namespace SharedStorage.Extensions;

public static class MediaExtensions
{
    public static string GetFileExtension(this MediaEntity media)
    {
        return Path.GetExtension(media.OriginalFileName ?? "").ToLowerInvariant();
    }

    public static bool IsImage(this MediaEntity media)
    {
        var extension = media.GetFileExtension();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg";
    }

    public static bool IsVideo(this MediaEntity media)
    {
        var extension = media.GetFileExtension();
        return extension is ".mp4" or ".avi" or ".mov" or ".mkv" or ".webm" or ".flv" or ".wmv";
    }

    public static bool IsDocument(this MediaEntity media)
    {
        var extension = media.GetFileExtension();
        return extension is ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt";
    }

    public static string GetMimeType(this MediaEntity media)
    {
        var extension = media.GetFileExtension();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".flv" => "video/x-flv",
            ".wmv" => "video/x-ms-wmv",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}