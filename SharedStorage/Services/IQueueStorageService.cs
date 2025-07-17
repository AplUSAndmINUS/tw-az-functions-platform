using Azure.Storage.Queues;

namespace SharedStorage.Services;

public record QueueMessage(string MessageId, string MessageText, DateTimeOffset? InsertedOn = null, DateTimeOffset? ExpiresOn = null);

public record QueueMessageResult(string MessageId, string PopReceipt, string MessageText, DateTimeOffset? InsertedOn = null, DateTimeOffset? ExpiresOn = null, long DequeueCount = 0);

public interface IQueueStorageService
{
    QueueClient GetQueueClient(string queueName);
    Task<QueueMessage> SendMessageAsync(string queueName, string messageText);
    Task<QueueMessageResult?> ReceiveMessageAsync(string queueName);
    Task<IEnumerable<QueueMessageResult>> ReceiveMessagesAsync(string queueName, int maxMessages = 32);
    Task DeleteMessageAsync(string queueName, string messageId, string popReceipt);
    Task<QueueMessage> UpdateMessageAsync(string queueName, string messageId, string popReceipt, string messageText);
    Task<long> GetQueueLengthAsync(string queueName);
    Task ClearQueueAsync(string queueName);
}