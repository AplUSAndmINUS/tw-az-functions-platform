using System.Globalization;

namespace Utils.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Formats a DateTime to a short date string in the format "MM/dd/yyyy"
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Formatted date string</returns>
    public static string ToShortDateString(this DateTime dateTime)
    {
        return dateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTime to a long date string in the format "MMMM dd, yyyy"
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Formatted date string</returns>
    public static string ToLongDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTime to ISO 8601 string format
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToIsoString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTime to a friendly relative time string (e.g., "2 hours ago")
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Relative time string</returns>
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var timeSpan = now - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays >= 14 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays >= 60 ? "s" : "")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays >= 730 ? "s" : "")} ago";
    }

    /// <summary>
    /// Formats a DateTime to a blog-friendly format (e.g., "January 15, 2024")
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Blog-friendly formatted string</returns>
    public static string ToBlogDateString(this DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Checks if the DateTime is today
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is today</returns>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    /// <summary>
    /// Checks if the DateTime is yesterday
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is yesterday</returns>
    public static bool IsYesterday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today.AddDays(-1);
    }

    /// <summary>
    /// Checks if the DateTime is within the current week
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is within the current week</returns>
    public static bool IsThisWeek(this DateTime dateTime)
    {
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return dateTime.Date >= startOfWeek && dateTime.Date < endOfWeek;
    }

    /// <summary>
    /// Gets the start of the day (midnight) for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the start of the day</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999) for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the end of the day</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the week (Sunday) for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the start of the week</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(-(int)dateTime.DayOfWeek);
    }

    /// <summary>
    /// Gets the end of the week (Saturday) for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the end of the week</returns>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the month for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the start of the month</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month for the given DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime representing the end of the month</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);
    }
}

/// <summary>
/// Extension methods for DateTimeOffset
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Formats a DateTimeOffset to a short date string in the format "MM/dd/yyyy"
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>Formatted date string</returns>
    public static string ToShortDateString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTimeOffset to a long date string in the format "MMMM dd, yyyy"
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>Formatted date string</returns>
    public static string ToLongDateFormat(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTimeOffset to ISO 8601 string format
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToIsoString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a DateTimeOffset to a friendly relative time string (e.g., "2 hours ago")
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>Relative time string</returns>
    public static string ToRelativeTimeString(this DateTimeOffset dateTimeOffset)
    {
        var now = DateTimeOffset.UtcNow;
        var timeSpan = now - dateTimeOffset;

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays >= 14 ? "s" : "")} ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays >= 60 ? "s" : "")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays >= 730 ? "s" : "")} ago";
    }

    /// <summary>
    /// Formats a DateTimeOffset to a blog-friendly format (e.g., "January 15, 2024")
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>Blog-friendly formatted string</returns>
    public static string ToBlogDateString(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);
    }
}