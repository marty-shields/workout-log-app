namespace WorkoutLog.UI.Navigation;

/// <summary>Where a page returns to, from the <c>from</c> query value of the link that opened it.</summary>
public static class Origin
{
    /// <summary>The <c>from</c> value of a link opened from the Dashboard.</summary>
    public const string Dashboard = "dashboard";

    /// <summary>The Dashboard's route (empty) when <paramref name="from"/> is <see cref="Dashboard"/>, otherwise <paramref name="listRoute"/>.</summary>
    /// <param name="from">The <c>from</c> query value, if any.</param>
    /// <param name="listRoute">Route of the page's list, e.g. <c>workouts</c>.</param>
    public static string Route(string? from, string listRoute) => from == Dashboard ? string.Empty : listRoute;
}
