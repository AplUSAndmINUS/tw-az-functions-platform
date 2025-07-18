using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SharedStorage.Services.Email;
using System.Net;
using System.Text.Json;
using Utils;
using Utils.Validation;

namespace Functions.BlogPosts.Functions;

public class ContactFormFunction
{
    private readonly ILogger<ContactFormFunction> _logger;
    private readonly IEmailService _emailService;
    private readonly IAPIKeyValidator _apiKeyValidator;
    private readonly IAppInsightsLogger<ContactFormFunction> _appInsightsLogger;

    public ContactFormFunction(
        ILogger<ContactFormFunction> logger,
        IEmailService emailService,
        IAPIKeyValidator apiKeyValidator,
        IAppInsightsLogger<ContactFormFunction> appInsightsLogger)
    {
        _logger = logger;
        _emailService = emailService;
        _apiKeyValidator = apiKeyValidator;
        _appInsightsLogger = appInsightsLogger;
    }

    [Function("SubmitContactForm")]
    public async Task<HttpResponseData> SubmitContactForm(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("SubmitContactForm function processed a request.");
        _appInsightsLogger.LogInformation("Contact form submission received");

        try
        {
            // Validate API key
            var apiKey = req.Headers.GetValues("X-API-Key").FirstOrDefault();
            if (!_apiKeyValidator.IsValid(apiKey ?? string.Empty))
            {
                _appInsightsLogger.LogWarning("Invalid API key provided for contact form submission");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Invalid API key.");
                return unauthorizedResponse;
            }

            // Read and parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request body cannot be empty.");
                return badResponse;
            }

            var contactData = JsonSerializer.Deserialize<ContactFormData>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (contactData == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid contact form data.");
                return badResponse;
            }

            // Validate required fields
            var validationErrors = ValidateContactData(contactData);
            if (validationErrors.Any())
            {
                var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await validationResponse.WriteAsJsonAsync(new
                {
                    success = false,
                    errors = validationErrors,
                    message = "Validation failed"
                });
                return validationResponse;
            }

            // Get client information
            var userAgent = req.Headers.GetValues("User-Agent").FirstOrDefault() ?? "";
            var ipAddress = req.Headers.GetValues("X-Forwarded-For").FirstOrDefault() ?? 
                           req.Headers.GetValues("X-Real-IP").FirstOrDefault() ?? "";

            // Format email content
            var emailBody = _emailService.FormatContactEmail(
                contactData.Name,
                contactData.Email,
                contactData.Message,
                DateTime.UtcNow,
                userAgent,
                ipAddress
            );

            // Get recipient email from environment
            var toEmail = System.Environment.GetEnvironmentVariable("TO_EMAIL") ?? 
                         System.Environment.GetEnvironmentVariable("SMTP_USERNAME");

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _appInsightsLogger.LogError("TO_EMAIL environment variable not configured", new InvalidOperationException("TO_EMAIL not configured"));
                var configResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await configResponse.WriteStringAsync("Email configuration error.");
                return configResponse;
            }

            // Send email
            var subject = $"Contact Form Submission from {contactData.Name}";
            await _emailService.SendEmailAsync(toEmail, subject, emailBody);

            _appInsightsLogger.LogInformation("Contact form email sent successfully to {ToEmail} from {FromEmail}", 
                toEmail, contactData.Email);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                message = "Contact form submitted successfully"
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            _appInsightsLogger.LogError("Contact form submission failed", ex);
            
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while processing your request.");
            return errorResponse;
        }
    }

    private static List<string> ValidateContactData(ContactFormData contactData)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(contactData.Name))
            errors.Add("Name is required");

        if (string.IsNullOrWhiteSpace(contactData.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(contactData.Email))
            errors.Add("Email format is invalid");

        if (string.IsNullOrWhiteSpace(contactData.Message))
            errors.Add("Message is required");

        if (contactData.Name?.Length > 100)
            errors.Add("Name must be less than 100 characters");

        if (contactData.Email?.Length > 255)
            errors.Add("Email must be less than 255 characters");

        if (contactData.Message?.Length > 5000)
            errors.Add("Message must be less than 5000 characters");

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class ContactFormData
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}