using Microsoft.Extensions.Logging;
using Moq;
using SharedStorage.Services.Email;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Services;
using Xunit;

namespace Tests
{
    public class EmailServiceKeyVaultIntegrationTests
    {
        private readonly Mock<IAppInsightsLogger<EmailService>> _mockLogger;
        private readonly Mock<IKeyVaultService> _mockKeyVaultService;

        public EmailServiceKeyVaultIntegrationTests()
        {
            _mockLogger = new Mock<IAppInsightsLogger<EmailService>>();
            _mockKeyVaultService = new Mock<IKeyVaultService>();
        }

        [Fact]
        public void EmailService_WithoutKeyVault_UseEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SMTP_USERNAME", "test@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "testpassword");
            Environment.SetEnvironmentVariable("SMTP_HOST", "smtp.test.com");
            Environment.SetEnvironmentVariable("SMTP_PORT", "587");

            // Act
            var emailService = new EmailService(_mockLogger.Object);

            // Assert - No exceptions should be thrown
            Assert.NotNull(emailService);
            
            // Cleanup
            Environment.SetEnvironmentVariable("SMTP_USERNAME", null);
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
            Environment.SetEnvironmentVariable("SMTP_HOST", null);
            Environment.SetEnvironmentVariable("SMTP_PORT", null);
        }

        [Fact]
        public void EmailService_WithKeyVaultService_UsesKeyVaultWhenAvailable()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SMTP_USERNAME", "env-test@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "env-testpassword");
            
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("SMTP-HOST", "smtp.gmail.com", default))
                .ReturnsAsync("smtp.vault.com");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("SMTP-PORT", "587", default))
                .ReturnsAsync("587");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("SMTP-USERNAME", It.IsAny<string>(), default))
                .ReturnsAsync("vault-test@example.com");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("SMTP-PASSWORD", It.IsAny<string>(), default))
                .ReturnsAsync("vault-testpassword");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("FROM-EMAIL", It.IsAny<string>(), default))
                .ReturnsAsync("vault-test@example.com");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("FROM-NAME", "{{YOUR_COMPANY_NAME}}", default))
                .ReturnsAsync("Test Company");
            _mockKeyVaultService.Setup(x => x.GetSecretAsync("TO-EMAIL", It.IsAny<string>(), default))
                .ReturnsAsync("vault-test@example.com");

            // Act
            var emailService = new EmailService(_mockLogger.Object, _mockKeyVaultService.Object);

            // Assert
            Assert.NotNull(emailService);
            _mockKeyVaultService.Verify(x => x.GetSecretAsync("SMTP-USERNAME", It.IsAny<string>(), default), Times.Once);
            _mockKeyVaultService.Verify(x => x.GetSecretAsync("SMTP-PASSWORD", It.IsAny<string>(), default), Times.Once);
            
            // Cleanup
            Environment.SetEnvironmentVariable("SMTP_USERNAME", null);
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
        }

        [Fact]
        public void EmailService_WithKeyVaultFailure_FallsBackToEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SMTP_USERNAME", "fallback-test@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "fallback-testpassword");
            
            _mockKeyVaultService.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ThrowsAsync(new Exception("Key Vault unavailable"));

            // Act
            var emailService = new EmailService(_mockLogger.Object, _mockKeyVaultService.Object);

            // Assert
            Assert.NotNull(emailService);
            _mockLogger.Verify(x => x.LogError(
                It.Is<string>(msg => msg.Contains("Failed to retrieve SMTP configuration from Key Vault")), 
                It.IsAny<Exception>()), 
                Times.Once);
            
            // Cleanup
            Environment.SetEnvironmentVariable("SMTP_USERNAME", null);
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", null);
        }
    }
}