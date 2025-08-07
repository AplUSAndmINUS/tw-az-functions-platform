using System.Text;

namespace Utils.DocumentationGenerator;

/// <summary>
/// Generates Markdown documentation for Azure Functions
/// </summary>
public class MarkdownGenerator
{
    public async Task GenerateDocumentationAsync(List<FunctionMetadata> functions, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        foreach (var function in functions)
        {
            var markdown = GenerateFunctionMarkdown(function);
            var fileName = $"{function.FunctionName}.md";
            var filePath = Path.Combine(outputDirectory, fileName);
            
            await File.WriteAllTextAsync(filePath, markdown);
            Console.WriteLine($"Generated documentation: {fileName}");
        }

        // Generate index file
        await GenerateIndexAsync(functions, outputDirectory);
    }

    private string GenerateFunctionMarkdown(FunctionMetadata function)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"## 📘 Function Documentation: `{function.FunctionName}`");
        sb.AppendLine();

        // Overview
        sb.AppendLine("### 🧠 Overview");
        sb.AppendLine(function.Description);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Endpoint
        sb.AppendLine("### 🔗 Endpoint");
        sb.AppendLine("```http");
        var method = function.HttpMethods.FirstOrDefault() ?? "GET";
        var route = function.Route.StartsWith("/") ? function.Route : "/" + function.Route;
        sb.AppendLine($"{method} {route}");
        sb.AppendLine("```");
        sb.AppendLine();

        // Authentication
        if (function.RequiresAuth)
        {
            sb.AppendLine("### 🔐 Authentication");
            sb.AppendLine("| Header | Value |");
            sb.AppendLine("| -- | -- |");
            sb.AppendLine("| x-api-key | your-api-key |");
            sb.AppendLine();
        }

        // URL Parameters
        if (!string.IsNullOrEmpty(function.RouteParameter))
        {
            sb.AppendLine("### URL Parameters");
            sb.AppendLine("| Name | Type | Required | Description |");
            sb.AppendLine("| -- | -- | -- | -- |");
            var param = function.Parameters.FirstOrDefault(p => p.Source == "route");
            if (param != null)
            {
                var required = param.IsRequired ? "✅" : "❌";
                sb.AppendLine($"| {param.Name} | {param.Type} | {required} | {param.Description} |");
            }
            sb.AppendLine();
        }

        // Request Body (for POST/PUT operations)
        if (function.HttpMethods.Contains("POST") || function.HttpMethods.Contains("PUT"))
        {
            sb.AppendLine("### 📦 Request Body");
            sb.AppendLine("```json");
            sb.AppendLine(GenerateRequestBodyExample(function));
            sb.AppendLine("```");
            sb.AppendLine();

            // Required Fields
            if (function.RequiredFields.Any())
            {
                sb.AppendLine("### ✅ Required Fields");
                sb.AppendLine("| Field | Type | Notes |");
                sb.AppendLine("| -- | -- | -- |");
                
                foreach (var field in function.RequiredFields)
                {
                    var notes = GetFieldNotes(field, function);
                    var type = GetFieldType(field);
                    sb.AppendLine($"| {field} | {type} | {notes} |");
                }
                sb.AppendLine();
            }
        }

        // Response
        sb.AppendLine("### 📤 Response (200 OK)");
        sb.AppendLine("```json");
        sb.AppendLine(GenerateResponseExample(function));
        sb.AppendLine("```");
        sb.AppendLine();

        // Error Responses
        sb.AppendLine("### ❌ Error Responses");
        sb.AppendLine("| Status Code | Message Example |");
        sb.AppendLine("| -- | -- |");
        sb.AppendLine("| 400 Bad Request | [\"Title is required\", \"Author slug is required\"] |");
        if (function.RequiresAuth)
        {
            sb.AppendLine("| 401 Unauthorized | \"Invalid API key\" |");
        }
        sb.AppendLine("| 500 Internal Server Error | \"An unexpected error occurred\" |");
        sb.AppendLine();

        // Testing Example
        sb.AppendLine("### 🧪 Testing Example (curl)");
        sb.AppendLine("```bash");
        sb.AppendLine(GenerateCurlExample(function));
        sb.AppendLine("```");
        sb.AppendLine();

        // Common Issues
        sb.AppendLine("### 🧠 Common Issues");
        GenerateCommonIssues(sb, function);
        sb.AppendLine();

        // Related Endpoints
        sb.AppendLine("### 🔗 Related Endpoints");
        sb.AppendLine("| Method | Endpoint | Description |");
        sb.AppendLine("| -- | -- | -- |");
        GenerateRelatedEndpoints(sb, function);

        // Tags for Notion
        sb.AppendLine();
        sb.AppendLine("#Function #Payload #ErrorHandling");

        return sb.ToString();
    }

    private string GenerateRequestBodyExample(FunctionMetadata function)
    {
        // Generate generic example suitable for platform
        return @"{
  ""title"": ""Sample Title"",
  ""slug"": ""sample-slug"",
  ""description"": ""Sample description"",
  ""content"": ""Sample content"",
  ""status"": ""Published""
}";
    }

    private string GenerateResponseExample(FunctionMetadata function)
    {
        // Generate generic response example
        return @"{
  ""id"": ""sample-id"",
  ""slug"": ""sample-slug"",
  ""title"": ""Sample Title"",
  ""status"": ""Published"",
  ""lastModified"": ""2025-08-07T13:48:40.4333258Z""
}";
    }

    private string GetFieldNotes(string field, FunctionMetadata function)
    {
        return field.ToLower() switch
        {
            "title" => "Title of the content",
            "content" => "Markdown supported",
            "category" => "Category classification",
            "slug" => "Unique identifier",
            _ => "Required field"
        };
    }

    private string GetFieldType(string field)
    {
        return field.ToLower() switch
        {
            "tagslist" => "array",
            "publishdate" => "string",
            "ispublished" => "boolean",
            "readingtimeminutes" => "number",
            _ => "string"
        };
    }

    private string GenerateCurlExample(FunctionMetadata function)
    {
        var method = function.HttpMethods.FirstOrDefault() ?? "GET";
        var route = function.Route.StartsWith("/") ? function.Route : "/" + function.Route;
        
        // Replace route parameters with example values
        if (!string.IsNullOrEmpty(function.RouteParameter))
        {
            route = route.Replace($"{{{function.RouteParameter}}}", "sample-slug");
            route = route.Replace($"{{{function.RouteParameter}?}}", "sample-slug");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"curl -X {method} \\");
        sb.AppendLine($"  http://localhost:7071{route} \\");
        
        if (function.RequiresAuth)
        {
            sb.AppendLine("  -H \"x-api-key: your-api-key\" \\");
        }
        
        if (method == "POST" || method == "PUT")
        {
            sb.AppendLine("  -H \"Content-Type: application/json\" \\");
            sb.AppendLine("  -d '{ ... }'");
        }
        else
        {
            sb.Length -= 3; // Remove the trailing " \"
        }

        return sb.ToString();
    }

    private void GenerateCommonIssues(StringBuilder sb, FunctionMetadata function)
    {
        sb.AppendLine("- Missing required fields will trigger 400 errors");
        sb.AppendLine("- Ensure API key is valid and scoped correctly");
        sb.AppendLine("- Check that referenced entities exist");
    }

    private void GenerateRelatedEndpoints(StringBuilder sb, FunctionMetadata function)
    {
        var category = function.Category.ToLower();
        var entityName = category.TrimEnd('s');
        
        if (function.FunctionName.StartsWith("Upsert"))
        {
            sb.AppendLine($"| GET | /{category}/{{slug}} | Retrieve a specific {entityName} |");
            sb.AppendLine($"| GET | /{category} | List all {category} |");
            sb.AppendLine($"| DELETE | /{category}/{{slug}} | Delete a {entityName} |");
        }
        else if (function.FunctionName.StartsWith("Get") && function.FunctionName.EndsWith("s"))
        {
            sb.AppendLine($"| PUT | /{category}/{{slug}} | Create or update a {entityName} |");
            sb.AppendLine($"| GET | /{category}/{{slug}} | Retrieve a specific {entityName} |");
            sb.AppendLine($"| DELETE | /{category}/{{slug}} | Delete a {entityName} |");
        }
        else if (function.FunctionName.StartsWith("Get"))
        {
            sb.AppendLine($"| PUT | /{category}/{{slug}} | Create or update a {entityName} |");
            sb.AppendLine($"| GET | /{category} | List all {category} |");
            sb.AppendLine($"| DELETE | /{category}/{{slug}} | Delete a {entityName} |");
        }
        else if (function.FunctionName.StartsWith("Delete"))
        {
            sb.AppendLine($"| PUT | /{category}/{{slug}} | Create or update a {entityName} |");
            sb.AppendLine($"| GET | /{category}/{{slug}} | Retrieve a specific {entityName} |");
            sb.AppendLine($"| GET | /{category} | List all {category} |");
        }
    }

    private async Task GenerateIndexAsync(List<FunctionMetadata> functions, string outputDirectory)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("# Azure Functions API Documentation");
        sb.AppendLine();
        sb.AppendLine("This directory contains automatically generated documentation for all Azure Functions in the application.");
        sb.AppendLine();
        sb.AppendLine("## Functions by Category");
        sb.AppendLine();

        var groupedFunctions = functions.GroupBy(f => f.Category).OrderBy(g => g.Key);
        
        foreach (var group in groupedFunctions)
        {
            sb.AppendLine($"### {group.Key}");
            sb.AppendLine();
            
            foreach (var function in group.OrderBy(f => f.FunctionName))
            {
                var methods = string.Join(", ", function.HttpMethods);
                sb.AppendLine($"- [{function.FunctionName}](./{function.FunctionName}.md) - {methods} - {function.Description}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Usage Notes");
        sb.AppendLine();
        sb.AppendLine("- All endpoints require API key authentication via `x-api-key` header");
        sb.AppendLine("- Base URL for local development: `http://localhost:7071`");
        sb.AppendLine("- All dates should be in ISO 8601 format");
        sb.AppendLine("- Media references must be stringified JSON arrays");
        sb.AppendLine();
        sb.AppendLine("#Function #Documentation #API");

        var indexPath = Path.Combine(outputDirectory, "README.md");
        await File.WriteAllTextAsync(indexPath, sb.ToString());
        Console.WriteLine("Generated index: README.md");
    }
}