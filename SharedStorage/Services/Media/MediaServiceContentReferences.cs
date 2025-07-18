using Utils.Constants;

namespace SharedStorage.Services.Media;

public static class MediaServiceContentReferences
{
    public static class ContentTypes
    {
        public static class Images
        {
            public const string Jpeg = "image/jpeg";
            public const string Jpg = "image/jpg";
            public const string Png = "image/png";
            public const string Gif = "image/gif";
            public const string Bmp = "image/bmp";
            public const string WebP = "image/webp";
        }

        public static class Videos
        {
            public const string Mp4 = "video/mp4";
            public const string Avi = "video/avi";
            public const string Mov = "video/mov";
            public const string Wmv = "video/wmv";
            public const string Flv = "video/flv";
            public const string WebM = "video/webm";
        }

        public static class Documents
        {
            public const string Pdf = "application/pdf";
            public const string Doc = "application/msword";
            public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            public const string Xls = "application/vnd.ms-excel";
            public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public const string Ppt = "application/vnd.ms-powerpoint";
            public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            public const string Txt = "text/plain";
            public const string Csv = "text/csv";
            public const string Rtf = "application/rtf";
        }
    }

    public static class FileExtensions
    {
        public static class Images
        {
            public const string Jpg = ".jpg";
            public const string Jpeg = ".jpeg";
            public const string Png = ".png";
            public const string Gif = ".gif";
            public const string Bmp = ".bmp";
            public const string WebP = ".webp";
        }

        public static class Videos
        {
            public const string Mp4 = ".mp4";
            public const string Avi = ".avi";
            public const string Mov = ".mov";
            public const string Wmv = ".wmv";
            public const string Flv = ".flv";
            public const string WebM = ".webm";
        }

        public static class Documents
        {
            public const string Pdf = ".pdf";
            public const string Doc = ".doc";
            public const string Docx = ".docx";
            public const string Xls = ".xls";
            public const string Xlsx = ".xlsx";
            public const string Ppt = ".ppt";
            public const string Pptx = ".pptx";
            public const string Txt = ".txt";
            public const string Csv = ".csv";
            public const string Rtf = ".rtf";
        }
    }

    public static class MediaCategories
    {
        public static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ContentTypes.Images.Jpeg,
            ContentTypes.Images.Jpg,
            ContentTypes.Images.Png,
            ContentTypes.Images.Gif,
            ContentTypes.Images.Bmp,
            ContentTypes.Images.WebP
        };

        public static readonly HashSet<string> VideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ContentTypes.Videos.Mp4,
            ContentTypes.Videos.Avi,
            ContentTypes.Videos.Mov,
            ContentTypes.Videos.Wmv,
            ContentTypes.Videos.Flv,
            ContentTypes.Videos.WebM
        };

        public static readonly HashSet<string> DocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ContentTypes.Documents.Pdf,
            ContentTypes.Documents.Doc,
            ContentTypes.Documents.Docx,
            ContentTypes.Documents.Xls,
            ContentTypes.Documents.Xlsx,
            ContentTypes.Documents.Ppt,
            ContentTypes.Documents.Pptx,
            ContentTypes.Documents.Txt,
            ContentTypes.Documents.Csv,
            ContentTypes.Documents.Rtf
        };

        public static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Images.Jpg,
            FileExtensions.Images.Jpeg,
            FileExtensions.Images.Png,
            FileExtensions.Images.Gif,
            FileExtensions.Images.Bmp,
            FileExtensions.Images.WebP
        };

        public static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Videos.Mp4,
            FileExtensions.Videos.Avi,
            FileExtensions.Videos.Mov,
            FileExtensions.Videos.Wmv,
            FileExtensions.Videos.Flv,
            FileExtensions.Videos.WebM
        };

        public static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            FileExtensions.Documents.Pdf,
            FileExtensions.Documents.Doc,
            FileExtensions.Documents.Docx,
            FileExtensions.Documents.Xls,
            FileExtensions.Documents.Xlsx,
            FileExtensions.Documents.Ppt,
            FileExtensions.Documents.Pptx,
            FileExtensions.Documents.Txt,
            FileExtensions.Documents.Csv,
            FileExtensions.Documents.Rtf
        };
    }

    public static AssetType GetAssetTypeFromContentType(string contentType)
    {
        if (MediaCategories.ImageContentTypes.Contains(contentType))
            return AssetType.Images;
        
        if (MediaCategories.VideoContentTypes.Contains(contentType))
            return AssetType.Video;
        
        if (MediaCategories.DocumentContentTypes.Contains(contentType))
            return AssetType.Data;
        
        return AssetType.Media;
    }

    public static AssetType GetAssetTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        
        if (MediaCategories.ImageExtensions.Contains(extension))
            return AssetType.Images;
        
        if (MediaCategories.VideoExtensions.Contains(extension))
            return AssetType.Video;
        
        if (MediaCategories.DocumentExtensions.Contains(extension))
            return AssetType.Data;
        
        return AssetType.Media;
    }

    public static bool IsMediaFile(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName);
        
        return MediaCategories.ImageContentTypes.Contains(contentType) ||
               MediaCategories.VideoContentTypes.Contains(contentType) ||
               MediaCategories.ImageExtensions.Contains(extension) ||
               MediaCategories.VideoExtensions.Contains(extension);
    }
}