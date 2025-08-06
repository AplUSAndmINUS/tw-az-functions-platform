using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Utils.Middleware;

/// <summary>
/// Middleware for enriching telemetry data with request context information
/// </summary>
public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, TelemetryClient telemetryClient, ILogger<TelemetryMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start tracking the request
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Add request context to telemetry
            AddRequestContextToTelemetry(context);

            // Track the request start
            _telemetryClient.TrackTrace($"Request started: {context.Request.Method} {context.Request.Path}", 
                SeverityLevel.Information, 
                GetRequestProperties(context));

            // Call the next middleware
            await _next(context);

            // Track successful request completion
            var duration = DateTime.UtcNow - startTime;
            TrackRequestCompletion(context, duration, true);
        }
        catch (Exception ex)
        {
            // Track failed request completion
            var duration = DateTime.UtcNow - startTime;
            TrackRequestCompletion(context, duration, false, ex);
            
            _logger.LogError(ex, "Request failed: {Method} {Path}", context.Request.Method, context.Request.Path);
            throw;
        }
    }

    private void AddRequestContextToTelemetry(HttpContext context)
    {
        // Add correlation ID if available
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? 
                           context.Request.Headers["X-Request-ID"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(correlationId))
        {
            _telemetryClient.Context.GlobalProperties["CorrelationId"] = correlationId;
        }

        // Add user agent
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userAgent))
        {
            _telemetryClient.Context.GlobalProperties["UserAgent"] = userAgent;
        }

        // Add client IP
        var clientIp = GetClientIpAddress(context);
        if (!string.IsNullOrEmpty(clientIp))
        {
            _telemetryClient.Context.GlobalProperties["ClientIp"] = clientIp;
        }
    }

    private Dictionary<string, string> GetRequestProperties(HttpContext context)
    {
        var properties = new Dictionary<string, string>
        {
            { "Method", context.Request.Method },
            { "Path", context.Request.Path },
            { "QueryString", context.Request.QueryString.ToString() },
            { "Protocol", context.Request.Protocol },
            { "Scheme", context.Request.Scheme },
            { "Host", context.Request.Host.ToString() }
        };

        // Add headers that might be useful for debugging
        if (context.Request.Headers.ContainsKey("Referer"))
        {
            properties["Referer"] = context.Request.Headers["Referer"].ToString();
        }

        return properties;
    }

    private void TrackRequestCompletion(HttpContext context, TimeSpan duration, bool success, Exception? exception = null)
    {
        var properties = GetRequestProperties(context);
        properties["Success"] = success.ToString();
        properties["StatusCode"] = context.Response.StatusCode.ToString();
        properties["Duration"] = duration.TotalMilliseconds.ToString("F2");

        var metrics = new Dictionary<string, double>
        {
            { "Duration", duration.TotalMilliseconds },
            { "StatusCode", context.Response.StatusCode }
        };

        if (exception != null)
        {
            properties["ExceptionType"] = exception.GetType().Name;
            properties["ExceptionMessage"] = exception.Message;
            
            _telemetryClient.TrackException(exception, properties, metrics);
        }

        _telemetryClient.TrackEvent("RequestCompleted", properties, metrics);

        // Also track as a request telemetry item
        var requestTelemetry = new RequestTelemetry(
            context.Request.Method + " " + context.Request.Path,
            DateTimeOffset.UtcNow - duration,
            duration,
            context.Response.StatusCode.ToString(),
            success);

        foreach (var prop in properties)
        {
            requestTelemetry.Properties[prop.Key] = prop.Value;
        }

        _telemetryClient.TrackRequest(requestTelemetry);
    }

    private string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (common in Azure)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // Check for real IP
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}