using System.Collections.Concurrent;
using Azure.Data.Tables;
using Azure;

namespace SharedStorage.Services.Media;

/// <summary>
/// Manages content references for media services to track file usage and dependencies
/// </summary>
public interface IMediaServiceContentReferences
{
    Task<bool> AddReferenceAsync(string mediaId, string referenceId, string referenceType);
    Task<bool> RemoveReferenceAsync(string mediaId, string referenceId);
    Task<IEnumerable<ContentReference>> GetReferencesAsync(string mediaId);
    Task<bool> HasReferencesAsync(string mediaId);
    Task<bool> CanDeleteMediaAsync(string mediaId);
    Task<int> GetReferenceCountAsync(string mediaId);
    Task<IEnumerable<string>> GetOrphanedMediaAsync();
}

public record ContentReference(
    string ReferenceId,
    string ReferenceType,
    DateTime CreatedAt,
    string? AdditionalData = null
);

public class MediaServiceContentReferences : IMediaServiceContentReferences
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ConcurrentDictionary<string, HashSet<ContentReference>> _cache;
    private readonly string _tableName = "MediaContentReferences";

    public MediaServiceContentReferences(ITableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        _cache = new ConcurrentDictionary<string, HashSet<ContentReference>>();
    }

    public async Task<bool> AddReferenceAsync(string mediaId, string referenceId, string referenceType)
    {
        if (string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(referenceId) || string.IsNullOrEmpty(referenceType))
            return false;

        var reference = new ContentReference(referenceId, referenceType, DateTime.UtcNow);
        
        // Update cache
        _cache.AddOrUpdate(mediaId, 
            new HashSet<ContentReference> { reference },
            (key, existing) => { existing.Add(reference); return existing; });

        // Persist to storage
        var entity = new MediaReferenceEntity
        {
            PartitionKey = mediaId,
            RowKey = $"{referenceType}_{referenceId}",
            MediaId = mediaId,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _tableStorageService.UpsertEntityAsync(_tableName, entity);
            return true;
        }
        catch
        {
            // Remove from cache if storage failed
            if (_cache.TryGetValue(mediaId, out var refs))
            {
                refs.Remove(reference);
                if (!refs.Any())
                {
                    _cache.TryRemove(mediaId, out _);
                }
            }
            return false;
        }
    }

    public async Task<bool> RemoveReferenceAsync(string mediaId, string referenceId)
    {
        if (string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(referenceId))
            return false;

        // Update cache
        var removed = false;
        if (_cache.TryGetValue(mediaId, out var refs))
        {
            var refToRemove = refs.FirstOrDefault(r => r.ReferenceId == referenceId);
            if (refToRemove != null)
            {
                refs.Remove(refToRemove);
                removed = true;
                if (!refs.Any())
                {
                    _cache.TryRemove(mediaId, out _);
                }
            }
        }

        // Remove from storage
        try
        {
            var result = await _tableStorageService.GetEntitiesAsync(_tableName, 
                filter: $"PartitionKey eq '{mediaId}' and ReferenceId eq '{referenceId}'");
            
            foreach (var entity in result.Entities)
            {
                await _tableStorageService.DeleteEntityAsync(_tableName, entity.PartitionKey, entity.RowKey);
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<ContentReference>> GetReferencesAsync(string mediaId)
    {
        if (string.IsNullOrEmpty(mediaId))
            return Enumerable.Empty<ContentReference>();

        // Check cache first
        if (_cache.TryGetValue(mediaId, out var cachedRefs))
        {
            return cachedRefs;
        }

        // Load from storage
        try
        {
            var result = await _tableStorageService.GetEntitiesAsync(_tableName, 
                filter: $"PartitionKey eq '{mediaId}'");
            
            var references = result.Entities.Select(e => new ContentReference(
                e.GetString("ReferenceId") ?? "",
                e.GetString("ReferenceType") ?? "",
                e.GetDateTime("CreatedAt") ?? DateTime.MinValue,
                e.GetString("AdditionalData")
            )).ToList();

            // Update cache
            _cache.TryAdd(mediaId, new HashSet<ContentReference>(references));
            
            return references;
        }
        catch
        {
            return Enumerable.Empty<ContentReference>();
        }
    }

    public async Task<bool> HasReferencesAsync(string mediaId)
    {
        var references = await GetReferencesAsync(mediaId);
        return references.Any();
    }

    public async Task<bool> CanDeleteMediaAsync(string mediaId)
    {
        return !await HasReferencesAsync(mediaId);
    }

    public async Task<int> GetReferenceCountAsync(string mediaId)
    {
        var references = await GetReferencesAsync(mediaId);
        return references.Count();
    }

    public Task<IEnumerable<string>> GetOrphanedMediaAsync()
    {
        try
        {
            // This would need to be implemented based on your specific storage setup
            // For now, returning empty list as we'd need to compare media storage with references
            return Task.FromResult(Enumerable.Empty<string>());
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }
}

internal class MediaReferenceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AdditionalData { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}