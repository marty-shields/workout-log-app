using System.Globalization;

namespace WorkoutLog.UI;

/// <summary>Text formats for dates and times.</summary>
internal static class DisplayFormat
{
    /// <summary>Day and date, e.g. Tue 20 Oct 2026.</summary>
    public static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Day, date and time, e.g. Tue 20 Oct 2026 · 18:05.</summary>
    public static string DateAndTime(DateTime value) => value.ToString("ddd d MMM yyyy · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Elapsed time as h:mm:ss.</summary>
    public static string Duration(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";
}
