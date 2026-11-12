using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Workouts;

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

    /// <summary>Day, date without the year, and time, e.g. Thu 5 Nov · 18:05.</summary>
    public static string DayAndTime(DateTime value) => value.ToString("ddd d MMM · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Day and date without the year, e.g. Thu 12 Mar.</summary>
    public static string Day(DateOnly value) => value.ToString("ddd d MMM", CultureInfo.InvariantCulture);

    /// <summary>Day and date with a two-digit year, e.g. Thu 12 Mar 25.</summary>
    public static string DayShortYear(DateOnly value) => value.ToString("ddd d MMM yy", CultureInfo.InvariantCulture);

    /// <summary>Month and year, e.g. November 2026.</summary>
    public static string Month(int year, int month) => new DateOnly(year, month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Dates in a span: one day as Thu 12 Mar 2025, one year as 6 Aug – 6 Nov 2026, and across years as 6 Nov 2025 – 6 Nov 2026.
    /// </summary>
    public static string Dates(DateSpan dates)
    {
        var to = dates.To.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
        if (dates.IsOneDay)
        {
            return dates.From.ToString("ddd ", CultureInfo.InvariantCulture) + to;
        }

        var from = dates.From.ToString(dates.From.Year == dates.To.Year ? "d MMM" : "d MMM yyyy", CultureInfo.InvariantCulture);
        return $"{from} – {to}";
    }

    /// <summary>
    /// <see cref="Dates"/> with no-break spaces inside each date, so narrow text wraps at the dash and never mid-date, e.g. 4 Nov 2024 – / 6 Nov 2026.
    /// </summary>
    public static string DatesKeptWhole(DateSpan dates) => Dates(dates).Replace(' ', '\u00A0').Replace("\u00A0–\u00A0", " – ", StringComparison.Ordinal);

    /// <summary>A number of workouts, e.g. 1 workout or 38 workouts.</summary>
    public static string Workouts(int count) => count == 1 ? "1 workout" : $"{count} workouts";

    /// <summary>Calendar days since a date: Today, Yesterday, or e.g. 3 days ago.</summary>
    public static string DaysAgo(int days) => days switch
    {
        0 => "Today",
        1 => "Yesterday",
        _ => $"{days} days ago",
    };

    /// <summary>Total time in whole hours and minutes, e.g. 12 h 30 min, or just minutes under an hour, e.g. 45 min.</summary>
    public static string TimeTrained(TimeSpan value) =>
        value.TotalHours >= 1 ? $"{(int)value.TotalHours} h {value.Minutes} min" : $"{(int)value.TotalMinutes} min";

    /// <summary>Workouts per week to one decimal, e.g. 2.5.</summary>
    public static string PerWeek(double value) => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>Elapsed time as h:mm:ss.</summary>
    public static string Duration(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";

    /// <summary>
    /// Weight rounded by <see cref="Decimals.Round2"/>, no trailing zeros and no space before the unit, e.g. 22.5kg; — when there is none.
    /// </summary>
    public static string Weight(decimal? kg) => kg is { } value ? $"{Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture)}kg" : "—";
}
