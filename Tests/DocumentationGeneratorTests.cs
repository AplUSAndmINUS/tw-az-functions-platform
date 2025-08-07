using Xunit;
using Utils.DocumentationGenerator;

namespace Tests;

public class DocumentationGeneratorTests
{
    [Fact]
    public void FunctionMetadata_CanBeInitialized()
    {
        // Arrange & Act
        var metadata = new FunctionMetadata
        {
            FunctionName = "TestFunction",
            ClassName = "TestClass",
            Route = "/test",
            Category = "Test",
            Description = "Test function"
        };

        // Assert
        Assert.Equal("TestFunction", metadata.FunctionName);
        Assert.Equal("TestClass", metadata.ClassName);
        Assert.Equal("/test", metadata.Route);
        Assert.Equal("Test", metadata.Category);
        Assert.Equal("Test function", metadata.Description);
        Assert.NotNull(metadata.HttpMethods);
        Assert.NotNull(metadata.Parameters);
        Assert.NotNull(metadata.RequiredFields);
        Assert.NotNull(metadata.ValidationRules);
    }

    [Fact]
    public void FunctionParameter_CanBeInitialized()
    {
        // Arrange & Act
        var parameter = new FunctionParameter
        {
            Name = "id",
            Type = "string",
            IsRequired = true,
            Description = "Unique identifier",
            Source = "route"
        };

        // Assert
        Assert.Equal("id", parameter.Name);
        Assert.Equal("string", parameter.Type);
        Assert.True(parameter.IsRequired);
        Assert.Equal("Unique identifier", parameter.Description);
        Assert.Equal("route", parameter.Source);
    }

    [Fact]
    public async Task FunctionParser_ParseFunctionsAsync_HandlesNonExistentDirectory()
    {
        // Arrange
        var parser = new FunctionParser();
        var nonExistentPath = "/tmp/nonexistent";

        // Act
        var result = await parser.ParseFunctionsAsync(nonExistentPath);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task MarkdownGenerator_GenerateDocumentationAsync_CreatesOutputDirectory()
    {
        // Arrange
        var generator = new MarkdownGenerator();
        var functions = new List<FunctionMetadata>
        {
            new FunctionMetadata
            {
                FunctionName = "TestFunction",
                Description = "Test description",
                Route = "/test",
                Category = "Test"
            }
        };
        functions[0].HttpMethods.Add("GET");

        var outputDir = "/tmp/test-docs";
        
        // Clean up if exists
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }

        // Act
        await generator.GenerateDocumentationAsync(functions, outputDir);

        // Assert
        Assert.True(Directory.Exists(outputDir));
        Assert.True(File.Exists(Path.Combine(outputDir, "TestFunction.md")));
        Assert.True(File.Exists(Path.Combine(outputDir, "README.md")));

        // Clean up
        Directory.Delete(outputDir, true);
    }

    [Fact]
    public async Task MarkdownGenerator_GeneratedDocumentation_ContainsExpectedContent()
    {
        // Arrange
        var generator = new MarkdownGenerator();
        var functions = new List<FunctionMetadata>
        {
            new FunctionMetadata
            {
                FunctionName = "TestFunction",
                Description = "Test description",
                Route = "/test",
                Category = "Test",
                RequiresAuth = true
            }
        };
        functions[0].HttpMethods.Add("GET");

        var outputDir = "/tmp/test-docs-content";
        
        // Clean up if exists
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }

        // Act
        await generator.GenerateDocumentationAsync(functions, outputDir);

        // Assert
        var functionDocPath = Path.Combine(outputDir, "TestFunction.md");
        var content = await File.ReadAllTextAsync(functionDocPath);
        
        Assert.Contains("## 📘 Function Documentation: `TestFunction`", content);
        Assert.Contains("Test description", content);
        Assert.Contains("GET /test", content);
        Assert.Contains("x-api-key", content);
        Assert.Contains("curl -X GET", content);

        // Clean up
        Directory.Delete(outputDir, true);
    }
}