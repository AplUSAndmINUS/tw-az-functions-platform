using SharedStorage.Validators;
using Xunit;

namespace Tests;

public class QueueNameValidatorTests
{
    [Theory]
    [InlineData("valid-queue-name")]
    [InlineData("queue123")]
    [InlineData("test-queue")]
    [InlineData("a-b")]
    [InlineData("123")]
    [InlineData("queue-with-multiple-hyphens")]
    public void ValidateQueueName_WithValidNames_ShouldNotThrow(string queueName)
    {
        // Act & Assert
        QueueNameValidator.ValidateQueueName(queueName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateQueueName_WithNullOrEmptyName_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void ValidateQueueName_WithTooShortName_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Fact]
    public void ValidateQueueName_WithTooLongName_ShouldThrow()
    {
        // Arrange
        var longName = new string('a', 64); // 64 characters

        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(longName));
    }

    [Theory]
    [InlineData("-invalid")]
    [InlineData("invalid-")]
    [InlineData("-")]
    public void ValidateQueueName_WithInvalidStartOrEnd_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Theory]
    [InlineData("queue--name")]
    [InlineData("test--queue")]
    [InlineData("a--b")]
    public void ValidateQueueName_WithConsecutiveHyphens_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Theory]
    [InlineData("Queue")]
    [InlineData("QUEUE")]
    [InlineData("Test-Queue")]
    [InlineData("queueName")]
    public void ValidateQueueName_WithUppercaseLetters_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Theory]
    [InlineData("queue_name")]
    [InlineData("queue.name")]
    [InlineData("queue@name")]
    [InlineData("queue#name")]
    [InlineData("queue name")]
    public void ValidateQueueName_WithInvalidCharacters_ShouldThrow(string queueName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueueNameValidator.ValidateQueueName(queueName));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("a1b")]
    [InlineData("123")]
    [InlineData("queue-name-123")]
    public void ValidateQueueName_WithValidLengthAndFormat_ShouldNotThrow(string queueName)
    {
        // Act & Assert
        QueueNameValidator.ValidateQueueName(queueName);
    }

    [Fact]
    public void ValidateQueueName_WithExact63Characters_ShouldNotThrow()
    {
        // Arrange
        var validName = new string('a', 61) + "1b"; // 63 characters ending with alphanumeric

        // Act & Assert
        QueueNameValidator.ValidateQueueName(validName);
    }

    [Fact]
    public void ValidateQueueName_WithExact3Characters_ShouldNotThrow()
    {
        // Arrange
        var validName = "a1b"; // 3 characters

        // Act & Assert
        QueueNameValidator.ValidateQueueName(validName);
    }
}