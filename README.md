# Azure Functions Platform

This repository contains an Azure Functions platform that powers the backend services for TerenceWaters.com.

## Overview

This project is an Azure Functions platform written in .NET 8 isolated that provide APIs for managing website content in table and blob storage. The functions use Azure Blob Storage for file storage and Azure Table Storage for structured data.

## Project Structure

-   **src/Functions/** - Contains all Azure Functions endpoints with isolated process model
    -   **BlogPosts/** - Sample functions for blog content
        -   **Functions/** - HTTP-triggered functions
        -   **Models/** - Data models for blog posts
-   **SharedStorage/** - Shared storage services
    -   **Services/** - Azure Blob and Table Storage service implementations
        -   **BlobStorageService.cs** - Handles blob storage operations with image processing
        -   **TableStorageService.cs** - Manages table storage operations
        -   **ImageConversionService.cs** - Converts images to WebP format
        -   **ThumbnailService.cs** - Generates thumbnails for images
    -   **Validators/** - Input validation for Azure resources
    -   **Environment/** - Environment-specific configuration
-   **Utils/** - Utility classes and helpers
    -   **Constants/** - Shared constant values
    -   **Validation/** - API key validation and other validators
    -   **AppInsightsLogger.cs** - Application Insights integration
    -   **CdnUrlBuilder.cs** - URL generation for CDN resources
    -   **ContentNameResolver.cs** - Resolves container and table names

## Features

-   Isolated process model (.NET 8 Azure Functions)
-   Blob storage integration with WebP image conversion
-   Table storage for structured data
-   Thumbnail generation for uploaded images
-   API key authentication
-   Application Insights integration for monitoring
-   CDN URL resolution for public assets

## Quick Start

### Prerequisites

-   .NET 8 SDK
-   Azure Functions Core Tools v4
-   Azure Storage Account (or Azurite for local development)
-   Visual Studio Code or Visual Studio 2022

### Local Development

1. Clone this repository
2. Configure your `local.settings.json` file:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=false",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "AZURE_CREDENTIALS": "your-azure-credentials-JSON",
        "StorageAccountName": "your-storage-account-name",
        "X_API_ENVIRONMENT_KEY": "your-api-key"
    }
}
```
