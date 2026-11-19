using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Navigation;

/// <summary>The Workouts route and its query, so a link opens the list for given dates.</summary>
public static class WorkoutsRoute
{
    /// <summary>The route of <paramref name="dates"/>, e.g. <c>workouts?first=2026-11-02&amp;last=2026-11-08&amp;from=trends%3Aweek%3A2026-11-02</c>.</summary>
    /// <param name="dates">The days to show, both ends included.</param>
    /// <param name="from">Where Workouts was opened from, as an <see cref="Origin"/> value; left out when null.</param>
    public static string To(DateSpan dates, string? from = null) =>
        $"workouts?first={TrendsRoute.Format(dates.From)}&last={TrendsRoute.Format(dates.To)}{(from is null ? null : $"&from={Uri.EscapeDataString(from)}")}";

    /// <summary>Reads the <c>first</c> and <c>last</c> query values; null when either is missing or not a date, or the first is after the last.</summary>
    public static DateSpan? ParseDates(string? first, string? last) =>
        TrendsRoute.ParseDate(first) is { } from && TrendsRoute.ParseDate(last) is { } to && from <= to ? new DateSpan(from, to) : null;
}
