using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.UI.Navigation;

/// <summary>Where a page returns to, from the <c>from</c> query value of the link that opened it.</summary>
public static class Origin
{
    /// <summary>The <c>from</c> value of a link opened from This week, once the Dashboard.</summary>
    public const string Dashboard = "dashboard";

    private const string TrendsPrefix = "trends:";

    /// <summary>The <c>from</c> value of a link opened from Trends showing <paramref name="period"/>, e.g. <c>trends:month:2026-11-01</c>.</summary>
    public static string Trends(Period period) => $"{TrendsPrefix}{TrendsRoute.KindName(period.Kind)}:{TrendsRoute.Format(period.Start)}";

    /// <summary>
    /// This week's route (empty) when <paramref name="from"/> is <see cref="Dashboard"/>, that Trends period's route when it is <see cref="Trends"/>,
    /// otherwise <paramref name="listRoute"/>. A Trends value with only a date, <c>trends:2026-11-09</c>, is the week holding it.
    /// </summary>
    /// <param name="from">The <c>from</c> query value, if any.</param>
    /// <param name="listRoute">Route of the page's list, e.g. <c>workouts</c>.</param>
    public static string Route(string? from, string listRoute)
    {
        if (from == Dashboard)
        {
            return string.Empty;
        }

        return from is not null && from.StartsWith(TrendsPrefix, StringComparison.Ordinal) && TrendsPeriod(from[TrendsPrefix.Length..]) is { } period
            ? TrendsRoute.To(period)
            : listRoute;
    }

    private static Period? TrendsPeriod(string text)
    {
        var parts = text.Split(':');
        var kind = parts.Length == 1 ? PeriodKind.Week : TrendsRoute.ParseKind(parts[0]);
        return parts.Length <= 2 && kind is { } known && TrendsRoute.ParseDate(parts[^1]) is { } day ? Period.Containing(known, day) : null;
    }
}
