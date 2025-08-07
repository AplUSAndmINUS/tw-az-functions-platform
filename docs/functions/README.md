# Azure Functions API Documentation

This directory contains automatically generated documentation for all Azure Functions in the application.

## Functions by Category

### BlogPosts

- [Ping](./Ping.md) - GET - Azure Function endpoint for Ping operations
- [SendQueueMessage](./SendQueueMessage.md) - POST - Azure Function endpoint for SendQueueMessage operations
- [SubmitContactForm](./SubmitContactForm.md) - POST - Processes contact form submissions and sends notifications to the appropriate recipients.

## Usage Notes

- All endpoints require API key authentication via `x-api-key` header
- Base URL for local development: `http://localhost:7071`
- All dates should be in ISO 8601 format
- Media references must be stringified JSON arrays

#Function #Documentation #API
