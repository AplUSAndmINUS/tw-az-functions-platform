# Azure Functions PaaS Platform

Azure Functions PaaS Platform is a comprehensive .NET 8 Azure Functions platform with isolated process model that provides reusable storage, image processing, email, and document conversion services for building Azure Functions applications.

**This is a Platform-as-a-Service (PaaS) solution designed to provide reusable services and infrastructure. The included functions (Ping, ContactForm, QueueMessage) are for local testing and validation purposes only. Production implementations should consume the platform's services rather than adding new functions to this codebase.**

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

# Build solution from root (for all projects) - NEVER CANCEL: Takes 40 seconds typically
dotnet build
# TIMEOUT: Set to 180+ seconds (3 minutes) for safety
# NOTE: 3 nullable reference warnings are expected and non-critical

# RECOMMENDED: Build from Functions directory for testing the complete platform
cd src/Functions
dotnet build
# This is the primary build location for the PaaS platform testing

# Run tests - NEVER CANCEL: Takes 30 seconds typically  
cd ../../  # Back to root for tests
dotnet test
# TIMEOUT: Set to 120+ seconds (2 minutes) for safety
# NOTE: 185/187 tests pass (2 known failures are acceptable)
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

**Optional Key Vault Integration**:
Add this setting to enable Key Vault for secure credential management:
```json
"AZURE_KEY_VAULT_URL": "https://your-keyvault.vault.azure.net/"
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
  Ping: [GET] http://localhost:7071/Ping
  QueueMessageFunction: [POST] http://localhost:7071/QueueMessage
  SubmitContactForm: [POST] http://localhost:7071/SubmitContactForm

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
# Expected: 185/187 tests pass (2 known failures are acceptable)
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
curl http://localhost:7071/Ping
# Expected: {"status": "OK", "message": "PaaS Platform is running", "timestamp": "..."}
```

#### 2. Contact Form (with API Key)
```bash
curl -X POST http://localhost:7071/SubmitContactForm \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-with-at-least-32-characters-minimum" \
  -d '{"name": "Test User", "email": "test@example.com", "message": "Test message"}'
# Expected: Success response if SMTP configured, or configuration error if not
```

#### 3. Queue Operations
```bash
curl -X POST http://localhost:7071/QueueMessage \
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
- **`src/Functions/`** - Azure Functions endpoints with HTTP triggers (**FOR TESTING ONLY**)
  - `BlogPosts/Functions/` - Test function implementations (Ping, QueueMessage, ContactForm)
  - `host.json` - Azure Functions runtime configuration (routePrefix: "" removes /api)
  - `Program.cs` - Dependency injection and startup configuration

- **`SharedStorage/`** - **CORE PaaS SERVICES** - Storage service implementations
  - `Services/BaseServices/` - Core storage services (Blob, Table, Queue, CosmosDB)
  - `Services/Email/` - SMTP email service with Key Vault integration and professional formatting
  - `Services/Media/` - Image processing (SixLabors.ImageSharp v3.1.11), document conversion, media handling
  - `Validators/` - Azure resource validation
  - `Extensions/` - Service registration helpers with Key Vault support

- **`Utils/`** - **CORE PaaS UTILITIES** - Utility classes and helpers
  - `Validation/` - API key validation with Azure Key Vault integration
  - `Constants/` - Shared constant values and API URLs
  - `AppInsightsLogger.cs` - Application Insights telemetry
  - `CdnUrlBuilder.cs` - CDN URL generation

- **`Tests/`** - Comprehensive xUnit test suite
  - 187 total tests covering all major components
  - 185 tests passing, 2 known failures (API key validation message format, image security test configuration)

### Key Features Tested
- **Storage Operations**: Blob upload/download, Table CRUD, Queue messaging, CosmosDB documents
- **Image Processing**: WebP conversion with SixLabors.ImageSharp v3.1.11, thumbnail generation, metadata extraction
- **Email Service**: Contact form processing with Key Vault integration and professional email templates
- **Document Processing**: Text extraction, format conversion
- **Security**: API key validation, Azure Identity authentication, image security validation
- **Key Vault Integration**: Secure credential management for SMTP and other services

### Configuration Requirements
- **API Keys**: Must be minimum 32 characters long
- **Storage**: Supports both Managed Identity and Connection String authentication
- **Email**: SMTP configuration required for contact form functionality (supports Key Vault)
- **Key Vault**: Optional Azure Key Vault integration for secure credential management
- **Image Security**: File size limits (50MB), dimension validation (8192x8192), processing timeouts (30 seconds)
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
├── Tests/                      # xUnit test project (185/187 tests passing)
├── README.md                   # Comprehensive documentation
├── AZURE_AUTHENTICATION_UPDATES.md # Latest authentication changes
├── SECURITY_CONFIGURATION.md   # Image processing security features
├── tw-az-functions-platform.sln # Main solution file
├── .prettierrc                 # Code formatting config
└── .vscode/extensions.json     # Recommended VS Code extensions
```

