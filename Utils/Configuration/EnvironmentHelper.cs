using Microsoft.Extensions.Configuration;

namespace Utils.Configuration;

/// <summary>
/// Helper class for environment configuration and variable access
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// Gets an environment variable value with an optional default value
    /// </summary>
    /// <param name="key">The environment variable key</param>
    /// <param name="defaultValue">The default value if the environment variable is not set</param>
    /// <returns>The environment variable value or the default value</returns>
    public static string GetEnvironmentVariable(string key, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

    /// <summary>
    /// Gets a required environment variable value and throws an exception if not found
    /// </summary>
    /// <param name="key">The environment variable key</param>
    /// <returns>The environment variable value</returns>
    /// <exception cref="InvalidOperationException">Thrown when the environment variable is not set</exception>
    public static string GetRequiredEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException($"Required environment variable '{key}' is not set.");
        }
        return value;
    }

    /// <summary>
    /// Gets an environment variable value as a boolean
    /// </summary>
    /// <param name="key">The environment variable key</param>
    /// <param name="defaultValue">The default value if the environment variable is not set or cannot be parsed</param>
    /// <returns>The environment variable value as a boolean</returns>
    public static bool GetEnvironmentVariableAsBool(string key, bool defaultValue = false)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets an environment variable value as an integer
    /// </summary>
    /// <param name="key">The environment variable key</param>
    /// <param name="defaultValue">The default value if the environment variable is not set or cannot be parsed</param>
    /// <returns>The environment variable value as an integer</returns>
    public static int GetEnvironmentVariableAsInt(string key, int defaultValue = 0)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Checks if running in a development environment
    /// </summary>
    /// <returns>True if running in development environment</returns>
    public static bool IsDevelopment()
    {
        var environment = GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if running in a production environment
    /// </summary>
    /// <returns>True if running in production environment</returns>
    public static bool IsProduction()
    {
        var environment = GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase);
    }
}