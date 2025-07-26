namespace SharedStorage.Models;

public abstract class BaseContentMapper<TEntity, TModel> 
    where TEntity : BaseContentEntity 
    where TModel : BaseContentModel
{
    protected virtual void MapBaseProperties(TEntity entity, TModel model)
    {
        model.Id = entity.RowKey;
        model.Title = entity.Title;
        model.Description = entity.Description;
        model.Tags = entity.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        model.CreatedDate = entity.CreatedDate;
        model.ModifiedDate = entity.ModifiedDate;
        model.CreatedBy = entity.CreatedBy;
        model.ModifiedBy = entity.ModifiedBy;
        model.IsActive = entity.IsActive;
        model.Category = entity.Category;
        model.SortOrder = entity.SortOrder;
        model.Metadata = entity.Metadata;
    }

    protected virtual void MapBaseProperties(TModel model, TEntity entity)
    {
        entity.RowKey = model.Id ?? string.Empty;
        entity.Title = model.Title;
        entity.Description = model.Description;
        entity.Tags = model.Tags?.Length > 0 ? string.Join(",", model.Tags) : null;
        entity.CreatedBy = model.CreatedBy;
        entity.ModifiedBy = model.ModifiedBy;
        entity.IsActive = model.IsActive;
        entity.Category = model.Category;
        entity.SortOrder = model.SortOrder;
        entity.Metadata = model.Metadata;
        entity.ModifiedDate = DateTime.UtcNow;
        
        // Only set CreatedDate if it's a new entity
        if (entity.CreatedDate == default)
        {
            entity.CreatedDate = DateTime.UtcNow;
        }
    }

    public abstract TModel ToModel(TEntity entity);
    public abstract TEntity ToEntity(TModel model);
}