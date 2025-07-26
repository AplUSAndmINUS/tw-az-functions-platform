using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SharedStorage.Extensions;
using Utils.Services;
using Utils.Validation;
using Xunit;

namespace Tests;

public class KeyVaultIntegrationTests
{
    [Fact]
    public void AddApiKeyValidation_WithKeyVaultUrl_RegistersKeyVaultValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AZURE_KEY_VAULT_URL"] = "https://your-test-vault-name.vault.azure.net/" // Test placeholder URL
            })
            .Build();

        // Add required logging services
        services.AddLogging();

        // Act
        services.AddApiKeyValidation(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetService<IAPIKeyValidator>();
        
        Assert.NotNull(validator);
        Assert.IsType<KeyVaultApiKeyValidator>(validator);
        
        var keyVaultService = serviceProvider.GetService<IKeyVaultService>();
        Assert.NotNull(keyVaultService);
        Assert.IsType<KeyVaultService>(keyVaultService);
    }

    [Fact]
    public void AddApiKeyValidation_WithoutKeyVaultUrl_RegistersSimpleValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["{{API_KEY_ENVIRONMENT_VARIABLE}}"] = "test-api-key-12345678901234567890123456"
            })
            .Build();

        // Add required logging services
        services.AddLogging();

        // Act
        services.AddApiKeyValidation(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetService<IAPIKeyValidator>();
        
        Assert.NotNull(validator);
        Assert.IsType<ApiKeyValidator>(validator);
        
        // Key Vault service should not be registered when not using Key Vault
        var keyVaultService = serviceProvider.GetService<IKeyVaultService>();
        Assert.Null(keyVaultService);
    }

    [Fact]
    public void AddKeyVaultServices_RegistersKeyVaultService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AZURE_KEY_VAULT_URL"] = "https://your-test-vault-name.vault.azure.net/" // Test placeholder URL
            })
            .Build();

        // Add required logging services
        services.AddLogging();

        // Act
        services.AddKeyVaultServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var keyVaultService = serviceProvider.GetService<IKeyVaultService>();
        
        Assert.NotNull(keyVaultService);
        Assert.IsType<KeyVaultService>(keyVaultService);
    }

    [Fact]
    public void KeyVaultApiKeyValidator_CanBeInstantiated()
    {
        // Arrange
        var mockKeyVaultService = new Mock<IKeyVaultService>();
        var mockLogger = new Mock<ILogger<KeyVaultApiKeyValidator>>();

        // Act
        var validator = new KeyVaultApiKeyValidator(
            mockKeyVaultService.Object, 
            mockLogger.Object);

        // Assert
        Assert.NotNull(validator);
        Assert.IsAssignableFrom<IAPIKeyValidator>(validator);
    }
}