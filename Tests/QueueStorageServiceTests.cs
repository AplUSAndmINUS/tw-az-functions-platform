using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharedStorage.Services;
using Xunit;

namespace Tests;

public class QueueStorageServiceTests
{
    private readonly ILogger<QueueStorageService> _logger;
    private readonly string _testStorageAccountName = "teststorage";
    private readonly string _testConnectionString = "UseDevelopmentStorage=true";

    public QueueStorageServiceTests()
    {
        _logger = NullLogger<QueueStorageService>.Instance;
    }

    [Fact]
    public void QueueStorageService_Constructor_WithManagedIdentity_ShouldSucceed()
    {
        // Arrange & Act
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void QueueStorageService_Constructor_WithConnectionString_ShouldSucceed()
    {
        // Arrange & Act
        var service = new QueueStorageService(_testStorageAccountName, _logger, _testConnectionString);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void QueueStorageService_Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueueStorageService(_testStorageAccountName, null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void QueueStorageService_Constructor_WithInvalidStorageAccountName_ShouldThrow(string storageAccountName)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new QueueStorageService(storageAccountName, _logger));
    }

    [Fact]
    public void QueueStorageService_GetQueueClient_ShouldReturnValidClient()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);
        var queueName = "test-queue";

        // Act
        var queueClient = service.GetQueueClient(queueName);

        // Assert
        Assert.NotNull(queueClient);
        Assert.Equal(queueName, queueClient.Name);
    }

    [Theory]
    [InlineData("valid-queue-name")]
    [InlineData("queue123")]
    [InlineData("test-queue-name")]
    public void QueueStorageService_GetQueueClient_WithValidNames_ShouldReturnClient(string queueName)
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act
        var queueClient = service.GetQueueClient(queueName);

        // Assert
        Assert.NotNull(queueClient);
        Assert.Equal(queueName, queueClient.Name);
    }

    [Fact]
    public async Task QueueStorageService_SendMessageAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendMessageAsync(null!, "test message"));
    }

    [Fact]
    public async Task QueueStorageService_SendMessageAsync_WithEmptyQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendMessageAsync("", "test message"));
    }

    [Fact]
    public async Task QueueStorageService_SendMessageAsync_WithNullMessage_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendMessageAsync("test-queue", null!));
    }

    [Fact]
    public async Task QueueStorageService_SendMessageAsync_WithEmptyMessage_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendMessageAsync("test-queue", ""));
    }

    [Fact]
    public async Task QueueStorageService_ReceiveMessageAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ReceiveMessageAsync(null!));
    }

    [Fact]
    public async Task QueueStorageService_ReceiveMessageAsync_WithEmptyQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ReceiveMessageAsync(""));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(33)]
    [InlineData(-1)]
    public async Task QueueStorageService_ReceiveMessagesAsync_WithInvalidMaxMessages_ShouldThrow(int maxMessages)
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ReceiveMessagesAsync("test-queue", maxMessages));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(32)]
    public void QueueStorageService_ReceiveMessagesAsync_WithValidMaxMessages_ShouldNotThrowArgumentException(int maxMessages)
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert - This should not throw ArgumentException for maxMessages validation
        // (It will throw other exceptions when trying to connect to Azure, but not ArgumentException)
        try
        {
            service.ReceiveMessagesAsync("test-queue", maxMessages).Wait();
        }
        catch (ArgumentException)
        {
            // This should not happen - ArgumentException means our validation is wrong
            throw;
        }
        catch (Exception)
        {
            // Any other exception (Azure connection errors, etc.) is expected and acceptable
            // We only care that ArgumentException is not thrown for valid maxMessages values
        }
    }

    [Fact]
    public async Task QueueStorageService_DeleteMessageAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteMessageAsync(null!, "messageId", "popReceipt"));
    }

    [Fact]
    public async Task QueueStorageService_DeleteMessageAsync_WithNullMessageId_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteMessageAsync("test-queue", null!, "popReceipt"));
    }

    [Fact]
    public async Task QueueStorageService_DeleteMessageAsync_WithNullPopReceipt_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteMessageAsync("test-queue", "messageId", null!));
    }

    [Fact]
    public async Task QueueStorageService_UpdateMessageAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateMessageAsync(null!, "messageId", "popReceipt", "newMessage"));
    }

    [Fact]
    public async Task QueueStorageService_UpdateMessageAsync_WithNullMessageId_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateMessageAsync("test-queue", null!, "popReceipt", "newMessage"));
    }

    [Fact]
    public async Task QueueStorageService_UpdateMessageAsync_WithNullPopReceipt_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateMessageAsync("test-queue", "messageId", null!, "newMessage"));
    }

    [Fact]
    public async Task QueueStorageService_UpdateMessageAsync_WithNullMessage_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateMessageAsync("test-queue", "messageId", "popReceipt", null!));
    }

    [Fact]
    public async Task QueueStorageService_GetQueueLengthAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetQueueLengthAsync(null!));
    }

    [Fact]
    public async Task QueueStorageService_ClearQueueAsync_WithNullQueueName_ShouldThrow()
    {
        // Arrange
        var service = new QueueStorageService(_testStorageAccountName, _logger);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.ClearQueueAsync(null!));
    }
}