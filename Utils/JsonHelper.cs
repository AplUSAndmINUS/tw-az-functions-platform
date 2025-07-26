using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes an object to JSON string using default options
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON string</returns>
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string using pretty printing
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Pretty-formatted JSON string</returns>
    public static string SerializePretty<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, PrettyOptions);
    }

    /// <summary>
    /// Serializes an object to JSON string using custom options
    /// </summary>
    /// <typeparam name="T">Type of object to serialize</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <param name="options">Custom serialization options</param>
    /// <returns>JSON string</returns>
    public static string Serialize<T>(T obj, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object using default options
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="json">JSON string</param>
    /// <returns>Deserialized object</returns>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object using custom options
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="json">JSON string</param>
    /// <param name="options">Custom deserialization options</param>
    /// <returns>Deserialized object</returns>
    public static T? Deserialize<T>(string json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to an object
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="json">JSON string</param>
    /// <param name="result">Deserialized object if successful</param>
    /// <returns>True if deserialization was successful</returns>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        result = default;
        
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is valid JSON
    /// </summary>
    /// <param name="json">JSON string to validate</param>
    /// <returns>True if valid JSON</returns>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Formats/prettifies a JSON string
    /// </summary>
    /// <param name="json">JSON string to format</param>
    /// <returns>Pretty-formatted JSON string</returns>
    public static string FormatJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, PrettyOptions);
        }
        catch
        {
            return json; // Return original if formatting fails
        }
    }

    /// <summary>
    /// Minifies a JSON string by removing whitespace
    /// </summary>
    /// <param name="json">JSON string to minify</param>
    /// <returns>Minified JSON string</returns>
    public static string MinifyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, DefaultOptions);
        }
        catch
        {
            return json; // Return original if minification fails
        }
    }

    /// <summary>
    /// Deep clones an object by serializing and deserializing it
    /// </summary>
    /// <typeparam name="T">Type of object to clone</typeparam>
    /// <param name="obj">Object to clone</param>
    /// <returns>Deep cloned object</returns>
    public static T? DeepClone<T>(T obj)
    {
        if (obj == null)
            return default;

        var json = JsonSerializer.Serialize(obj, DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Merges two JSON objects into one
    /// </summary>
    /// <param name="json1">First JSON object</param>
    /// <param name="json2">Second JSON object (properties override first)</param>
    /// <returns>Merged JSON string</returns>
    public static string MergeJson(string json1, string json2)
    {
        if (string.IsNullOrWhiteSpace(json1))
            return json2 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(json2))
            return json1;

        try
        {
            using var doc1 = JsonDocument.Parse(json1);
            using var doc2 = JsonDocument.Parse(json2);

            var merged = new Dictionary<string, object?>();

            // Add properties from first JSON
            foreach (var prop in doc1.RootElement.EnumerateObject())
            {
                merged[prop.Name] = GetValue(prop.Value);
            }

            // Override with properties from second JSON
            foreach (var prop in doc2.RootElement.EnumerateObject())
            {
                merged[prop.Name] = GetValue(prop.Value);
            }

            return JsonSerializer.Serialize(merged, DefaultOptions);
        }
        catch
        {
            return json1; // Return first JSON if merge fails
        }
    }

    private static object? GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText(), DefaultOptions),
            JsonValueKind.Array => JsonSerializer.Deserialize<object[]>(element.GetRawText(), DefaultOptions),
            _ => element.GetRawText()
        };
    }
}