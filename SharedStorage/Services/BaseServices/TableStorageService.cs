using Azure.Data.Tables;
using Azure.Identity;
using Azure;
using Microsoft.Extensions.Logging;
using SharedStorage.Validators;

namespace SharedStorage.Services.BaseServices;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageService> _logger;
    
    public TableStorageService(string storageAccountName, ILogger<TableStorageService> logger, string? connectionString = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(storageAccountName))
            throw new ArgumentException("Storage account name cannot be null or empty.", nameof(storageAccountName));

        _logger.LogInformation("Creating table client for {StorageAccount}", storageAccountName);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            // Use connection string authentication
            _logger.LogInformation("Using connection string authentication for table storage");
            _tableServiceClient = new TableServiceClient(connectionString);
        }
        else
        {
            // Use managed identity authentication
            _logger.LogInformation("Using managed identity authentication for table storage");
            var endpoint = $"https://{storageAccountName}.table.core.windows.net";
            
            // Modern approach: Include only the credentials we need instead of excluding ones we don't
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeAzureCliCredential = false,
                ExcludeManagedIdentityCredential = false,
                ExcludeEnvironmentCredential = false,
                // All other credential types are excluded by default
                DisableInstanceDiscovery = true // Improves performance by avoiding AAD instance discovery
            };
            
            _tableServiceClient = new TableServiceClient(new Uri(endpoint), new DefaultAzureCredential(options));
        }
        
        _logger.LogInformation("Table storage client created successfully");
    }

    public async Task<TableEntity?> GetEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        if (string.IsNullOrWhiteSpace(partitionKey))
            throw new ArgumentException("Partition key cannot be null or empty.", nameof(partitionKey));
        if (string.IsNullOrWhiteSpace(rowKey))
            throw new ArgumentException("Row key cannot be null or empty.", nameof(rowKey));

        // Validate table name and existence
        await AzureResourceValidator.ValidateAzureTableExistsAsync(_tableServiceClient, tableName);
        
        var client = _tableServiceClient.GetTableClient(tableName);

        try
        {
            _logger.LogInformation("Retrieving entity from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
            var response = await client.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);

            if (response.HasValue)
            {
                _logger.LogInformation("Entity retrieved successfully from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
            }
            else
            {
                _logger.LogInformation("Entity not found in table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
            }
            
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Entity not found in table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
            return null;
        }
    }

    public async Task<TablePageResult> GetEntitiesAsync(
        string tableName,
        string? filter = null,
        int pageSize = 25,
        string? continuationToken = null)
    {
        // Validate table name and existence
        await AzureResourceValidator.ValidateAzureTableExistsAsync(_tableServiceClient, tableName);
        var client = _tableServiceClient.GetTableClient(tableName);

        _logger.LogInformation("Retrieving entities from table {TableName} with filter {Filter} and page size {PageSize} token {Token}", tableName, filter, pageSize, continuationToken);

        try
        {
            await foreach (var page in client.QueryAsync<TableEntity>(filter).AsPages(continuationToken, pageSize))
            {
                _logger.LogInformation("Successfully retrieved {Count} entities from table {TableName}", page.Values.Count, tableName);
                return new TablePageResult(
                    Entities: page.Values,
                    ContinuationToken: page.ContinuationToken,
                    TotalCount: page.Values.Count,
                    HasMore: page.ContinuationToken != null
                );
            }

            return new TablePageResult(
                Entities: Enumerable.Empty<TableEntity>(),
                ContinuationToken: null,
                TotalCount: 0,
                HasMore: false
            );
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to retrieve entities from table {TableName}", tableName);
            throw;
        }
    }

    public async Task UpsertEntityAsync(string tableName, ITableEntity entity)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

        // Validate table name and existence
        await AzureResourceValidator.ValidateAzureTableExistsAsync(_tableServiceClient, tableName);
        
        var client = _tableServiceClient.GetTableClient(tableName);

        try
        {
            _logger.LogInformation("Upserting entity into table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, entity.PartitionKey, entity.RowKey);
            await client.UpsertEntityAsync(entity);
            _logger.LogInformation("Entity upserted successfully into table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, entity.PartitionKey, entity.RowKey);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to upsert entity into table {TableName}", tableName);
            throw;
        }
    }

    public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        if (string.IsNullOrWhiteSpace(partitionKey))
            throw new ArgumentException("Partition key cannot be null or empty.", nameof(partitionKey));
        if (string.IsNullOrWhiteSpace(rowKey))
            throw new ArgumentException("Row key cannot be null or empty.", nameof(rowKey));

        // Validate table name and existence
        await AzureResourceValidator.ValidateAzureTableExistsAsync(_tableServiceClient, tableName);
        
        var client = _tableServiceClient.GetTableClient(tableName);

        try
        {
            _logger.LogInformation("Deleting entity from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
            await client.DeleteEntityAsync(partitionKey, rowKey);
            _logger.LogInformation("Entity deleted successfully from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Entity not found in table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}", tableName, partitionKey, rowKey);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to delete entity from table {TableName}", tableName);
            throw;
        }
    }

    public TableClient GetTableClient(string tableName)
    {
        return _tableServiceClient.GetTableClient(tableName);
    }
}