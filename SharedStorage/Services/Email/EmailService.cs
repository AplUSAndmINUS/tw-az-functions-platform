using System.Net;
using System.Net.Mail;
using System.Text;
using Utils;

namespace SharedStorage.Services.Email;

/// <summary>
/// Email service implementation for sending formatted emails
/// </summary>
public class EmailService : IEmailService
{
    private readonly IAppInsightsLogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _toEmail;

    public EmailService(IAppInsightsLogger<EmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get SMTP configuration from environment variables
        _smtpHost = System.Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(System.Environment.GetEnvironmentVariable("SMTP_PORT"), out int port) ? port : 587;
        _smtpUsername = System.Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? throw new InvalidOperationException("SMTP_USERNAME environment variable is required");
        _smtpPassword = System.Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new InvalidOperationException("SMTP_PASSWORD environment variable is required");
        _fromEmail = System.Environment.GetEnvironmentVariable("FROM_EMAIL") ?? _smtpUsername;
        _fromName = System.Environment.GetEnvironmentVariable("FROM_NAME") ?? "{{YOUR_COMPANY_NAME}}";
        _toEmail = System.Environment.GetEnvironmentVariable("TO_EMAIL") ?? _smtpUsername;
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