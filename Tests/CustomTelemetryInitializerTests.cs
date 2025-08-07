using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using Utils;
using Xunit;

namespace Tests;

public class CustomTelemetryInitializerTests
{
    [Fact]
    public void Initialize_ShouldSetApplicationName()
    {
        // Arrange
        var initializer = new CustomTelemetryInitializer();
        var telemetry = new TraceTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.Equal("tw-az-functions-platform", telemetry.Context.Cloud.RoleName);
    }

    [Fact]
    public void Initialize_ShouldAddGlobalProperties()
    {
        // Arrange
        var initializer = new CustomTelemetryInitializer();
        var telemetry = new TraceTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.True(telemetry.Context.GlobalProperties.ContainsKey("Platform"));
        Assert.Equal("Azure Functions", telemetry.Context.GlobalProperties["Platform"]);
        Assert.True(telemetry.Context.GlobalProperties.ContainsKey("Framework"));
        Assert.Equal(".NET 8.0", telemetry.Context.GlobalProperties["Framework"]);
        Assert.True(telemetry.Context.GlobalProperties.ContainsKey("Environment"));
    }

    [Fact]
    public void Initialize_ShouldAddApplicationVersion()
    {
        // Arrange
        var initializer = new CustomTelemetryInitializer();
        var telemetry = new TraceTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.True(telemetry.Context.GlobalProperties.ContainsKey("ApplicationVersion"));
        // Default version should be 1.0.0 when APPLICATION_VERSION env var is not set
        Assert.Equal("1.0.0", telemetry.Context.GlobalProperties["ApplicationVersion"]);
    }

    [Fact]
    public void Initialize_WithEnvironmentVariable_ShouldUseCustomVersion()
    {
        // Arrange
        var initializer = new CustomTelemetryInitializer();
        var telemetry = new TraceTelemetry();
        var testVersion = "2.0.0-test";
        
        // Set environment variable
        System.Environment.SetEnvironmentVariable("APPLICATION_VERSION", testVersion);

        try
        {
            // Act
            initializer.Initialize(telemetry);

            // Assert
            Assert.Equal(testVersion, telemetry.Context.GlobalProperties["ApplicationVersion"]);
        }
        finally
        {
            // Clean up
            System.Environment.SetEnvironmentVariable("APPLICATION_VERSION", null);
        }
    }

    [Fact]
    public void Initialize_ShouldNotOverrideExistingRoleName()
    {
        // Arrange
        var initializer = new CustomTelemetryInitializer();
        var telemetry = new TraceTelemetry();
        var existingRoleName = "existing-service";
        telemetry.Context.Cloud.RoleName = existingRoleName;

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.Equal(existingRoleName, telemetry.Context.Cloud.RoleName);
    }
}