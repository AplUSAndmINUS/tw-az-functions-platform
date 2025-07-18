using Utils.Extensions;
using Xunit;

namespace Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToShortDateString_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = dateTime.ToShortDateString();

        // Assert
        Assert.Equal("01/15/2024", result);
    }

    [Fact]
    public void ToLongDateFormat_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = dateTime.ToLongDateFormat();

        // Assert
        Assert.Equal("January 15, 2024", result);
    }

    [Fact]
    public void ToBlogDateString_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = dateTime.ToBlogDateString();

        // Assert
        Assert.Equal("January 15, 2024", result);
    }

    [Fact]
    public void IsToday_WithTodaysDate_ReturnsTrue()
    {
        // Arrange
        var today = DateTime.Today;

        // Act
        var result = today.IsToday();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsToday_WithYesterdaysDate_ReturnsFalse()
    {
        // Arrange
        var yesterday = DateTime.Today.AddDays(-1);

        // Act
        var result = yesterday.IsToday();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsYesterday_WithYesterdaysDate_ReturnsTrue()
    {
        // Arrange
        var yesterday = DateTime.Today.AddDays(-1);

        // Act
        var result = yesterday.IsYesterday();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StartOfDay_ReturnsCorrectTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var result = dateTime.StartOfDay();

        // Assert
        Assert.Equal(new DateTime(2024, 1, 15, 0, 0, 0), result);
    }

    [Fact]
    public void EndOfDay_ReturnsCorrectTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 45);

        // Act
        var result = dateTime.EndOfDay();

        // Assert
        Assert.Equal(new DateTime(2024, 1, 15, 23, 59, 59, 999), result);
    }

    [Fact]
    public void StartOfMonth_ReturnsCorrectDate()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = dateTime.StartOfMonth();

        // Assert
        Assert.Equal(new DateTime(2024, 1, 1), result);
    }

    [Fact]
    public void EndOfMonth_ReturnsCorrectDate()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15);

        // Act
        var result = dateTime.EndOfMonth();

        // Assert
        Assert.Equal(new DateTime(2024, 1, 31, 23, 59, 59, 999), result);
    }
}

public class DateTimeOffsetExtensionsTests
{
    [Fact]
    public void ToShortDateString_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = dateTimeOffset.ToShortDateString();

        // Assert
        Assert.Equal("01/15/2024", result);
    }

    [Fact]
    public void ToLongDateFormat_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = dateTimeOffset.ToLongDateFormat();

        // Assert
        Assert.Equal("January 15, 2024", result);
    }

    [Fact]
    public void ToBlogDateString_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = dateTimeOffset.ToBlogDateString();

        // Assert
        Assert.Equal("January 15, 2024", result);
    }
}