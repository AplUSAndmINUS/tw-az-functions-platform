using SharedStorage.Models;

namespace SharedStorage.Extensions;

public static class MediaReferenceExtensions
{
    public static string GetDisplayName(this MediaReference reference)
    {
        return Path.GetFileNameWithoutExtension(reference.OriginalBlobName);
    }

    public static string GetFileExtension(this MediaReference reference)
    {
        return Path.GetExtension(reference.OriginalBlobName).ToLowerInvariant();
    }

    public static bool IsImage(this MediaReference reference)
    {
        var extension = reference.GetFileExtension();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg";
    }

    public static bool IsVideo(this MediaReference reference)
    {
        var extension = reference.GetFileExtension();
        return extension is ".mp4" or ".avi" or ".mov" or ".mkv" or ".webm" or ".flv" or ".wmv";
    }

    public static bool IsDocument(this MediaReference reference)
    {
        var extension = reference.GetFileExtension();
        return extension is ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt";
    }

    public static string GetThumbnailUrl(this MediaReference reference)
    {
        return reference.ThumbnailCdnUrl ?? reference.CdnUrl;
    }
}