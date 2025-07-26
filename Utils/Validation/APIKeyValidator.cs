using Microsoft.Extensions.Logging;

namespace Utils.Validation;

public class ApiKeyValidator : IAPIKeyValidator
{
    private readonly string _validApiKey;
    private readonly ILogger<ApiKeyValidator>? _logger;
    private string? _errorMessage;

    public ApiKeyValidator(string validApiKey, ILogger<ApiKeyValidator>? logger = null)
    {
        _validApiKey = validApiKey ?? throw new ArgumentNullException(nameof(validApiKey));
        _logger = logger;
    }

    public bool IsValid(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _errorMessage = "API key cannot be null or empty.";
            _logger?.LogWarning("API key validation failed: {ErrorMessage}", _errorMessage);
            return false;
        }

        // Check minimum length for security
        if (apiKey.Length < 32)
        {
            _errorMessage = "API key must be at least 32 characters long.";
            _logger?.LogWarning("API key validation failed: {ErrorMessage}", _errorMessage);
            return false;
        }

        // Validate against the expected API key
        if (!string.Equals(apiKey, _validApiKey, StringComparison.Ordinal))
        {
            _errorMessage = "Invalid API key.";
            _logger?.LogWarning("API key validation failed: Invalid key provided");
            return false;
        }

        _errorMessage = null;
        _logger?.LogDebug("API key validation successful");
        return true;
    }

    public async Task<bool> IsValidAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        // For the basic implementation, we can just call the synchronous version
        // In a real-world scenario, this might involve database lookups or external validation
        return await Task.FromResult(IsValid(apiKey));
    }

    public string? GetErrorMessage()
    {
        return _errorMessage;
    }

    public void ClearErrorMessage()
    {
        _errorMessage = null;
    }

    /// <summary>
    /// Validates API key format without checking against the valid key
    /// </summary>
    /// <param name="apiKey">API key to validate format</param>
    /// <returns>True if format is valid</returns>
    public static bool IsValidFormat(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        // Check minimum length
        if (apiKey.Length < 32)
            return false;

        // Check that it doesn't contain obvious invalid characters
        if (apiKey.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
            return false;

        return true;
    }

    /// <summary>
    /// Generates a secure API key
    /// </summary>
    /// <param name="length">Length of the API key (minimum 32)</param>
    /// <returns>Generated API key</returns>
    public static string GenerateApiKey(int length = 64)
    {
        if (length < 32)
            throw new ArgumentException("API key length must be at least 32 characters", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

/* Usage example:
  var apiKeyValidator = new ApiKeyValidator("your-valid-api-key", logger);
  if (!apiKeyValidator.IsValid(requestApiKey))
  {
      return new BadRequestObjectResult(apiKeyValidator.GetErrorMessage());
  }
*/