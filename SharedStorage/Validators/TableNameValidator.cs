using System.Text.RegularExpressions;

namespace SharedStorage.Validators;

public static class TableNameValidator
{
    private static readonly Regex TableNameRegex = new(@"^[a-zA-Z][a-zA-Z0-9]{2,62}$", RegexOptions.Compiled);
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "tables"
    };

    public static void ValidateTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        }

        if (tableName.Length < 3 || tableName.Length > 63)
        {
            throw new ArgumentException("Table name must be between 3 and 63 characters long.", nameof(tableName));
        }

        if (!TableNameRegex.IsMatch(tableName))
        {
            throw new ArgumentException("Table name must start with a letter and only contain alphanumeric characters.", nameof(tableName));
        }

        if (ReservedNames.Contains(tableName))
        {
            throw new ArgumentException($"Table name '{tableName}' is reserved and cannot be used.", nameof(tableName));
        }
    }

    public static bool IsValidTableName(string tableName)
    {
        try
        {
            ValidateTableName(tableName);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string SanitizeTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        }

        // Remove invalid characters and ensure it starts with a letter
        var sanitized = Regex.Replace(tableName, @"[^a-zA-Z0-9]", "");
        
        if (string.IsNullOrEmpty(sanitized) || !char.IsLetter(sanitized[0]))
        {
            sanitized = "Table" + sanitized;
        }

        // Ensure length constraints
        if (sanitized.Length < 3)
        {
            sanitized = sanitized.PadRight(3, '0');
        }
        else if (sanitized.Length > 63)
        {
            sanitized = sanitized.Substring(0, 63);
        }

        // Check for reserved names and append suffix if needed
        if (ReservedNames.Contains(sanitized))
        {
            sanitized += "Data";
            if (sanitized.Length > 63)
            {
                sanitized = sanitized.Substring(0, 59) + "Data";
            }
        }

        return sanitized;
    }
}