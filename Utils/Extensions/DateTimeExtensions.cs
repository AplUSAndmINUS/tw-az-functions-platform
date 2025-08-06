using System.Globalization;

namespace Utils.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to ISO 8601 string format
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToIso8601String(this DateTime dateTime)
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
    /// Converts DateTime to Unix timestamp (seconds since epoch)
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp as long</returns>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp in milliseconds
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp in milliseconds as long</returns>
    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Returns the start of the day (00:00:00)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of day</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Returns the end of the day (23:59:59.999)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of day</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Returns the start of the week (Monday)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of week</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Returns the end of the week (Sunday)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of week</returns>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(6).EndOfDay();
    }

    /// <summary>
    /// Returns the start of the month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of month</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Returns the end of the month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of month</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Returns the start of the year
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of year</returns>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Returns the end of the year
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of year</returns>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return dateTime.StartOfYear().AddYears(1).AddMilliseconds(-1);
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
    /// Checks if the DateTime is tomorrow
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is tomorrow</returns>
    public static bool IsTomorrow(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today.AddDays(1);
    }

    /// <summary>
    /// Checks if the DateTime is in the past
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is in the past</returns>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.Now;
    }

    /// <summary>
    /// Checks if the DateTime is in the future
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>True if the date is in the future</returns>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.Now;
    }

    /// <summary>
    /// Returns a human-readable relative time string
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>Relative time string (e.g., "2 hours ago", "in 3 days")</returns>
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalDays > 365)
        {
            var years = (int)(timeSpan.TotalDays / 365);
            return $"{years} year{(years == 1 ? "" : "s")} ago";
        }
        
        if (timeSpan.TotalDays > 30)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            return $"{months} month{(months == 1 ? "" : "s")} ago";
        }
        
        if (timeSpan.TotalDays >= 1)
        {
            var days = (int)timeSpan.TotalDays;
            return $"{days} day{(days == 1 ? "" : "s")} ago";
        }
        
        if (timeSpan.TotalHours >= 1)
        {
            var hours = (int)timeSpan.TotalHours;
            return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
        }
        
        if (timeSpan.TotalMinutes >= 1)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
        }
        
        return "just now";
    }

    /// <summary>
    /// Converts DateTime to a safe filename string
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Safe filename string</returns>
    public static string ToSafeFilename(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Rounds DateTime to the nearest specified interval
    /// </summary>
    /// <param name="dateTime">The DateTime to round</param>
    /// <param name="interval">The interval to round to</param>
    /// <returns>Rounded DateTime</returns>
    public static DateTime RoundTo(this DateTime dateTime, TimeSpan interval)
    {
        var ticks = (long)(dateTime.Ticks / (double)interval.Ticks + 0.5) * interval.Ticks;
        return new DateTime(ticks);
    }

    /// <summary>
    /// Truncates DateTime to the specified interval
    /// </summary>
    /// <param name="dateTime">The DateTime to truncate</param>
    /// <param name="interval">The interval to truncate to</param>
    /// <returns>Truncated DateTime</returns>
    public static DateTime TruncateTo(this DateTime dateTime, TimeSpan interval)
    {
        var ticks = (long)(dateTime.Ticks / (double)interval.Ticks) * interval.Ticks;
        return new DateTime(ticks);
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