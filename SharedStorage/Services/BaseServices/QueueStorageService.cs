using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Identity;
using Azure;
using Microsoft.Extensions.Logging;
using SharedStorage.Validators;

namespace SharedStorage.Services;

public class QueueStorageService : IQueueStorageService
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ILogger<QueueStorageService> _logger;

    public QueueStorageService(
        string storageAccountName,
        ILogger<QueueStorageService> logger,
        string? connectionString = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(storageAccountName))
            throw new ArgumentException("Storage account name cannot be null or empty.", nameof(storageAccountName));

        _logger.LogInformation("Creating queue storage client for {StorageAccount}", storageAccountName);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            // Use connection string authentication
            _logger.LogInformation("Using connection string authentication for queue storage");
            _queueServiceClient = new QueueServiceClient(connectionString);
        }
        else
        {
            // Use managed identity authentication
            _logger.LogInformation("Using managed identity authentication for queue storage");
            var endpoint = $"https://{storageAccountName}.queue.core.windows.net";
            
            var options = new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeAzureCliCredential = false,
                ExcludeManagedIdentityCredential = false,
                ExcludeEnvironmentCredential = false,
                DisableInstanceDiscovery = true
            };
            
            _queueServiceClient = new QueueServiceClient(new Uri(endpoint), new DefaultAzureCredential(options));
        }
        
        _logger.LogInformation("Queue storage client created successfully");
    }

    public async Task<QueueMessage> SendMessageAsync(string queueName, string messageText)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
        if (string.IsNullOrWhiteSpace(messageText))
            throw new ArgumentException("Message text cannot be null or empty.", nameof(messageText));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync();

        try
        {
            _logger.LogInformation("Sending message to queue {QueueName}", queueName);
            var response = await queueClient.SendMessageAsync(messageText);
            _logger.LogInformation("Message sent successfully to queue {QueueName} with ID {MessageId}", queueName, response.Value.MessageId);
            
            return new QueueMessage(
                response.Value.MessageId,
                messageText,
                response.Value.TimeNextVisible,
                response.Value.ExpirationTime
            );
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to send message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task<QueueMessageResult?> ReceiveMessageAsync(string queueName)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Receiving message from queue {QueueName}", queueName);
            var response = await queueClient.ReceiveMessageAsync();
            
            if (response.Value != null)
            {
                var message = response.Value;
                _logger.LogInformation("Message received successfully from queue {QueueName} with ID {MessageId}", queueName, message.MessageId);
                
                return new QueueMessageResult(
                    message.MessageId,
                    message.PopReceipt,
                    message.MessageText,
                    message.InsertedOn,
                    message.ExpiresOn,
                    message.DequeueCount
                );
            }
            else
            {
                _logger.LogInformation("No messages available in queue {QueueName}", queueName);
                return null;
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to receive message from queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task<IEnumerable<QueueMessageResult>> ReceiveMessagesAsync(string queueName, int maxMessages = 32)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
        if (maxMessages < 1 || maxMessages > 32)
            throw new ArgumentException("Max messages must be between 1 and 32.", nameof(maxMessages));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Receiving up to {MaxMessages} messages from queue {QueueName}", maxMessages, queueName);
            var response = await queueClient.ReceiveMessagesAsync(maxMessages);
            
            var messages = response.Value.Select(message => new QueueMessageResult(
                message.MessageId,
                message.PopReceipt,
                message.MessageText,
                message.InsertedOn,
                message.ExpiresOn,
                message.DequeueCount
            )).ToList();
            
            _logger.LogInformation("Successfully received {Count} messages from queue {QueueName}", messages.Count, queueName);
            return messages;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to receive messages from queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        if (string.IsNullOrWhiteSpace(popReceipt))
            throw new ArgumentException("Pop receipt cannot be null or empty.", nameof(popReceipt));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Deleting message {MessageId} from queue {QueueName}", messageId, queueName);
            await queueClient.DeleteMessageAsync(messageId, popReceipt);
            _logger.LogInformation("Message {MessageId} deleted successfully from queue {QueueName}", messageId, queueName);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Message {MessageId} not found in queue {QueueName}, nothing to delete", messageId, queueName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to delete message {MessageId} from queue {QueueName}", messageId, queueName);
            throw;
        }
    }

    public async Task<QueueMessage> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string messageText)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
        if (string.IsNullOrWhiteSpace(messageId))
            throw new ArgumentException("Message ID cannot be null or empty.", nameof(messageId));
        if (string.IsNullOrWhiteSpace(popReceipt))
            throw new ArgumentException("Pop receipt cannot be null or empty.", nameof(popReceipt));
        if (string.IsNullOrWhiteSpace(messageText))
            throw new ArgumentException("Message text cannot be null or empty.", nameof(messageText));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Updating message {MessageId} in queue {QueueName}", messageId, queueName);
            var response = await queueClient.UpdateMessageAsync(messageId, popReceipt, messageText);
            _logger.LogInformation("Message {MessageId} updated successfully in queue {QueueName}", messageId, queueName);
            
            return new QueueMessage(
                messageId,
                messageText,
                null, // InsertedOn is not returned in update response
                null  // ExpiresOn is not returned in update response
            );
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to update message {MessageId} in queue {QueueName}", messageId, queueName);
            throw;
        }
    }

    public async Task<long> GetQueueLengthAsync(string queueName)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Getting queue length for {QueueName}", queueName);
            var response = await queueClient.GetPropertiesAsync();
            var length = response.Value.ApproximateMessagesCount;
            _logger.LogInformation("Queue {QueueName} has approximately {Length} messages", queueName, length);
            return length;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to get queue length for {QueueName}", queueName);
            throw;
        }
    }

    public async Task ClearQueueAsync(string queueName)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

        // Validate queue name
        await AzureResourceValidator.ValidateAzureQueueExistsAsync(_queueServiceClient, queueName);
        
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        try
        {
            _logger.LogInformation("Clearing queue {QueueName}", queueName);
            await queueClient.ClearMessagesAsync();
            _logger.LogInformation("Queue {QueueName} cleared successfully", queueName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to clear queue {QueueName}", queueName);
            throw;
        }
    }

    public QueueClient GetQueueClient(string queueName)
    {
        return _queueServiceClient.GetQueueClient(queueName);
    }
}