using Microsoft.Extensions.Logging;
using SharedStorage.Models;
using SharedStorage.Services.BaseServices;
using SharedStorage.Extensions;

namespace SharedStorage.Services.Content;

public interface IContentService<TEntity, TModel> 
    where TEntity : BaseContentEntity 
    where TModel : BaseContentModel
{
    Task<TModel?> GetAsync(string partitionKey, string rowKey);
    Task<IEnumerable<TModel>> GetAllAsync(string partitionKey);
    Task<TModel> CreateAsync(TModel model);
    Task<TModel> UpdateAsync(TModel model);
    Task DeleteAsync(string partitionKey, string rowKey);
    Task<bool> ExistsAsync(string partitionKey, string rowKey);
}

public class ContentService<TEntity, TModel> : IContentService<TEntity, TModel>
    where TEntity : BaseContentEntity, new()
    where TModel : BaseContentModel, new()
{
    private readonly ITableStorageService _tableStorageService;
    private readonly BaseContentMapper<TEntity, TModel> _mapper;
    private readonly ILogger<ContentService<TEntity, TModel>> _logger;
    private readonly string _tableName;

    public ContentService(
        ITableStorageService tableStorageService,
        BaseContentMapper<TEntity, TModel> mapper,
        ILogger<ContentService<TEntity, TModel>> logger,
        string tableName)
    {
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    public async Task<TModel?> GetAsync(string partitionKey, string rowKey)
    {
        try
        {
            var entity = await _tableStorageService.GetEntityAsync(_tableName, partitionKey, rowKey);
            if (entity == null)
                return null;

            var typedEntity = entity.ConvertTo<TEntity>();
            return _mapper.ToModel(typedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity with PartitionKey: {PartitionKey}, RowKey: {RowKey}", partitionKey, rowKey);
            throw;
        }
    }

    public async Task<IEnumerable<TModel>> GetAllAsync(string partitionKey)
    {
        try
        {
            var filter = $"PartitionKey eq '{partitionKey}'";
            var result = await _tableStorageService.GetEntitiesAsync(_tableName, filter);
            
            var models = new List<TModel>();
            foreach (var entity in result.Entities)
            {
                var typedEntity = entity.ConvertTo<TEntity>();
                models.Add(_mapper.ToModel(typedEntity));
            }

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities for PartitionKey: {PartitionKey}", partitionKey);
            throw;
        }
    }

    public async Task<TModel> CreateAsync(TModel model)
    {
        try
        {
            var entity = _mapper.ToEntity(model);
            if (string.IsNullOrEmpty(entity.RowKey))
            {
                entity.RowKey = Guid.NewGuid().ToString();
            }

            await _tableStorageService.UpsertEntityAsync(_tableName, entity);
            
            model.Id = entity.RowKey;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity");
            throw;
        }
    }

    public async Task<TModel> UpdateAsync(TModel model)
    {
        try
        {
            var entity = _mapper.ToEntity(model);
            await _tableStorageService.UpsertEntityAsync(_tableName, entity);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity with ID: {Id}", model.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string partitionKey, string rowKey)
    {
        try
        {
            await _tableStorageService.DeleteEntityAsync(_tableName, partitionKey, rowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity with PartitionKey: {PartitionKey}, RowKey: {RowKey}", partitionKey, rowKey);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string partitionKey, string rowKey)
    {
        try
        {
            var entity = await _tableStorageService.GetEntityAsync(_tableName, partitionKey, rowKey);
            return entity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if entity exists with PartitionKey: {PartitionKey}, RowKey: {RowKey}", partitionKey, rowKey);
            return false;
        }
    }
}