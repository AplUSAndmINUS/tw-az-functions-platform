# Azure Functions PaaS Platform

This repository contains a comprehensive Platform-as-a-Service (PaaS) Azure Functions platform that provides reusable components and services for building Azure Functions applications. The platform serves as a foundation that can be leveraged to create new functions separate from specific business applications.

## Overview

This project is an Azure Functions platform written in .NET 8 isolated that provides APIs and services for managing content across multiple storage types including Azure Blob Storage, Azure Table Storage, and Azure CosmosDB. The platform includes advanced image processing, document conversion, media handling, and comprehensive validation services.

## Project Structure

The solution is organized into the following projects:

### 📁 **src/Functions/** - Azure Functions Application
Contains the Azure Functions endpoint with isolated process model:
- **BlogPosts/** - Sample ping function demonstrating platform usage
  - **Functions/** - HTTP-triggered functions  
  - **Models/** - Data models
- **Program.cs** - Dependency injection configuration and startup
- **host.json** - Azure Functions host configuration

### 📁 **SharedStorage/** - Shared Storage Services Library
Comprehensive storage service implementations:
- **Services/** - Storage service implementations
  - **BaseServices/** - Core storage services
    - **BlobStorageService.cs** - Azure Blob Storage operations with image processing
    - **TableStorageService.cs** - Azure Table Storage operations
  - **CosmosDbService.cs** - Azure CosmosDB operations
  - **ImageConversionService.cs** - Image format conversion (WebP optimization)
  - **ThumbnailService.cs** - Thumbnail generation for images
  - **MediaHandler.cs** - Media file processing and metadata extraction
  - **DocumentConversionService.cs** - Document format conversion and text extraction
- **Validators/** - Azure resource validation
- **Environment/** - Environment-specific configuration

### 📁 **Utils/** - Utility Classes and Helpers
Universal utilities and validation components:
- **Constants/** - Shared constant values
- **Validation/** - API key validation and other validators
- **AppInsightsLogger.cs** - Application Insights integration
- **CdnUrlBuilder.cs** - URL generation for CDN resources
- **ContentNameResolver.cs** - Container and table name resolution

### 📁 **Tests/** - Unit Testing Suite
Comprehensive test coverage for all platform components:
- **ApiKeyValidatorTests.cs** - API key validation tests
- **DocumentConversionServiceTests.cs** - Document processing tests
- **MediaHandlerTests.cs** - Media handling tests

## Features

### 🔧 **Core Platform Features**
- Isolated process model (.NET 8 Azure Functions)
- Dependency injection with proper service registration
- Comprehensive logging with Application Insights integration
- API key authentication and validation
- Environment-specific configuration management

### 🗄️ **Storage Services**
- **Blob Storage**: File upload/download with automatic image optimization
- **Table Storage**: Structured data operations with pagination
- **CosmosDB**: NoSQL document database operations
- **Media Processing**: Automatic image conversion to WebP format
- **Thumbnail Generation**: Automatic thumbnail creation for uploaded images

### 📄 **Document & Media Processing**
- **Image Processing**: Automatic WebP conversion with quality optimization
- **Document Conversion**: Text extraction and format conversion
- **Media Metadata**: Extraction of media file information
- **File Type Detection**: Automatic content type detection and validation

### 🛡️ **Security & Validation**
- API key validation with configurable requirements
- Azure resource validation
- Input sanitization and validation
- Secure credential management with Azure Identity

### 📊 **Monitoring & Logging**
- Application Insights telemetry integration
- Structured logging with Microsoft.Extensions.Logging
- Performance monitoring and error tracking
- CDN URL resolution for optimized asset delivery

## Quick Start

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Azure Storage Account (or Azurite for local development)
- Azure CosmosDB Account (optional, for CosmosDB features)
- Visual Studio Code or Visual Studio 2022

### Local Development

1. **Clone this repository**
   ```bash
   git clone https://github.com/AplUSAndmINUS/tw-az-functions-platform.git
   cd tw-az-functions-platform
   ```

2. **Configure your `local.settings.json` file in the `src/Functions` directory:**
   ```json
   {
       "IsEncrypted": false,
       "Values": {
           "AzureWebJobsStorage": "UseDevelopmentStorage=true",
           "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
           "StorageAccountName": "your-storage-account-name",
           "CosmosAccountName": "your-cosmos-account-name",
           "X_API_ENVIRONMENT_KEY": "your-api-key-32-characters-minimum"
       }
   }
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

5. **Start the Functions runtime:**
   ```bash
   cd src/Functions
   func start
   ```

### Deployment

The platform can be deployed to Azure using standard Azure Functions deployment methods:

- **Azure Functions Extension for VS Code**
- **Azure CLI**: `az functionapp deployment source config-zip`
- **Azure DevOps Pipelines**
- **GitHub Actions**

## Configuration

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `StorageAccountName` | Azure Storage Account name | Yes |
| `CosmosAccountName` | Azure CosmosDB Account name | No* |
| `X_API_ENVIRONMENT_KEY` | API key for authentication | Yes |

*Required only if using CosmosDB services

### Service Registration

The platform automatically registers all services with dependency injection in `Program.cs`:

```csharp
// Core services
services.AddSingleton<IBlobStorageService, BlobStorageService>();
services.AddSingleton<ITableStorageService, TableStorageService>();
services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Processing services
services.AddSingleton<IImageService, ImageConversionService>();
services.AddSingleton<IThumbnailService, ThumbnailService>();
services.AddSingleton<IMediaHandler, MediaHandler>();
services.AddSingleton<IDocumentConversionService, DocumentConversionService>();

// Validation and utilities
services.AddSingleton<IAPIKeyValidator, ApiKeyValidator>();
services.AddSingleton<AppInsightsLogger>();
```

## Usage Examples

### Using Blob Storage Service

```csharp
public class MyFunction
{
    private readonly IBlobStorageService _blobService;
    
    public MyFunction(IBlobStorageService blobService)
    {
        _blobService = blobService;
    }
    
    [Function("UploadImage")]
    public async Task<HttpResponseData> UploadImage(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        // Upload will automatically convert to WebP and generate thumbnail
        var result = await _blobService.UploadBlobAsync("images", "photo.jpg", imageStream);
        // Returns MediaReference with original, processed, and thumbnail URLs
    }
}
```

### Using CosmosDB Service

```csharp
public class DataFunction
{
    private readonly ICosmosDbService _cosmosService;
    
    public DataFunction(ICosmosDbService cosmosService)
    {
        _cosmosService = cosmosService;
    }
    
    [Function("SaveDocument")]
    public async Task<HttpResponseData> SaveDocument(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var document = new { id = "123", content = "data" };
        await _cosmosService.UpsertItemAsync("mydb", "documents", document, "123");
    }
}
```

### Using Media Handler

```csharp
public class MediaFunction
{
    private readonly IMediaHandler _mediaHandler;
    
    [Function("ProcessMedia")]
    public async Task<HttpResponseData> ProcessMedia(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var result = await _mediaHandler.ProcessMediaAsync("media", "video.mp4", fileStream);
        // Automatically processes based on file type and generates metadata
    }
}
```

## Testing

The platform includes comprehensive unit tests covering all major components:

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=ApiKeyValidatorTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

- ✅ API Key Validation
- ✅ Document Conversion Services  
- ✅ Media Handler Operations
- ✅ Storage Service Interfaces
- ✅ Utility Functions

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow the existing code style and patterns
- Add unit tests for new functionality
- Update documentation for new features
- Ensure all tests pass before submitting PR

## Performance Considerations

- **Image Processing**: Images are automatically converted to WebP format for optimal compression
- **Pagination**: All list operations support pagination to handle large datasets
- **Caching**: Consider implementing caching strategies for frequently accessed data
- **Connection Pooling**: The platform uses Azure Identity with managed connections

## Security Best Practices

- **API Keys**: Use strong, randomly generated API keys (minimum 32 characters)
- **Azure Identity**: Leverage managed identities for secure Azure service authentication
- **Input Validation**: All inputs are validated before processing
- **Error Handling**: Sensitive information is not exposed in error messages

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions:
1. Check the [Issues](https://github.com/AplUSAndmINUS/tw-az-functions-platform/issues) section
2. Create a new issue with detailed information
3. Review the documentation and examples above

---

**Built with ❤️ for the Azure Functions community**
