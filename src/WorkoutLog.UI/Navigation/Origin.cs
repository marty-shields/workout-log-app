namespace WorkoutLog.UI.Navigation;

/// <summary>Where a page returns to, from the <c>from</c> query value of the link that opened it.</summary>
public static class Origin
{
    /// <summary>The <c>from</c> value of a link opened from This week, once the Dashboard.</summary>
    public const string Dashboard = "dashboard";

    private const string TrendsPrefix = "trends:";

    /// <summary>The <c>from</c> value of a link opened from the Trends week holding <paramref name="day"/>, e.g. <c>trends:2026-11-09</c>.</summary>
    public static string Trends(DateOnly day) => TrendsPrefix + TrendsRoute.Format(day);

    /// <summary>
    /// This week's route (empty) when <paramref name="from"/> is <see cref="Dashboard"/>, that Trends week's route when it is <see cref="Trends"/>,
    /// otherwise <paramref name="listRoute"/>.
    /// </summary>
    /// <param name="from">The <c>from</c> query value, if any.</param>
    /// <param name="listRoute">Route of the page's list, e.g. <c>workouts</c>.</param>
    public static string Route(string? from, string listRoute)
    {
        if (from == Dashboard)
        {
            return string.Empty;
        }

        return from is not null && from.StartsWith(TrendsPrefix, StringComparison.Ordinal) && TrendsRoute.ParseDate(from[TrendsPrefix.Length..]) is { } day
            ? TrendsRoute.Week(day)
            : listRoute;
    }
}
