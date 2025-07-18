namespace Utils.Validation;

public class ApiKeyValidator : IAPIKeyValidator
{
    private readonly string _validApiKey;
    private string? _errorMessage;

    public ApiKeyValidator(string validApiKey)
    {
        _validApiKey = validApiKey;
    }

    public bool IsValid(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _errorMessage = "API key cannot be null or empty.";
            return false;
        }

        if (apiKey.Length < 32 || !string.Equals(apiKey, _validApiKey, StringComparison.Ordinal))
        {
            _errorMessage = "Invalid API key.";
            return false;
        }

        _errorMessage = null;
        return true;
    }

    public async Task<bool> IsValidAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        // For the basic implementation, we can just call the synchronous version
        return await Task.FromResult(IsValid(apiKey));
    }

    public string? GetErrorMessage()
    {
        return _errorMessage;
    }
}

/* Usage example:
  var apiKeyValidator = new ApiKeyValidator("your-valid-api-key");
  if (!apiKeyValidator.IsValid(requestApiKey))
  {
      return new BadRequestObjectResult(apiKeyValidator.GetErrorMessage());
  }
*/