# Azure Functions PaaS Platform

Azure Functions PaaS Platform is a comprehensive .NET 8 Azure Functions platform with isolated process model that provides reusable storage, image processing, email, and document conversion services for building Azure Functions applications.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites and Setup
- **.NET 8 SDK**: Install from https://dotnet.microsoft.com/download/dotnet/8.0
- **Visual Studio Code**: Install Azure Functions extension: `ms-azuretools.vscode-azurefunctions`
- **Azure Functions Core Tools v4**: Required for local testing (see installation section below)
- **Azure Storage Account** or **Azurite** for local development storage emulation

### Quick Start Commands
Bootstrap, build, and test the solution using these **EXACT** commands:

```bash
# Clone and navigate to repository
git clone https://github.com/AplUSAndmINUS/tw-az-functions-platform.git
cd tw-az-functions-platform

# Restore packages - NEVER CANCEL: Takes 1.5 minutes on first run
dotnet restore
# TIMEOUT: Set to 300+ seconds (5 minutes) for package restoration

# Build solution - NEVER CANCEL: Takes 40 seconds typically
dotnet build
# TIMEOUT: Set to 180+ seconds (3 minutes) for safety
# NOTE: 3 nullable reference warnings are expected and non-critical

# Run tests - NEVER CANCEL: Takes 30 seconds typically  
dotnet test
# TIMEOUT: Set to 120+ seconds (2 minutes) for safety
# NOTE: 1 test failure expected (API key validation message mismatch) - 169/170 tests pass
```

### Azure Functions Core Tools Installation

**Critical**: Azure Functions Core Tools v4 is required to run functions locally.

**Method 1 - NPM (Recommended)**:
```bash
npm install -g azure-functions-core-tools@4 --unsafe-perm true
# TIMEOUT: Set to 600+ seconds (10 minutes) - download is large
```

**Method 2 - Manual Download** (if NPM fails):
```bash
# Linux x64
wget https://github.com/Azure/azure-functions-core-tools/releases/latest/download/Azure.Functions.Cli.linux-x64.zip
unzip Azure.Functions.Cli.linux-x64.zip -d ~/azure-functions-cli
export PATH="$HOME/azure-functions-cli:$PATH"
# Add to ~/.bashrc or ~/.zshrc for persistence
```

**Method 3 - Package Manager**:
```bash
# Ubuntu/Debian
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install azure-functions-core-tools-4
```

### Local Development Configuration

Create `src/Functions/local.settings.json` with this template:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "StorageAccountName": "your-storage-account-name",
        "CosmosAccountName": "your-cosmos-account-name",
        "API_KEY": "your-api-key-with-at-least-32-characters-minimum",
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

**Alternative for Connection String Authentication**:
Add these additional settings:
```json
"USE_CONNECTION_STRING": "true",
"AZURE_STORAGE_CONNECTION_STRING": "DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net"
```

### Running the Application

```bash
# Start Azure Functions runtime
cd src/Functions
func start
# NEVER CANCEL: Startup takes 30-60 seconds
# TIMEOUT: Set to 120+ seconds (2 minutes) for initial startup
```

**Expected Startup Output**:
```
Azure Functions Core Tools
Core Tools Version:       4.x.x
Function Runtime Version: 4.x.x

Functions:
  Ping: [GET] http://localhost:7071/api/Ping
  QueueMessageFunction: [POST] http://localhost:7071/api/QueueMessage
  SubmitContactForm: [POST] http://localhost:7071/api/SubmitContactForm

Host lock lease acquired by instance ID: xxx
```

## Validation

### Mandatory Build Validation Steps
**ALWAYS** run these commands after making changes:

```bash
# Build with warnings check
dotnet build
# Expected: 3 nullable reference warnings (acceptable)
# TIMEOUT: 180+ seconds

# Run full test suite
dotnet test
# Expected: 169/170 tests pass (1 known failure is acceptable)
# TIMEOUT: 120+ seconds

# Check for compilation errors in specific projects
dotnet build SharedStorage/SharedStorage.csproj
dotnet build Utils/Utils.csproj
dotnet build src/Functions/Functions.csproj
```

### Manual Functional Testing

After starting the Functions runtime, **ALWAYS** test these endpoints:

#### 1. Health Check Endpoint
```bash
curl http://localhost:7071/api/Ping
# Expected: {"status": "OK", "message": "PaaS Platform is running", "timestamp": "..."}
```

#### 2. Contact Form (with API Key)
```bash
curl -X POST http://localhost:7071/api/SubmitContactForm \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-with-at-least-32-characters-minimum" \
  -d '{"name": "Test User", "email": "test@example.com", "message": "Test message"}'
# Expected: Success response if SMTP configured, or configuration error if not
```

#### 3. Queue Operations
```bash
curl -X POST http://localhost:7071/api/QueueMessage \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-with-at-least-32-characters-minimum" \
  -d '{"queueName": "test-queue", "message": "Hello World"}'
# Expected: Queue message ID response or storage connection error
```

### Code Formatting Validation
**ALWAYS** format code before committing:

```bash
# Format code using Prettier (for any JSON/config files)
npx prettier --write "**/*.json"

# Format C# code using built-in formatter
dotnet format
# TIMEOUT: 60+ seconds for large codebases
```

