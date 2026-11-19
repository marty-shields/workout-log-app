using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Navigation;

/// <summary>Where a page returns to, from the <c>from</c> query value of the link that opened it.</summary>
public static class Origin
{
    /// <summary>The <c>from</c> value of a link opened from This week.</summary>
    public const string ThisWeek = "this-week";

    private const string TrendsPrefix = "trends:";
    private const string WorkoutsPrefix = "workouts:";

    /// <summary>The <c>from</c> value of a link opened from Trends showing <paramref name="period"/>, e.g. <c>trends:month:2026-11-01</c>.</summary>
    public static string Trends(Period period) => $"{TrendsPrefix}{TrendsRoute.KindName(period.Kind)}:{TrendsRoute.Format(period.Start)}";

    /// <summary>
    /// The <c>from</c> value of a link opened from Workouts showing <paramref name="dates"/>, e.g. <c>workouts:2026-11-02:2026-11-08</c>.
    /// <paramref name="from"/> is where Workouts itself was opened from, kept at the end so back from Workouts still reaches it.
    /// </summary>
    public static string Workouts(DateSpan dates, string? from) =>
        $"{WorkoutsPrefix}{TrendsRoute.Format(dates.From)}:{TrendsRoute.Format(dates.To)}{(from is null ? null : $":{from}")}";

    /// <summary><paramref name="route"/> with <paramref name="from"/> as its <c>from</c> query value; just the route when <paramref name="from"/> is null.</summary>
    public static string Link(string route, string? from) => from is null ? route : $"{route}?from={Uri.EscapeDataString(from)}";

    /// <summary>
    /// The route to return to: that Trends period's when <paramref name="from"/> is <see cref="Trends"/>, those Workouts dates' when it is
    /// <see cref="Workouts"/>, otherwise This week's (empty), also for <see cref="ThisWeek"/>, a missing value or one that doesn't parse.
    /// A Trends value with only a date, <c>trends:2026-11-09</c>, is the week holding it.
    /// </summary>
    /// <param name="from">The <c>from</c> query value, if any.</param>
    public static string Route(string? from)
    {
        if (from is null)
        {
            return string.Empty;
        }

        if (from.StartsWith(TrendsPrefix, StringComparison.Ordinal))
        {
            return TrendsPeriod(from[TrendsPrefix.Length..]) is { } period ? TrendsRoute.To(period) : string.Empty;
        }

        return from.StartsWith(WorkoutsPrefix, StringComparison.Ordinal) && WorkoutsDates(from[WorkoutsPrefix.Length..]) is var (dates, outer)
            ? WorkoutsRoute.To(dates, outer)
            : string.Empty;
    }

    private static Period? TrendsPeriod(string text)
    {
        var parts = text.Split(':');
        var kind = parts.Length == 1 ? PeriodKind.Week : TrendsRoute.ParseKind(parts[0]);
        return parts.Length <= 2 && kind is { } known && TrendsRoute.ParseDate(parts[^1]) is { } day ? Period.Containing(known, day) : null;
    }

    private static (DateSpan Dates, string? From)? WorkoutsDates(string text)
    {
        var parts = text.Split(':', 3);
        return parts.Length >= 2 && WorkoutsRoute.ParseDates(parts[0], parts[1]) is { } dates
            ? (dates, parts.Length == 3 ? parts[2] : null)
            : null;
    }
}