#### Key Files Always Check After Changes
- `SharedStorage/Extensions/ServiceCollectionExtensions.cs` - Service registration (includes Key Vault integration)
- `src/Functions/Program.cs` - DI configuration and startup
- `Utils/Constants/ApiUrls.cs` - API endpoint definitions
- `SharedStorage/Services/Email/EmailService.cs` - Email functionality with Key Vault support
- `SharedStorage/Services/Media/ImageConversionService.cs` - ImageSharp v3.1.11 compatibility
- `SharedStorage/Services/Media/ThumbnailService.cs` - Enhanced thumbnail generation
- `Tests/` - Always run tests after service changes (185/187 passing expected)

#### Required Environment Variables
- `StorageAccountName` - Azure Storage account name
- `CosmosAccountName` - CosmosDB account name (optional)
- `API_KEY` - Minimum 32 characters
- `AZURE_KEY_VAULT_URL` - Key Vault URL for secure credentials (optional)
- `SMTP_*` settings - Required for email functionality (can be in Key Vault)
- `USE_CONNECTION_STRING` - Toggle authentication method

## Security and Upgrade Management

### Security Update Guidelines

**CRITICAL**: All security updates must be handled gracefully with thorough testing and validation.

#### Security Update Process
1. **Assessment Phase**:
   ```bash
   # Check for security vulnerabilities
   dotnet list package --vulnerable
   dotnet list package --deprecated
   ```

2. **Testing Phase**:
   ```bash
   # Before applying security updates, establish baseline
   cd src/Functions
   dotnet build  # Must succeed
   cd ../../
   dotnet test   # Must show 183/184 tests passing
   ```

3. **Update Application**:
   ```bash
   # Update packages with security fixes
   dotnet add package [PackageName] --version [SecureVersion]
   # OR update all packages
   dotnet restore --force
   ```

4. **Validation Phase**:
   ```bash
   # Full validation after security updates
   dotnet clean
   dotnet restore
   cd src/Functions && dotnet build && cd ../..
   dotnet test
   # Functions runtime test
   cd src/Functions && func start &
   sleep 30
   curl http://localhost:7071/Ping  # Must return OK status
   pkill -f func
   ```

#### Critical Security Areas
- **API Key Validation**: Minimum 32 characters, Azure Key Vault integration
- **Storage Authentication**: Support for Managed Identity and Connection String methods
- **SMTP Configuration**: Secure app passwords, TLS encryption
- **Azure Identity**: Proper credential chain configuration

### Bicep Package Upgrade Procedures

**REQUIREMENT**: All Bicep package upgrades must be thoroughly tested and documented.

#### Pre-Upgrade Checklist
- [ ] Document current Bicep version and dependencies
- [ ] Review breaking changes in target version
- [ ] Backup current infrastructure templates
- [ ] Test upgrade in isolated environment first

#### Upgrade Testing Process
1. **Infrastructure Validation**:
   ```bash
   # Validate current Bicep templates
   az bicep build --file infrastructure/main.bicep
   az deployment group validate --resource-group test-rg --template-file main.json
   ```

2. **Service Integration Testing**:
   ```bash
   # Test platform services after infrastructure changes
   cd src/Functions
   dotnet build && func start &
   # Run full endpoint validation suite
   curl http://localhost:7071/Ping
   curl -X POST http://localhost:7071/QueueMessage -H "X-API-Key: test-key-with-32-characters-minimum" -d '{"queueName":"test","message":"test"}'
   ```

3. **Documentation Requirements**:
   - Document all breaking changes and migration steps
   - Update configuration examples
   - Verify environment variable requirements
   - Test both Managed Identity and Connection String authentication

### Authentication Method Versatility

**DESIGN PRINCIPLE**: The system must support multiple authentication methods for maximum flexibility.

#### Supported Authentication Methods
1. **Azure Managed Identity** (Recommended for Production):
   ```json
   {
     "StorageAccountName": "your-storage-account",
     "CosmosAccountName": "your-cosmos-account"
   }
   ```

