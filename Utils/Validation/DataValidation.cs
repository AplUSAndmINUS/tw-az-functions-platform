using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Utils.Validation;

public static class DataValidation
{
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex GuidRegex = new(@"^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Validates if a string is a valid email address
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if valid email address</returns>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email);
    }

    /// <summary>
    /// Validates if a string is a valid phone number
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if valid phone number</returns>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove common formatting characters
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(".", "");
        return PhoneRegex.IsMatch(cleaned);
    }

    /// <summary>
    /// Validates if a string is a valid URL
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <returns>True if valid URL</returns>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates if a string is a valid GUID
    /// </summary>
    /// <param name="guid">GUID string to validate</param>
    /// <returns>True if valid GUID</returns>
    public static bool IsValidGuid(string? guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        return Guid.TryParse(guid, out _);
    }

    /// <summary>
    /// Validates if a string contains only alphanumeric characters
    /// </summary>
    /// <param name="input">String to validate</param>
    /// <returns>True if alphanumeric</returns>
    public static bool IsAlphanumeric(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return input.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Validates if a string is a valid Base64 string
    /// </summary>
    /// <param name="base64">Base64 string to validate</param>
    /// <returns>True if valid Base64</returns>
    public static bool IsValidBase64(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return false;

        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string has a minimum and maximum length
    /// </summary>
    /// <param name="input">String to validate</param>
    /// <param name="minLength">Minimum length</param>
    /// <param name="maxLength">Maximum length</param>
    /// <returns>True if length is within bounds</returns>
    public static bool IsValidLength(string? input, int minLength, int maxLength)
    {
        if (input == null)
            return minLength == 0;

        return input.Length >= minLength && input.Length <= maxLength;
    }

    /// <summary>
    /// Validates if a number is within a specified range
    /// </summary>
    /// <param name="value">Number to validate</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>True if within range</returns>
    public static bool IsInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a number is within a specified range
    /// </summary>
    /// <param name="value">Number to validate</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>True if within range</returns>
    public static bool IsInRange(double value, double min, double max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates if a date is within a specified range
    /// </summary>
    /// <param name="date">Date to validate</param>
    /// <param name="minDate">Minimum date</param>
    /// <param name="maxDate">Maximum date</param>
    /// <returns>True if within range</returns>
    public static bool IsDateInRange(DateTime date, DateTime minDate, DateTime maxDate)
    {
        return date >= minDate && date <= maxDate;
    }

    /// <summary>
    /// Validates if a string matches a specific pattern
    /// </summary>
    /// <param name="input">String to validate</param>
    /// <param name="pattern">Regex pattern</param>
    /// <returns>True if matches pattern</returns>
    public static bool MatchesPattern(string? input, string pattern)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(pattern))
            return false;

        try
        {
            return Regex.IsMatch(input, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a file extension is in a list of allowed extensions
    /// </summary>
    /// <param name="fileName">File name to validate</param>
    /// <param name="allowedExtensions">Array of allowed extensions (e.g., [".jpg", ".png"])</param>
    /// <returns>True if extension is allowed</returns>
    public static bool IsValidFileExtension(string? fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName) || allowedExtensions == null || !allowedExtensions.Any())
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if a file size is within allowed limits
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <param name="maxSizeInBytes">Maximum allowed size in bytes</param>
    /// <returns>True if size is within limits</returns>
    public static bool IsValidFileSize(long fileSize, long maxSizeInBytes)
    {
        return fileSize > 0 && fileSize <= maxSizeInBytes;
    }

    /// <summary>
    /// Sanitizes a string by removing or replacing dangerous characters
    /// </summary>
    /// <param name="input">String to sanitize</param>
    /// <param name="replacement">Character to replace dangerous characters with</param>
    /// <returns>Sanitized string</returns>
    public static string SanitizeString(string? input, char replacement = '_')
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove or replace dangerous characters
        var dangerous = new[] { '<', '>', '"', '\'', '&', '\r', '\n', '\t' };
        var result = input;

        foreach (var c in dangerous)
        {
            result = result.Replace(c, replacement);
        }

        return result.Trim();
    }

    /// <summary>
    /// Validates if a string is a valid JSON
    /// </summary>
    /// <param name="json">JSON string to validate</param>
    /// <returns>True if valid JSON</returns>
    public static bool IsValidJson(string? json)
    {
        return JsonHelper.IsValidJson(json ?? string.Empty);
    }

    /// <summary>
    /// Validates an object using data annotations
    /// </summary>
    /// <param name="obj">Object to validate</param>
    /// <param name="validationResults">List to store validation results</param>
    /// <returns>True if object is valid</returns>
    public static bool ValidateObject(object obj, out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);
        return Validator.TryValidateObject(obj, context, validationResults, true);
    }

    /// <summary>
    /// Validates a property value using data annotations
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <param name="validationResults">List to store validation results</param>
    /// <returns>True if property is valid</returns>
    public static bool ValidateProperty(object value, ValidationContext validationContext, out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
        return Validator.TryValidateProperty(value, validationContext, validationResults);
    }

    /// <summary>
    /// Validates if a string is a valid Azure storage account name
    /// </summary>
    /// <param name="accountName">Storage account name to validate</param>
    /// <returns>True if valid storage account name</returns>
    public static bool IsValidStorageAccountName(string? accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            return false;

        // Azure storage account names must be 3-24 characters, lowercase letters and numbers only
        return accountName.Length >= 3 && 
               accountName.Length <= 24 && 
               accountName.All(c => char.IsLower(c) || char.IsDigit(c));
    }

    /// <summary>
    /// Validates if a string is a valid container name
    /// </summary>
    /// <param name="containerName">Container name to validate</param>
    /// <returns>True if valid container name</returns>
    public static bool IsValidContainerName(string? containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            return false;

        // Azure container names must be 3-63 characters, lowercase letters, numbers, and hyphens
        // Cannot start or end with hyphen, cannot have consecutive hyphens
        if (containerName.Length < 3 || containerName.Length > 63)
            return false;

        if (containerName.StartsWith('-') || containerName.EndsWith('-'))
            return false;

        if (containerName.Contains("--"))
            return false;

        return containerName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}