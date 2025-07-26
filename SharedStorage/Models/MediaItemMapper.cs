namespace SharedStorage.Models;

public class MediaItemMapper : BaseContentMapper<MediaEntity, MediaItemModel>
{
    public override MediaItemModel ToModel(MediaEntity entity)
    {
        var model = new MediaItemModel();
        MapBaseProperties(entity, model);
        
        model.OriginalFileName = entity.OriginalFileName;
        model.BlobName = entity.BlobName;
        model.ThumbnailBlobName = entity.ThumbnailBlobName;
        model.ContainerName = entity.ContainerName;
        model.FileSize = entity.FileSize;
        model.MimeType = entity.MimeType;
        model.CdnUrl = entity.CdnUrl;
        model.ThumbnailCdnUrl = entity.ThumbnailCdnUrl;
        model.ProcessingStatus = entity.ProcessingStatus;
        model.ProcessingError = entity.ProcessingError;
        model.ProcessedDate = entity.ProcessedDate;
        model.Checksum = entity.Checksum;

        return model;
    }

    public override MediaEntity ToEntity(MediaItemModel model)
    {
        var entity = new MediaEntity();
        MapBaseProperties(model, entity);
        
        entity.OriginalFileName = model.OriginalFileName;
        entity.BlobName = model.BlobName;
        entity.ThumbnailBlobName = model.ThumbnailBlobName;
        entity.ContainerName = model.ContainerName;
        entity.FileSize = model.FileSize;
        entity.MimeType = model.MimeType;
        entity.CdnUrl = model.CdnUrl;
        entity.ThumbnailCdnUrl = model.ThumbnailCdnUrl;
        entity.ProcessingStatus = model.ProcessingStatus;
        entity.ProcessingError = model.ProcessingError;
        entity.ProcessedDate = model.ProcessedDate;
        entity.Checksum = model.Checksum;

        return entity;
    }

    public MediaItemDTO ToDTO(MediaEntity entity)
    {
        var dto = new MediaItemDTO
        {
            Id = entity.RowKey,
            OriginalFileName = entity.OriginalFileName,
            BlobName = entity.BlobName,
            ThumbnailBlobName = entity.ThumbnailBlobName,
            ContainerName = entity.ContainerName,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            CdnUrl = entity.CdnUrl,
            ThumbnailCdnUrl = entity.ThumbnailCdnUrl,
            ProcessingStatus = entity.ProcessingStatus,
            ProcessingError = entity.ProcessingError,
            ProcessedDate = entity.ProcessedDate,
            Checksum = entity.Checksum,
            Title = entity.Title,
            Description = entity.Description,
            Tags = entity.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
            IsActive = entity.IsActive,
            Category = entity.Category,
            SortOrder = entity.SortOrder,
            Metadata = entity.Metadata
        };

        // Add type-specific properties for specialized entities
        if (entity is ImageEntity imageEntity)
        {
            dto.Width = imageEntity.Width;
            dto.Height = imageEntity.Height;
            dto.Format = imageEntity.Format;
        }
        else if (entity is VideoEntity videoEntity)
        {
            dto.Width = videoEntity.Width;
            dto.Height = videoEntity.Height;
            dto.Duration = videoEntity.Duration;
            dto.Format = videoEntity.Format;
        }

        return dto;
    }
}