2. **Connection String Authentication** (Development/Legacy):
   ```json
   {
     "USE_CONNECTION_STRING": "true",
     "AZURE_STORAGE_CONNECTION_STRING": "DefaultEndpointsProtocol=https;AccountName=...",
     "COSMOS_CONNECTION_STRING": "AccountEndpoint=https://..."
   }
   ```

3. **API Key Authentication** (Function Access):
   ```json
   {
     "API_KEY": "minimum-32-character-secure-key-for-function-access"
   }
   ```

4. **Azure Key Vault Integration** (Secret Management):
   ```json
   {
     "KEY_VAULT_URL": "https://your-keyvault.vault.azure.net/",
     "USE_KEY_VAULT": "true"
   }
   ```

#### Testing All Authentication Methods
```bash
# Test Managed Identity (requires Azure environment)
export StorageAccountName="testaccount"
cd src/Functions && func start

# Test Connection String (local development)
export USE_CONNECTION_STRING="true"
export AZURE_STORAGE_CONNECTION_STRING="UseDevelopmentStorage=true"
cd src/Functions && func start

# Validate API key security
curl -H "X-API-Key: invalid-short-key" http://localhost:7071/QueueMessage  # Should fail
curl -H "X-API-Key: valid-32-character-minimum-length-key" http://localhost:7071/QueueMessage  # Should process
```

### Feature Development Guidelines

**ROBUSTNESS vs NECESSITY**: Include features for robustness, but avoid unnecessary additions.

#### Feature Inclusion Criteria
✅ **Include if**:
- Enhances platform reliability or security
- Provides essential PaaS service capability
- Improves error handling or observability
- Supports multiple authentication methods
- Enables better testing or validation

❌ **Avoid if**:
- Feature is application-specific rather than platform-level
- Adds complexity without clear robustness benefit
- Duplicates existing functionality
- Creates tight coupling between services
- Requires application-specific business logic

#### Platform Service Examples (GOOD)
- Storage services (Blob, Table, Queue, CosmosDB)
- Image processing and conversion utilities
- Email service with templating
- Document processing capabilities
- Authentication and validation utilities
- Logging and monitoring integration

#### Application Features Examples (AVOID)
- Blog post management logic
- User account management
- Content management workflows
- Business-specific data models
- Application routing and navigation

### Sync Evaluation with az-tw-website-functions

**REQUIREMENT**: Evaluate "Sync" user stories from az-tw-website-functions repository for platform compatibility.

#### Sync Evaluation Process
1. **Repository Analysis**:
   ```bash
   # Compare function signatures and capabilities
   git clone https://github.com/[org]/az-tw-website-functions.git /tmp/website-functions
   cd /tmp/website-functions
   find . -name "*.cs" -exec grep -l "Function\|HttpTrigger" {} \;
   ```

2. **Compatibility Assessment**:
   - Review function patterns and dependencies
   - Identify platform services that could be extracted
   - Evaluate authentication method compatibility
   - Check for PaaS-appropriate abstractions

3. **Implementation Decision Matrix**:
   | Sync Feature | Platform Service? | Implementation Required? | Testing Approach |
   |-------------|-------------------|-------------------------|------------------|
   | User Auth   | Yes - Auth Service | Extract to Utils/       | Full auth flow   |
   | Blog Logic  | No - App Specific | Skip                   | N/A              |
   | File Upload | Yes - Storage     | Enhance SharedStorage/ | Upload + validation |

4. **Implementation and Testing**:
   ```bash
   # After extracting platform services from sync evaluation
   cd src/Functions && dotnet build  # Must build successfully
   cd ../../ && dotnet test          # Must maintain 183/184 test pass rate
   cd src/Functions && func start    # Must start and serve test functions
   # Run comprehensive platform service tests
   ```

#### Documentation Requirements for Sync Updates
- Document extracted services and their capabilities
- Provide migration guide for consuming applications
- Update authentication method examples
- Add testing scenarios for new platform services
- Maintain backward compatibility where possible

**CRITICAL**: Any sync-derived updates must maintain the PaaS nature of the platform and avoid introducing application-specific logic.

## Troubleshooting

### Common Issues and Solutions

**Build Fails with Nullable Warnings**:
- 3 nullable reference warnings in `Tests/EmailServiceValidationTests.cs` are expected
- These warnings do not affect functionality

**Test Failure in API Key Validation**:
- 1 test expects "Invalid API key." but gets "API key must be at least 32 characters long"
- This is a test expectation issue, not a functional problem
- 183/184 tests passing is acceptable

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