## Architecture and Key Components

### Project Structure
- **`src/Functions/`** - Azure Functions endpoints with HTTP triggers
  - `BlogPosts/Functions/` - Function implementations (Ping, QueueMessage, ContactForm)
  - `host.json` - Azure Functions runtime configuration
  - `Program.cs` - Dependency injection and startup configuration

- **`SharedStorage/`** - Storage service implementations
  - `Services/BaseServices/` - Core storage services (Blob, Table, Queue, CosmosDB)
  - `Services/Email/` - SMTP email service with professional formatting
  - `Services/` - Image processing, document conversion, media handling
  - `Validators/` - Azure resource validation
  - `Extensions/` - Service registration helpers

- **`Utils/`** - Utility classes and helpers
  - `Validation/` - API key validation with Azure Key Vault integration
  - `Constants/` - Shared constant values and API URLs
  - `AppInsightsLogger.cs` - Application Insights telemetry
  - `CdnUrlBuilder.cs` - CDN URL generation

- **`Tests/`** - Comprehensive xUnit test suite
  - 170 total tests covering all major components
  - 1 known failing test (API key validation message mismatch)

### Key Features Tested
- **Storage Operations**: Blob upload/download, Table CRUD, Queue messaging, CosmosDB documents
- **Image Processing**: WebP conversion, thumbnail generation, metadata extraction
- **Email Service**: Contact form processing with professional email templates
- **Document Processing**: Text extraction, format conversion
- **Security**: API key validation, Azure Identity authentication

### Configuration Requirements
- **API Keys**: Must be minimum 32 characters long
- **Storage**: Supports both Managed Identity and Connection String authentication
- **Email**: SMTP configuration required for contact form functionality
- **CosmosDB**: Optional, only required for document storage features

## Performance and Timing Expectations

### Build Times (Set appropriate timeouts)
- **Fresh clone + restore**: 1.5 minutes - **TIMEOUT: 300+ seconds**
- **Incremental build**: 40 seconds - **TIMEOUT: 180+ seconds**
- **Test execution**: 30 seconds - **TIMEOUT: 120+ seconds**
- **Function startup**: 30-60 seconds - **TIMEOUT: 120+ seconds**

### Development Workflow Times
- **Code formatting**: 5-10 seconds - **TIMEOUT: 60+ seconds**
- **Solution rebuild**: 45-60 seconds - **TIMEOUT: 180+ seconds**
- **Full CI simulation**: 3-4 minutes total

**CRITICAL**: NEVER CANCEL long-running operations. Azure Functions and .NET builds can take several minutes on first run or after package updates.

## Common Tasks

### Frequently Run Commands Output Reference
Use these instead of searching or running bash commands to save time:

#### Repository Root Structure
```
├── src/Functions/              # Azure Functions project
├── SharedStorage/              # Storage services library  
├── Utils/                      # Utility classes
├── Tests/                      # xUnit test project
├── README.md                   # Comprehensive documentation
├── tw-az-functions-platform.sln # Main solution file
├── .prettierrc                 # Code formatting config
└── .vscode/extensions.json     # Recommended VS Code extensions
```

#### Key Files Always Check After Changes
- `SharedStorage/Extensions/ServiceCollectionExtensions.cs` - Service registration
- `src/Functions/Program.cs` - DI configuration and startup
- `Utils/Constants/ApiUrls.cs` - API endpoint definitions
- `SharedStorage/Services/Email/EmailService.cs` - Email functionality
- `Tests/` - Always run tests after service changes

#### Required Environment Variables
- `StorageAccountName` - Azure Storage account name
- `CosmosAccountName` - CosmosDB account name (optional)
- `API_KEY` - Minimum 32 characters
- `SMTP_*` settings - Required for email functionality
- `USE_CONNECTION_STRING` - Toggle authentication method

## Troubleshooting

### Common Issues and Solutions

**Build Fails with Nullable Warnings**:
- 3 nullable reference warnings in `Tests/EmailServiceValidationTests.cs` are expected
- These warnings do not affect functionality

**Test Failure in API Key Validation**:
- 1 test expects "Invalid API key." but gets "API key must be at least 32 characters long"
- This is a test expectation issue, not a functional problem
- 169/170 tests passing is acceptable

**Functions Won't Start**:
- Ensure Azure Functions Core Tools v4 is installed
- Verify `local.settings.json` exists in `src/Functions/`
- Check that API_KEY is at least 32 characters
- Confirm FUNCTIONS_WORKER_RUNTIME is set to "dotnet-isolated"

**Storage Connection Errors**:
- For local development, install Azurite storage emulator
- Verify AzureWebJobsStorage is set to "UseDevelopmentStorage=true"
- For Azure, ensure StorageAccountName and authentication method are configured

**Email Service Errors**:
- SMTP settings are required for contact form functionality
- Use app passwords for Gmail SMTP authentication
- Verify firewall allows outbound SMTP connections

### Network or Installation Issues
If Azure Functions Core Tools installation fails:
1. Try npm installation with different Node.js versions
2. Use manual download method with direct GitHub releases
3. Check for corporate firewall blocking downloads
4. Consider using Azure Functions in containers as alternative

Always set generous timeouts and never cancel builds or tests prematurely.