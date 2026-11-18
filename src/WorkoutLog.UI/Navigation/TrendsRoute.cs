using System.Globalization;

namespace WorkoutLog.UI.Navigation;

/// <summary>The Trends route and its query, so a link opens a given week.</summary>
public static class TrendsRoute
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>The route of the week holding <paramref name="day"/>, e.g. <c>trends?period=week&amp;date=2026-11-09</c>.</summary>
    public static string Week(DateOnly day) => $"trends?period=week&date={Format(day)}";

    /// <summary>Writes <paramref name="day"/> as the <c>date</c> query value reads it: 2026-11-09.</summary>
    public static string Format(DateOnly day) => day.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>Reads a <c>date</c> query value written as 2026-11-09; null when it is missing or not that.</summary>
    public static DateOnly? ParseDate(string? text) =>
        DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : null;
}
