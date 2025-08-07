using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Utils;

/// <summary>
/// Custom telemetry initializer that adds common properties to all telemetry data
/// </summary>
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        // Add application name if not already set
        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
        {
            telemetry.Context.Cloud.RoleName = "tw-az-functions-platform";
        }

        // Add version information if available
        var version = System.Environment.GetEnvironmentVariable("APPLICATION_VERSION") ?? "1.0.0";
        telemetry.Context.GlobalProperties["ApplicationVersion"] = version;

        // Add environment information
        var environment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? 
                         System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                         "Unknown";
        telemetry.Context.GlobalProperties["Environment"] = environment;

        // Add custom properties
        telemetry.Context.GlobalProperties["Platform"] = "Azure Functions";
        telemetry.Context.GlobalProperties["Framework"] = ".NET 8.0";

        // Add function app name if running in Azure Functions
        var functionAppName = System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        if (!string.IsNullOrEmpty(functionAppName))
        {
            telemetry.Context.GlobalProperties["FunctionAppName"] = functionAppName;
        }

        // Add region if available
        var region = System.Environment.GetEnvironmentVariable("REGION_NAME") ?? 
                    System.Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP");
        if (!string.IsNullOrEmpty(region))
        {
            telemetry.Context.GlobalProperties["Region"] = region;
        }
    }
}