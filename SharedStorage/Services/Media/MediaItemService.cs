using Microsoft.Extensions.Logging;
using SharedStorage.Extensions;
using SharedStorage.Models;
using SharedStorage.Services.BaseServices;

namespace SharedStorage.Services.Media;

public interface IMediaItemService
{
    Task<MediaItemDTO?> GetMediaItemAsync(string id);
    Task<IEnumerable<MediaItemDTO>> GetMediaItemsAsync(string? category = null);
    Task<MediaItemDTO> CreateMediaItemAsync(MediaItemDTO mediaItem);
    Task<MediaItemDTO> UpdateMediaItemAsync(MediaItemDTO mediaItem);
    Task DeleteMediaItemAsync(string id);
    Task<bool> MediaItemExistsAsync(string id);
}

public class MediaItemService : IMediaItemService
{
    private readonly ITableStorageService _tableStorageService;
    private readonly MediaItemMapper _mapper;
    private readonly ILogger<MediaItemService> _logger;
    private const string TableName = "MediaItems";

    public MediaItemService(
        ITableStorageService tableStorageService,
        MediaItemMapper mapper,
        ILogger<MediaItemService> logger)
    {
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaItemDTO?> GetMediaItemAsync(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving media item with ID: {MediaItemId}", id);
            
            var entity = await _tableStorageService.GetEntityAsync(TableName, "media", id);
            if (entity == null)
            {
                _logger.LogWarning("Media item with ID {MediaItemId} not found", id);
                return null;
            }

            var mediaEntity = entity.ConvertTo<MediaEntity>();
            return _mapper.ToDTO(mediaEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media item with ID: {MediaItemId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<MediaItemDTO>> GetMediaItemsAsync(string? category = null)
    {
        try
        {
            _logger.LogInformation("Retrieving media items for category: {Category}", category ?? "all");
            
            var filter = "PartitionKey eq 'media'";
            if (!string.IsNullOrEmpty(category))
            {
                filter += $" and Category eq '{category}'";
            }

            var result = await _tableStorageService.GetEntitiesAsync(TableName, filter);
            
            var mediaItems = new List<MediaItemDTO>();
            foreach (var entity in result.Entities)
            {
                var mediaEntity = entity.ConvertTo<MediaEntity>();
                mediaItems.Add(_mapper.ToDTO(mediaEntity));
            }

            return mediaItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media items for category: {Category}", category);
            throw;
        }
    }

    public async Task<MediaItemDTO> CreateMediaItemAsync(MediaItemDTO mediaItem)
    {
        try
        {
            _logger.LogInformation("Creating new media item: {FileName}", mediaItem.OriginalFileName);
            
            if (string.IsNullOrEmpty(mediaItem.Id))
            {
                mediaItem.Id = Guid.NewGuid().ToString();
            }

            var model = new MediaItemModel
            {
                Id = mediaItem.Id,
                OriginalFileName = mediaItem.OriginalFileName,
                BlobName = mediaItem.BlobName,
                ThumbnailBlobName = mediaItem.ThumbnailBlobName,
                ContainerName = mediaItem.ContainerName,
                FileSize = mediaItem.FileSize,
                MimeType = mediaItem.MimeType,
                CdnUrl = mediaItem.CdnUrl,
                ThumbnailCdnUrl = mediaItem.ThumbnailCdnUrl,
                ProcessingStatus = mediaItem.ProcessingStatus,
                ProcessingError = mediaItem.ProcessingError,
                ProcessedDate = mediaItem.ProcessedDate,
                Checksum = mediaItem.Checksum,
                Title = mediaItem.Title,
                Description = mediaItem.Description,
                Tags = mediaItem.Tags,
                CreatedBy = mediaItem.CreatedBy,
                ModifiedBy = mediaItem.ModifiedBy,
                IsActive = mediaItem.IsActive,
                Category = mediaItem.Category,
                SortOrder = mediaItem.SortOrder,
                Metadata = mediaItem.Metadata
            };

            var entity = _mapper.ToEntity(model);
            entity.PartitionKey = "media";
            entity.RowKey = mediaItem.Id;

            await _tableStorageService.UpsertEntityAsync(TableName, entity);
            
            return _mapper.ToDTO(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating media item: {FileName}", mediaItem.OriginalFileName);
            throw;
        }
    }

    public async Task<MediaItemDTO> UpdateMediaItemAsync(MediaItemDTO mediaItem)
    {
        try
        {
            _logger.LogInformation("Updating media item with ID: {MediaItemId}", mediaItem.Id);
            
            if (string.IsNullOrEmpty(mediaItem.Id))
            {
                throw new ArgumentException("Media item ID cannot be null or empty", nameof(mediaItem));
            }

            var model = new MediaItemModel
            {
                Id = mediaItem.Id,
                OriginalFileName = mediaItem.OriginalFileName,
                BlobName = mediaItem.BlobName,
                ThumbnailBlobName = mediaItem.ThumbnailBlobName,
                ContainerName = mediaItem.ContainerName,
                FileSize = mediaItem.FileSize,
                MimeType = mediaItem.MimeType,
                CdnUrl = mediaItem.CdnUrl,
                ThumbnailCdnUrl = mediaItem.ThumbnailCdnUrl,
                ProcessingStatus = mediaItem.ProcessingStatus,
                ProcessingError = mediaItem.ProcessingError,
                ProcessedDate = mediaItem.ProcessedDate,
                Checksum = mediaItem.Checksum,
                Title = mediaItem.Title,
                Description = mediaItem.Description,
                Tags = mediaItem.Tags,
                CreatedBy = mediaItem.CreatedBy,
                ModifiedBy = mediaItem.ModifiedBy,
                IsActive = mediaItem.IsActive,
                Category = mediaItem.Category,
                SortOrder = mediaItem.SortOrder,
                Metadata = mediaItem.Metadata
            };

            var entity = _mapper.ToEntity(model);
            entity.PartitionKey = "media";
            entity.RowKey = mediaItem.Id;

            await _tableStorageService.UpsertEntityAsync(TableName, entity);
            
            return _mapper.ToDTO(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media item with ID: {MediaItemId}", mediaItem.Id);
            throw;
        }
    }

    public async Task DeleteMediaItemAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting media item with ID: {MediaItemId}", id);
            await _tableStorageService.DeleteEntityAsync(TableName, "media", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media item with ID: {MediaItemId}", id);
            throw;
        }
    }

    public async Task<bool> MediaItemExistsAsync(string id)
    {
        try
        {
            var entity = await _tableStorageService.GetEntityAsync(TableName, "media", id);
            return entity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if media item exists with ID: {MediaItemId}", id);
            return false;
        }
    }
}