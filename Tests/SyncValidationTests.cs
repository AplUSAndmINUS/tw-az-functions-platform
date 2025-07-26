using SharedStorage.Services;
using SharedStorage.Services.BaseServices;
using SharedStorage.Validators;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests;

public class SyncValidationTests
{
    [Fact]
    public void TableStorageService_ShouldCompileWithCorrectSyntax()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TableStorageService>>();
        
        // Act & Assert - Should not throw compilation errors
        var service = new TableStorageService("test-storage", mockLogger.Object);
        Assert.NotNull(service);
    }
    
    [Fact]
    public void AzureResourceValidator_ValidateTableName_ShouldNotThrowForValidName()
    {
        // Arrange
        var validTableName = "ValidTableName123";
        
        // Act & Assert - Should not throw
        TableNameValidator.ValidateTableName(validTableName);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("ab")]  // too short
    [InlineData("123abc")]  // starts with number
    public void TableNameValidator_ShouldThrowForInvalidNames(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => TableNameValidator.ValidateTableName(invalidName));
    }
}