using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Moq;
using SharedStorage.Services.Email;
using Utils;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Tests;

public class EmailServiceTelemetryTests
{
    [Fact]
    public async Task SendEmailAsync_Success_ShouldLogApiCallMetrics()
    {
        // Arrange
        var mockLogger = new Mock<IAppInsightsLogger<EmailService>>();
        
        // Set up environment variables for email service
        System.Environment.SetEnvironmentVariable("SMTP_USERNAME", "test@example.com");
        System.Environment.SetEnvironmentVariable("SMTP_PASSWORD", "testpassword");
        System.Environment.SetEnvironmentVariable("TO_EMAIL", "recipient@example.com");

        var emailService = new EmailService(mockLogger.Object);

        // Act & Assert - This will fail because we don't have actual SMTP credentials
        // But we can verify that the telemetry logging would be called
        try
        {
            await emailService.SendEmailAsync("test@example.com", "Test Subject", "Test Body");
        }
        catch (Exception)
        {
            // Expected to fail due to lack of real SMTP server
            // The important thing is that the telemetry logging methods would be called
        }

        // Verify that the logging methods were called
        mockLogger.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Sending email")), It.IsAny<object[]>()),
            Times.Once);

        // Verify that LogApiCall was called (either for success or failure)
        mockLogger.Verify(
            x => x.LogApiCall("SMTP", "SendEmail", It.IsAny<TimeSpan>(), It.IsAny<bool>()),
            Times.Once);

        // Clean up environment variables
        System.Environment.SetEnvironmentVariable("SMTP_USERNAME", null);
        System.Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
        System.Environment.SetEnvironmentVariable("TO_EMAIL", null);
    }

    [Fact]
    public void EmailService_Constructor_ShouldHandleNullLogger()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailService(null!));
    }

    [Theory]
    [InlineData("", "Subject", "Body")]
    [InlineData("invalid-email", "Subject", "Body")]
    [InlineData("test@example.com", "", "Body")]
    [InlineData("test@example.com", "Subject", "")]
    public async Task SendEmailAsync_InvalidParameters_ShouldThrowArgumentException(string to, string subject, string body)
    {
        // Arrange
        var mockLogger = new Mock<IAppInsightsLogger<EmailService>>();
        
        // Set up environment variables for email service
        System.Environment.SetEnvironmentVariable("SMTP_USERNAME", "test@example.com");
        System.Environment.SetEnvironmentVariable("SMTP_PASSWORD", "testpassword");

        var emailService = new EmailService(mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            emailService.SendEmailAsync(to, subject, body));

        // Clean up environment variables
        System.Environment.SetEnvironmentVariable("SMTP_USERNAME", null);
        System.Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
    }
}