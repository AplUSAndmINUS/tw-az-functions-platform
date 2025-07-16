using Microsoft.Azure.Cosmos;

namespace SharedStorage.Services;

public record CosmosDbPageResult<T>(
    IEnumerable<T> Items,
    string? ContinuationToken,
    int TotalCount,
    bool HasMore
);

public interface ICosmosDbService
{
    Database GetDatabase(string databaseName);
    Container GetContainer(string databaseName, string containerName);
    Task<T?> GetItemAsync<T>(string databaseName, string containerName, string id, string partitionKey);
    Task<CosmosDbPageResult<T>> GetItemsAsync<T>(string databaseName, string containerName, string? query = null, int pageSize = 25, string? continuationToken = null);
    Task<T> UpsertItemAsync<T>(string databaseName, string containerName, T item, string partitionKey);
    Task DeleteItemAsync(string databaseName, string containerName, string id, string partitionKey);
}