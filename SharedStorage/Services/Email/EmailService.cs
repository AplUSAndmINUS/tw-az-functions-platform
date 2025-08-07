using System.Net;
using System.Net.Mail;
using System.Text;
using Utils;
using Utils.Services;

namespace SharedStorage.Services.Email;

/// <summary>
/// Email service implementation for sending formatted emails
/// Supports both Key Vault and environment variable authentication for SMTP credentials
/// </summary>
public class EmailService : IEmailService
{
    private readonly IAppInsightsLogger<EmailService> _logger;
    private readonly IKeyVaultService? _keyVaultService;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _toEmail;

    public EmailService(IAppInsightsLogger<EmailService> logger, IKeyVaultService? keyVaultService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyVaultService = keyVaultService;
        
        // Initialize SMTP configuration - try Key Vault first, fallback to environment variables
        var smtpConfig = InitializeSmtpConfigurationAsync().GetAwaiter().GetResult();
        
        _smtpHost = smtpConfig.Host;
        _smtpPort = smtpConfig.Port;
        _smtpUsername = smtpConfig.Username;
        _smtpPassword = smtpConfig.Password;
        _fromEmail = smtpConfig.FromEmail;
        _fromName = smtpConfig.FromName;
        _toEmail = smtpConfig.ToEmail;
    }

    /// <summary>
    /// Initializes SMTP configuration from Key Vault or environment variables
    /// </summary>
    private async Task<SmtpConfiguration> InitializeSmtpConfigurationAsync()
    {
        var config = new SmtpConfiguration();

        if (_keyVaultService != null)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve SMTP configuration from Key Vault");

                // Try to get SMTP configuration from Key Vault
                config.Host = await _keyVaultService.GetSecretAsync("SMTP-HOST", "smtp.gmail.com");
                var portSecret = await _keyVaultService.GetSecretAsync("SMTP-PORT", "587");
                config.Port = int.TryParse(portSecret, out int port) ? port : 587;
                config.Username = await _keyVaultService.GetSecretAsync("SMTP-USERNAME", 
                    System.Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? 
                    throw new InvalidOperationException("SMTP_USERNAME not found in Key Vault or environment variables"));
                config.Password = await _keyVaultService.GetSecretAsync("SMTP-PASSWORD", 
                    System.Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? 
                    throw new InvalidOperationException("SMTP_PASSWORD not found in Key Vault or environment variables"));
                config.FromEmail = await _keyVaultService.GetSecretAsync("FROM-EMAIL", config.Username);
                config.FromName = await _keyVaultService.GetSecretAsync("FROM-NAME", "{{YOUR_COMPANY_NAME}}");
                config.ToEmail = await _keyVaultService.GetSecretAsync("TO-EMAIL", config.Username);

                _logger.LogInformation("Successfully retrieved SMTP configuration from Key Vault");
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve SMTP configuration from Key Vault, falling back to environment variables", ex);
            }
        }

        // Fallback to environment variables (backward compatibility)
        _logger.LogInformation("Using SMTP configuration from environment variables");
        config.Host = System.Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        config.Port = int.TryParse(System.Environment.GetEnvironmentVariable("SMTP_PORT"), out int envPort) ? envPort : 587;
        config.Username = System.Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? 
            throw new InvalidOperationException("SMTP_USERNAME environment variable is required");
        config.Password = System.Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? 
            throw new InvalidOperationException("SMTP_PASSWORD environment variable is required");
        config.FromEmail = System.Environment.GetEnvironmentVariable("FROM_EMAIL") ?? config.Username;
        config.FromName = System.Environment.GetEnvironmentVariable("FROM_NAME") ?? "{{YOUR_COMPANY_NAME}}";
        config.ToEmail = System.Environment.GetEnvironmentVariable("TO_EMAIL") ?? config.Username;

        return config;
    }

    /// <summary>
    /// SMTP configuration container
    /// </summary>
    private class SmtpConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email address cannot be null or empty", nameof(to));
        
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Email subject cannot be null or empty", nameof(subject));
        
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Email body cannot be null or empty", nameof(body));

        // Validate email format
        if (!IsValidEmailAddress(to))
            throw new ArgumentException($"Invalid email address format: {to}", nameof(to));

        var startTime = DateTime.UtcNow;
        var success = false;

        try
        {
            _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            
            success = true;
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            
            // Log email operation metrics for telemetry
            _logger.LogApiCall("SMTP", "SendEmail", duration, success);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError("Failed to send email to {To}", ex, to);
            
            // Log failed email operation for telemetry
            _logger.LogApiCall("SMTP", "SendEmail", duration, success);
            throw;
        }
    }

    public string FormatContactEmail(string name, string email, string message, DateTime submittedAt, string userAgent = "", string ipAddress = "")
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine("CONTACT FORM SUBMISSION");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();
        
        sb.AppendLine($"From: {name}");
        sb.AppendLine($"Email: {email}");
        sb.AppendLine($"Submitted: {submittedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        
        sb.AppendLine("MESSAGE:");
        sb.AppendLine("-".PadRight(60, '-'));
        sb.AppendLine(message);
        sb.AppendLine("-".PadRight(60, '-'));
        sb.AppendLine();
        
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            sb.AppendLine("TECHNICAL DETAILS:");
            sb.AppendLine($"User Agent: {userAgent}");
        }
        
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            sb.AppendLine($"IP Address: {ipAddress}");
        }
        
        sb.AppendLine();
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine("End of submission");
        sb.AppendLine("=".PadRight(60, '='));
        
        return sb.ToString();
    }

    /// <summary>
    /// Validates if the provided email address has a valid format
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    private static bool IsValidEmailAddress(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}