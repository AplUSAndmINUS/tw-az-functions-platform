using SharedStorage.Services;
using SharedStorage.Extensions;
using Utils;
using Utils.Validation;
using Utils.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Core; // Add this
using Microsoft.Azure.Functions.Worker; // Add this for WorkerOptions

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                
                // Use the new extension method to register all shared storage services
                services.AddSharedStorageServices(configuration);

                // Register Key Vault Service
                services.AddSingleton<IKeyVaultService, KeyVaultService>();

                // Register APIKeyValidator - use Key Vault validator if AZURE_KEY_VAULT_URL is configured
                var keyVaultUrl = configuration["AZURE_KEY_VAULT_URL"] 
                    ?? System.Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL");
                
                if (!string.IsNullOrWhiteSpace(keyVaultUrl))
                {
                    // Use Key Vault-based API key validator
                    services.AddSingleton<IAPIKeyValidator, KeyVaultApiKeyValidator>();
                }
                else
                {
                    // Fall back to simple API key validator
                    services.AddSingleton<IAPIKeyValidator>(sp =>
                    {
                        var validApiKey = configuration["{{API_KEY_ENVIRONMENT_VARIABLE}}"]
                            ?? System.Environment.GetEnvironmentVariable("{{API_KEY_ENVIRONMENT_VARIABLE}}");
                        if (string.IsNullOrWhiteSpace(validApiKey))
                            throw new InvalidOperationException("Missing {{API_KEY_ENVIRONMENT_VARIABLE}} in configuration.");

                        return new ApiKeyValidator(validApiKey);
                    });
                }

                // Register Application Insights telemetry
                services.AddApplicationInsightsTelemetryWorkerService();
                services.AddSingleton<TelemetryClient>();

                // Register AppInsightsLogger
                services.AddSingleton<AppInsightsLogger>();
            })
            .ConfigureFunctionsWorkerDefaults()
            .Build();

        Console.WriteLine("Azure Functions PaaS Platform is starting...");

        host.Run();
    }
}