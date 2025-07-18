using Microsoft.Extensions.Logging;
using Utils.Services;

namespace Utils.Validation;

/// <summary>
/// API Key validator that retrieves valid API keys from Azure Key Vault
/// </summary>
public class KeyVaultApiKeyValidator : IAPIKeyValidator
{
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<KeyVaultApiKeyValidator> _logger;
    private readonly string _apiKeySecretName;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the KeyVaultApiKeyValidator
    /// </summary>
    /// <param name="keyVaultService">Key Vault service instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="apiKeySecretName">Name of the secret containing the valid API key</param>
    public KeyVaultApiKeyValidator(
        IKeyVaultService keyVaultService,
        ILogger<KeyVaultApiKeyValidator> logger,
        string apiKeySecretName = "API-KEY")
    {
        _keyVaultService = keyVaultService;
        _logger = logger;
        _apiKeySecretName = apiKeySecretName;
    }

    /// <summary>
    /// Validates the provided API key synchronously by calling the async version
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if the API key is valid; otherwise, false</returns>
    public bool IsValid(string apiKey)
    {
        try
        {
            return IsValidAsync(apiKey).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key synchronously");
            _errorMessage = "Internal error validating API key.";
            return false;
        }
    }

    /// <summary>
    /// Validates the provided API key asynchronously
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the API key is valid; otherwise, false</returns>
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _errorMessage = "API key cannot be null or empty.";
                return false;
            }

            if (apiKey.Length < 32)
            {
                _errorMessage = "API key is too short. Must be at least 32 characters.";
                return false;
            }

            _logger.LogDebug("Validating API key against Key Vault secret: {SecretName}", _apiKeySecretName);

            // Retrieve the valid API key from Key Vault
            var validApiKey = await _keyVaultService.GetSecretAsync(_apiKeySecretName, cancellationToken);

            if (string.IsNullOrWhiteSpace(validApiKey))
            {
                _logger.LogWarning("Valid API key not found in Key Vault secret: {SecretName}", _apiKeySecretName);
                _errorMessage = "Unable to retrieve valid API key configuration.";
                return false;
            }

            // Compare the provided API key with the one from Key Vault
            var isValid = string.Equals(apiKey, validApiKey, StringComparison.Ordinal);

            if (isValid)
            {
                _logger.LogDebug("API key validation successful");
                _errorMessage = null;
            }
            else
            {
                _logger.LogWarning("API key validation failed - key mismatch");
                _errorMessage = "Invalid API key.";
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key against Key Vault");
            _errorMessage = "Internal error validating API key.";
            return false;
        }
    }

    /// <summary>
    /// Gets the error message if the API key is invalid
    /// </summary>
    /// <returns>The error message or null if the API key is valid</returns>
    public string? GetErrorMessage()
    {
        return _errorMessage;
    }
}

/* Usage example:
var keyVaultService = new KeyVaultService(logger);
var apiKeyValidator = new KeyVaultApiKeyValidator(keyVaultService, logger, "API-KEY");

// Async validation (recommended)
if (!await apiKeyValidator.IsValidAsync(requestApiKey))
{
    return new BadRequestObjectResult(apiKeyValidator.GetErrorMessage());
}

// Sync validation (fallback)
if (!apiKeyValidator.IsValid(requestApiKey))
{
    return new BadRequestObjectResult(apiKeyValidator.GetErrorMessage());
}
*/