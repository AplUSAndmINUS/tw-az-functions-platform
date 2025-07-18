namespace Utils.Services;

/// <summary>
/// Interface for interacting with Azure Key Vault
/// </summary>
public interface IKeyVaultService
{
    /// <summary>
    /// Retrieves a secret from Azure Key Vault
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The secret value</returns>
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a secret from Azure Key Vault with a default value if not found
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve</param>
    /// <param name="defaultValue">Default value to return if secret is not found</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The secret value or default value</returns>
    Task<string> GetSecretAsync(string secretName, string defaultValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a secret exists in Azure Key Vault
    /// </summary>
    /// <param name="secretName">The name of the secret to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the secret exists, false otherwise</returns>
    Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a secret in Azure Key Vault
    /// </summary>
    /// <param name="secretName">The name of the secret to set</param>
    /// <param name="secretValue">The value of the secret</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task SetSecretAsync(string secretName, string secretValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret from Azure Key Vault
    /// </summary>
    /// <param name="secretName">The name of the secret to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}