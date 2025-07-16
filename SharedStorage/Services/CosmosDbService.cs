using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using SharedStorage.Validators;

namespace SharedStorage.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(
        string cosmosAccountName,
        ILogger<CosmosDbService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Creating CosmosDB client for {CosmosAccount}", cosmosAccountName ?? "unknown");

        var endpoint = $"https://{cosmosAccountName}.documents.azure.com:443/";
        _cosmosClient = new CosmosClient(endpoint, new DefaultAzureCredential());
        _logger.LogInformation("CosmosDB client created for {Endpoint}", endpoint);
    }

    public Database GetDatabase(string databaseName)
    {
        _logger.LogInformation("Getting database {DatabaseName}", databaseName);
        return _cosmosClient.GetDatabase(databaseName);
    }

    public Container GetContainer(string databaseName, string containerName)
    {
        _logger.LogInformation("Getting container {ContainerName} from database {DatabaseName}", containerName, databaseName);
        return _cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<T?> GetItemAsync<T>(string databaseName, string containerName, string id, string partitionKey)
    {
        try
        {
            _logger.LogInformation("Retrieving item {Id} from container {ContainerName} in database {DatabaseName}", id, containerName, databaseName);
            
            var container = GetContainer(databaseName, containerName);
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            
            _logger.LogInformation("Successfully retrieved item {Id}", id);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Item {Id} not found in container {ContainerName}", id, containerName);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {Id} from container {ContainerName}", id, containerName);
            throw;
        }
    }

    public async Task<CosmosDbPageResult<T>> GetItemsAsync<T>(string databaseName, string containerName, string? query = null, int pageSize = 25, string? continuationToken = null)
    {
        try
        {
            _logger.LogInformation("Retrieving items from container {ContainerName} in database {DatabaseName}", containerName, databaseName);
            
            var container = GetContainer(databaseName, containerName);
            
            // Use default query if none provided
            var queryText = query ?? "SELECT * FROM c";
            
            var queryDefinition = new QueryDefinition(queryText);
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = pageSize
            };

            var iterator = container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
            
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var items = response.ToList();
                
                _logger.LogInformation("Retrieved {Count} items from container {ContainerName}", items.Count, containerName);
                
                return new CosmosDbPageResult<T>(
                    items,
                    response.ContinuationToken,
                    items.Count,
                    iterator.HasMoreResults
                );
            }
            
            return new CosmosDbPageResult<T>(
                Enumerable.Empty<T>(),
                null,
                0,
                false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items from container {ContainerName}", containerName);
            throw;
        }
    }

    public async Task<T> UpsertItemAsync<T>(string databaseName, string containerName, T item, string partitionKey)
    {
        try
        {
            _logger.LogInformation("Upserting item to container {ContainerName} in database {DatabaseName}", containerName, databaseName);
            
            var container = GetContainer(databaseName, containerName);
            var response = await container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey));
            
            _logger.LogInformation("Successfully upserted item to container {ContainerName}", containerName);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting item to container {ContainerName}", containerName);
            throw;
        }
    }

    public async Task DeleteItemAsync(string databaseName, string containerName, string id, string partitionKey)
    {
        try
        {
            _logger.LogInformation("Deleting item {Id} from container {ContainerName} in database {DatabaseName}", id, containerName, databaseName);
            
            var container = GetContainer(databaseName, containerName);
            await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
            
            _logger.LogInformation("Successfully deleted item {Id} from container {ContainerName}", id, containerName);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Item {Id} not found for deletion in container {ContainerName}", id, containerName);
            // Don't throw - item already doesn't exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {Id} from container {ContainerName}", id, containerName);
            throw;
        }
    }
}