using Utils.Validation;

namespace Tests;

public class ApiKeyValidatorTests
{
    [Fact]
    public void IsValid_WithValidApiKey_ReturnsTrue()
    {
        // Arrange
        var validApiKey = "valid-api-key-with-32-or-more-characters";
        var validator = new ApiKeyValidator(validApiKey);

        // Act
        var result = validator.IsValid(validApiKey);

        // Assert
        Assert.True(result);
        Assert.Null(validator.GetErrorMessage());
    }

    [Fact]
    public void IsValid_WithNullApiKey_ReturnsFalse()
    {
        // Arrange
        var validApiKey = "valid-api-key-with-32-or-more-characters";
        var validator = new ApiKeyValidator(validApiKey);

        // Act
        var result = validator.IsValid(null!);

        // Assert
        Assert.False(result);
        Assert.Equal("API key cannot be null or empty.", validator.GetErrorMessage());
    }

    [Fact]
    public void IsValid_WithEmptyApiKey_ReturnsFalse()
    {
        // Arrange
        var validApiKey = "valid-api-key-with-32-or-more-characters";
        var validator = new ApiKeyValidator(validApiKey);

        // Act
        var result = validator.IsValid("");

        // Assert
        Assert.False(result);
        Assert.Equal("API key cannot be null or empty.", validator.GetErrorMessage());
    }

    [Fact]
    public void IsValid_WithInvalidApiKey_ReturnsFalse()
    {
        // Arrange
        var validApiKey = "valid-api-key-with-32-or-more-characters";
        var validator = new ApiKeyValidator(validApiKey);

        // Act
        var result = validator.IsValid("invalid-key");

        // Assert
        Assert.False(result);
        Assert.Equal("Invalid API key.", validator.GetErrorMessage());
    }
}