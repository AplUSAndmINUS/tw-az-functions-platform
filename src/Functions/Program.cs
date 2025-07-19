using SharedStorage.Services;
using SharedStorage.Extensions;
using Utils;
using Utils.Validation;
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

                // Register API Key validation services (includes Key Vault services if configured)
                services.AddApiKeyValidation(configuration);

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