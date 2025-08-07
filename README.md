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
    - **PingFunction.cs** - Health check endpoint
    - **QueueMessageFunction.cs** - Queue message operations
    - **ContactFormFunction.cs** - Contact form submission handler
  - **Models/** - Data models
- **Program.cs** - Dependency injection configuration and startup
- **host.json** - Azure Functions host configuration

### 📁 **SharedStorage/** - Shared Storage Services Library
Comprehensive storage service implementations:
- **Services/** - Storage service implementations
  - **BaseServices/** - Core storage services
    - **BlobStorageService.cs** - Azure Blob Storage operations with image processing
    - **TableStorageService.cs** - Azure Table Storage operations  
    - **QueueStorageService.cs** - Azure Queue Storage operations with message management
  - **Email/** - Email service implementation
    - **EmailService.cs** - SMTP email service with contact form formatting
    - **IEmailService.cs** - Email service interface
  - **CosmosDbService.cs** - Azure CosmosDB operations
  - **ImageConversionService.cs** - Image format conversion (WebP optimization)
  - **ThumbnailService.cs** - Thumbnail generation for images
  - **MediaHandler.cs** - Media file processing and metadata extraction
  - **DocumentConversionService.cs** - Document format conversion and text extraction
- **Validators/** - Azure resource validation (blob containers, tables, queues)
- **Environment/** - Environment-specific configuration
- **Extensions/** - Service registration and dependency injection extensions

### 📁 **Utils/** - Utility Classes and Helpers
Universal utilities and validation components:
- **Constants/** - Shared constant values
- **Validation/** - API key validation and other validators
- **AppInsightsLogger.cs** - Application Insights integration
- **CdnUrlBuilder.cs** - URL generation for CDN resources
- **ContentNameResolver.cs** - Container and table name resolution

### 📁 **Tests/** - Unit Testing Suite
Comprehensive test coverage for all platform components (185 of 187 tests passing):
- **ApiKeyValidatorTests.cs** - API key validation tests
- **DocumentConversionServiceTests.cs** - Document processing tests
- **MediaHandlerTests.cs** - Media handling tests
- **EmailServiceKeyVaultIntegrationTests.cs** - Key Vault integration tests
- **ImageSecurityTests.cs** - Image processing security validation tests

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
- **Queue Storage**: Message queue operations with full CRUD support
- **CosmosDB**: NoSQL document database operations
- **Media Processing**: Automatic image conversion to WebP format with SixLabors.ImageSharp v3.1.11
- **Thumbnail Generation**: Automatic thumbnail creation for uploaded images with enhanced security
- **Email Service**: SMTP email service with Key Vault integration and professional templates

### 📄 **Document & Media Processing**
- **Image Processing**: Automatic WebP conversion with quality optimization and SixLabors.ImageSharp v3.1.11 compatibility
- **Document Conversion**: Text extraction and format conversion
- **Media Metadata**: Extraction of media file information
- **File Type Detection**: Automatic content type detection and validation
- **Security Features**: File size limits, dimension validation, format verification, and timeout protection

### 🛡️ **Security & Validation**
- **Dual Authentication**: Support for both Managed Identity and Connection String authentication
- **Key Vault Integration**: Azure Key Vault support for secure credential management (Email service)
- **Authentication Toggle**: Environment variable to switch between authentication methods
- **API Key Validation**: Configurable API key requirements (minimum 32 characters)
- **Azure Resource Validation**: Blob containers, tables, and queues
- **Input Sanitization**: Comprehensive validation for all inputs
- **Image Security**: File size limits (50MB), dimension validation (8192x8192), format verification, and processing timeouts
- **Secure Credential Management**: Azure Identity integration with modern DefaultAzureCredential options

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
- Azure Key Vault (optional, for secure credential management)
- Visual Studio Code or Visual Studio 2022

### Important Documentation

Before starting, review these additional documentation files:
- **[AZURE_AUTHENTICATION_UPDATES.md](AZURE_AUTHENTICATION_UPDATES.md)** - Latest authentication changes and credential configuration
- **[SECURITY_CONFIGURATION.md](SECURITY_CONFIGURATION.md)** - Image processing security features and configuration
- **[.github/copilot-instructions.md](.github/copilot-instructions.md)** - Comprehensive development guidelines and commands

### Local Development

1. **Clone this repository**
   ```bash
   git clone https://github.com/AplUSAndmINUS/tw-az-functions-platform.git
   cd tw-az-functions-platform
   ```

2. **Replace placeholder values throughout the codebase:**

   Before configuring and running the application, you need to replace the following placeholders with your actual values:

   - `{{YOUR_GITHUB_USERNAME}}` - Replace with your GitHub username or organization name
   - `{{YOUR_REPOSITORY_NAME}}` - Replace with your repository name (e.g., `my-azure-functions-platform`)
   - `{{YOUR_DOMAIN}}` - Replace with your actual domain name (e.g., `example.com`)
   - `{{STORAGE_ACCOUNT_NAME}}` - Replace with your Azure Storage Account name
   - `{{DEFAULT_STORAGE_ACCOUNT_NAME}}` - Replace with your default storage account name
   - `{{DEFAULT_COSMOS_DB_NAME}}` - Replace with your default CosmosDB account name
   - `{{API_KEY_ENVIRONMENT_VARIABLE}}` - Replace with your preferred API key environment variable name (e.g., `API_KEY`)
   - `{{YOUR_COMPANY_NAME}}` - Replace with your company or application name
   
   **For test files specifically:**
   - `your-test-vault-name` - Replace with your test Key Vault name (in test files only)

   **Important:** These placeholders appear in the following files:
   - `README.md` - Update repository URLs and documentation
   - `Utils/Constants/ApiUrls.cs` - Update all URL endpoints
   - `SharedStorage/Extensions/ServiceCollectionExtensions.cs` - Update default account names
   - `SharedStorage/Services/Email/EmailService.cs` - Update default company name
   - `src/Functions/Program.cs` - Update API key environment variable name
   - `Tests/KeyVaultIntegrationTests.cs` - Update test Key Vault URL (for testing only)

3. **Configure your `local.settings.json` file in the `src/Functions` directory:**

   **For Managed Identity (Default):**
   ```json
   {
       "IsEncrypted": false,
       "Values": {
           "AzureWebJobsStorage": "UseDevelopmentStorage=true",
           "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
           "StorageAccountName": "your-storage-account-name",
           "CosmosAccountName": "your-cosmos-account-name",
           "{{API_KEY_ENVIRONMENT_VARIABLE}}": "your-api-key-32-characters-minimum",
           "SMTP_HOST": "smtp.gmail.com",
           "SMTP_PORT": "587",
           "SMTP_USERNAME": "your-email@gmail.com",
           "SMTP_PASSWORD": "your-app-password",
           "FROM_EMAIL": "your-email@gmail.com",
           "FROM_NAME": "Your Name",
           "TO_EMAIL": "recipient@example.com"
       }
   }
   ```

   **For Connection String Authentication:**
   ```json
   {
       "IsEncrypted": false,
       "Values": {
           "AzureWebJobsStorage": "UseDevelopmentStorage=true",
           "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
           "StorageAccountName": "your-storage-account-name",
           "CosmosAccountName": "your-cosmos-account-name",
           "{{API_KEY_ENVIRONMENT_VARIABLE}}": "your-api-key-32-characters-minimum",
           "USE_CONNECTION_STRING": "true",
           "AZURE_STORAGE_CONNECTION_STRING": "DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net",
           "SMTP_HOST": "smtp.gmail.com",
           "SMTP_PORT": "587",
           "SMTP_USERNAME": "your-email@gmail.com",
           "SMTP_PASSWORD": "your-app-password",
           "FROM_EMAIL": "your-email@gmail.com",
           "FROM_NAME": "Your Name",
           "TO_EMAIL": "recipient@example.com"
       }
   }
   ```

4. **Build the solution:**
   ```bash
   dotnet build
   ```

5. **Run tests:**
   ```bash
   dotnet test
   ```
   *Note: 185 of 187 tests should pass. The 2 known failing tests are related to API key validation message format and image security test configurations.*

6. **Start the Functions runtime:**
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

### Authentication Methods

The platform supports two authentication methods for Azure Storage services:

#### 1. Managed Identity (Default)
Uses Azure Managed Identity for authentication. This is the default and recommended approach for production deployments.

```json
{
  "Values": {
    "StorageAccountName": "your-storage-account-name"
  }
}
```

#### 2. Connection String
Uses Azure Storage connection strings for authentication. Enable this by setting the environment variable:

```json
{
  "Values": {
    "USE_CONNECTION_STRING": "true",
    "AZURE_STORAGE_CONNECTION_STRING": "DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net",
    "StorageAccountName": "your-storage-account-name"
  }
}
```

### Environment Variables

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `StorageAccountName` | Azure Storage Account name | Yes | |
| `CosmosAccountName` | Azure CosmosDB Account name | No* | |
| `{{API_KEY_ENVIRONMENT_VARIABLE}}` | API key for authentication (min 32 chars) | Yes | |
| `USE_CONNECTION_STRING` | Toggle to use connection string auth | No | `false` |
| `AZURE_STORAGE_CONNECTION_STRING` | Azure Storage connection string | No** | |
| `AZURE_KEY_VAULT_URL` | Azure Key Vault URL for secure credentials | No*** | |
| `SMTP_HOST` | SMTP server hostname | No**** | `smtp.gmail.com` |
| `SMTP_PORT` | SMTP server port | No**** | `587` |
| `SMTP_USERNAME` | SMTP username/email | Yes**** | |
| `SMTP_PASSWORD` | SMTP password or app password | Yes**** | |
| `FROM_EMAIL` | From email address | No**** | Uses `SMTP_USERNAME` |
| `FROM_NAME` | From name for emails | No**** | `{{YOUR_COMPANY_NAME}}` |
| `TO_EMAIL` | Default recipient email | No**** | Uses `SMTP_USERNAME` |

*Required only if using CosmosDB services  
**Required only if `USE_CONNECTION_STRING` is set to `true`  
***Optional - enables Key Vault integration for secure credential management  
****Required only if using the Contact Form functionality (can be stored in Key Vault)

### Service Registration

The platform automatically registers all services with dependency injection in `Program.cs`:

```csharp
// Core services
services.AddSingleton<IBlobStorageService, BlobStorageService>();
services.AddSingleton<ITableStorageService, TableStorageService>();
services.AddSingleton<IQueueStorageService, QueueStorageService>();
services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Processing services
services.AddSingleton<IImageService, ImageConversionService>();
services.AddSingleton<IThumbnailService, ThumbnailService>();
services.AddSingleton<IMediaHandler, MediaHandler>();
services.AddSingleton<IDocumentConversionService, DocumentConversionService>();

// Communication services
services.AddScoped<IEmailService, EmailService>(); // Now supports Key Vault integration

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

### Using Queue Storage Service

```csharp
public class QueueFunction
{
    private readonly IQueueStorageService _queueService;
    
    public QueueFunction(IQueueStorageService queueService)
    {
        _queueService = queueService;
    }
    
    [Function("SendMessage")]
    public async Task<HttpResponseData> SendMessage(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var message = await _queueService.SendMessageAsync("notifications", "Hello World!");
        // Returns QueueMessage with MessageId, TimeNextVisible, and ExpirationTime
    }
    
    [Function("ProcessMessages")]
    public async Task<HttpResponseData> ProcessMessages(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        var messages = await _queueService.ReceiveMessagesAsync("notifications", 10);
        foreach (var message in messages)
        {
            // Process message
            await _queueService.DeleteMessageAsync("notifications", message.MessageId, message.PopReceipt);
        }
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

### Using Contact Form Function

The contact form function provides a complete email contact form solution with validation and professional formatting:

```csharp
// The function is already implemented and ready to use
// POST /api/SubmitContactForm
// Headers: X-API-Key: your-api-key
// Body: { "name": "John Doe", "email": "john@example.com", "message": "Hello!" }

public class ContactFormFunction
{
    private readonly IEmailService _emailService;
    
    public ContactFormFunction(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [Function("SubmitContactForm")]
    public async Task<HttpResponseData> SubmitContactForm(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        // Automatically validates input, formats email, and sends to configured recipient
        // Returns appropriate success/error responses
    }
}
```

**Contact Form Request Example:**
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "message": "Hello, I would like to get in touch regarding your services."
}
```

**Email Output:**
The service automatically formats the contact form into a professional email with:
- Contact information (name, email, timestamp)
- Message content with proper formatting
- Technical details (IP address, user agent)
- Professional email template

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
- ✅ Queue Storage Service Operations
- ✅ Queue Name Validation
- ✅ Key Vault Integration (Email Service)
- ✅ Image Security Validation
- ✅ Utility Functions

**Current Status**: 185 of 187 tests passing (2 known issues with test configuration)

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

- **Image Processing**: Images are automatically converted to WebP format for optimal compression using SixLabors.ImageSharp v3.1.11
- **Security Constraints**: File size limits (50MB), dimension validation (8192x8192), and processing timeouts (30 seconds)
- **Pagination**: All list operations support pagination to handle large datasets
- **Caching**: Consider implementing caching strategies for frequently accessed data
- **Connection Pooling**: The platform uses Azure Identity with managed connections
- **Memory Management**: Image processing includes memory allocation limits to prevent DoS attacks

## Security Best Practices

- **Authentication Methods**: Choose between Managed Identity (recommended for production) or Connection String authentication
- **Key Vault Integration**: Use Azure Key Vault for secure credential management (SMTP, connection strings)
- **Environment Variables**: Use environment variables for sensitive configuration when Key Vault is not available
- **API Keys**: Use strong, randomly generated API keys (minimum 32 characters)
- **Azure Identity**: Leverage managed identities with modern DefaultAzureCredential options for secure Azure service authentication
- **Input Validation**: All inputs are validated before processing with size and format constraints
- **Image Security**: File size limits, dimension validation, format verification, and processing timeouts prevent abuse
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
