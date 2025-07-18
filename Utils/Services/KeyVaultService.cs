using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Utils.Configuration;

namespace Utils.Services;

/// <summary>
/// Service for interacting with Azure Key Vault
/// </summary>
public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;

    /// <summary>
    /// Initializes a new instance of the KeyVaultService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public KeyVaultService(ILogger<KeyVaultService> logger)
    {
        _logger = logger;
        
        var keyVaultUrl = EnvironmentHelper.GetRequiredEnvironmentVariable("AZURE_KEY_VAULT_URL");
        
        // Use DefaultAzureCredential for authentication
        var credential = new DefaultAzureCredential();
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        
        _logger.LogInformation("KeyVaultService initialized with URL: {KeyVaultUrl}", keyVaultUrl);
    }

    /// <summary>
    /// Initializes a new instance of the KeyVaultService with a custom credential
    /// </summary>
    /// <param name="keyVaultUrl">The URL of the Azure Key Vault</param>
    /// <param name="credential">The credential to use for authentication</param>
    /// <param name="logger">Logger instance</param>
    public KeyVaultService(string keyVaultUrl, Azure.Core.TokenCredential credential, ILogger<KeyVaultService> logger)
    {
        _logger = logger;
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        
        _logger.LogInformation("KeyVaultService initialized with URL: {KeyVaultUrl}", keyVaultUrl);
    }

    /// <inheritdoc />
    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving secret: {SecretName}", secretName);
            
            var response = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Successfully retrieved secret: {SecretName}", secretName);
            return response.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetSecretAsync(string secretName, string defaultValue, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetSecretAsync(secretName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve secret: {SecretName}, returning default value", secretName);
            return defaultValue;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if secret exists: {SecretName}", secretName);
            
            await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Secret exists: {SecretName}", secretName);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogDebug("Secret does not exist: {SecretName}", secretName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if secret exists: {SecretName}", secretName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Setting secret: {SecretName}", secretName);
            
            await _secretClient.SetSecretAsync(secretName, secretValue, cancellationToken);
            
            _logger.LogDebug("Successfully set secret: {SecretName}", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting secret: {SecretName}", secretName);
            
            var operation = await _secretClient.StartDeleteSecretAsync(secretName, cancellationToken);
            await operation.WaitForCompletionAsync(cancellationToken);
            
            _logger.LogDebug("Successfully deleted secret: {SecretName}", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret: {SecretName}", secretName);
            throw;
        }
    }
}