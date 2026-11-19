using System.Globalization;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.UI.Navigation;

/// <summary>The Trends route and its query, so a link opens a given week, month or year.</summary>
public static class TrendsRoute
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>The route of <paramref name="period"/>, e.g. <c>trends?period=month&amp;date=2026-11-01</c>.</summary>
    public static string To(Period period) => $"trends?period={KindName(period.Kind)}&date={Format(period.Start)}";

    /// <summary>The <c>period</c> query value of a kind: week, month or year.</summary>
    public static string KindName(PeriodKind kind) => kind.ToString().ToLowerInvariant();

    /// <summary>Reads a <c>period</c> query value (week, month or year, in lower case); null when it is missing or not one of those.</summary>
    public static PeriodKind? ParseKind(string? text) => text switch
    {
        "week" => PeriodKind.Week,
        "month" => PeriodKind.Month,
        "year" => PeriodKind.Year,
        _ => null,
    };

    /// <summary>Writes <paramref name="day"/> as the <c>date</c> query value reads it: 2026-11-09.</summary>
    public static string Format(DateOnly day) => day.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>Reads a <c>date</c> query value written as 2026-11-09; null when it is missing or not that.</summary>
    public static DateOnly? ParseDate(string? text) =>
        DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : null;
}
