using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.UI;

/// <summary>Text formats for dates, times and weights.</summary>
internal static class DisplayFormat
{
    /// <summary>Day and date, e.g. Tue 20 Oct 2026.</summary>
    public static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Day and date, with the year only when it isn't <paramref name="today"/>'s, e.g. Tue 20 Oct or Wed 15 Oct 2025.</summary>
    public static string ShortDate(DateTime value, DateTime today) =>
        value.ToString(value.Year == today.Year ? "ddd d MMM" : "ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Date, with the year only when it isn't <paramref name="today"/>'s, e.g. 20 Oct or 15 Oct 2025.</summary>
    public static string DayMonth(DateTime value, DateTime today) =>
        value.ToString(value.Year == today.Year ? "d MMM" : "d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Time as m:ss, or h:mm:ss from an hour, e.g. 1:45 or 1:02:30.</summary>
    public static string Clock(TimeSpan value) =>
        value.TotalHours >= 1 ? Duration(value) : $"{(int)value.TotalMinutes}:{value.Seconds:00}";

    /// <summary>Day, date and time, e.g. Tue 20 Oct 2026 · 18:05.</summary>
    public static string DateAndTime(DateTime value) => value.ToString("ddd d MMM yyyy · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Elapsed time as h:mm:ss.</summary>
    public static string Duration(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";

    /// <summary>
    /// Weight rounded by <see cref="Decimals.Round2"/>, no trailing zeros and no space before the unit, e.g. 22.5kg; — when there is none.
    /// </summary>
    public static string Weight(decimal? kg) => kg is { } value ? $"{Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture)}kg" : "—";
}
