using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.UI;

/// <summary>Text formats for dates, times and weights.</summary>
internal static class DisplayFormat
{
    /// <summary>Day and date, e.g. Tue 20 Oct 2026.</summary>
    public static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Day, date and time, e.g. Tue 20 Oct 2026 · 18:05.</summary>
    public static string DateAndTime(DateTime value) => value.ToString("ddd d MMM yyyy · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Elapsed time as h:mm:ss.</summary>
    public static string Duration(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";

    /// <summary>
    /// Weight rounded by <see cref="Decimals.Round2"/>, no trailing zeros and no space before the unit, e.g. 22.5kg; — when there is none.
    /// </summary>
    public static string Weight(decimal? kg) => kg is { } value ? $"{Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture)}kg" : "—";
}
