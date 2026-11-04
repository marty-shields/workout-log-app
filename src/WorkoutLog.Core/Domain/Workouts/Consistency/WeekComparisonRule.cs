namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>This week so far against last week up to the same point, with weeks bucketed as <see cref="WeekBarsRule"/> does.</summary>
internal sealed class WeekComparisonRule
{
    /// <summary>
    /// Totals of the <paramref name="workouts"/> started from this week's Monday 00:00 to <paramref name="now"/> inclusive, and in
    /// the week before it up to the same weekday and local time of day inclusive, in <paramref name="zone"/>.
    /// </summary>
    public WeekTallies Compare(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var monday = LocalWeek.Monday(now, zone);
        var thisWeekStart = LocalWeek.Start(monday, zone);
        return new WeekTallies(
            Tally(workouts.Where(w => w.StartedAt >= thisWeekStart && w.StartedAt <= now)),
            Tally(LastWeekToDate(workouts, monday, LocalWeek.Local(now, zone), zone)));
    }

    /// <summary>
    /// Compared by local wall-clock time rather than by instant, so a clock change between the weeks doesn't move the cut by an hour.
    /// The same point last week is always before <paramref name="monday"/>, so the window is last Monday 00:00 up to it.
    /// </summary>
    private static IEnumerable<WorkoutTally> LastWeekToDate(IReadOnlyList<WorkoutTally> workouts, DateOnly monday, DateTime localNow, TimeZoneInfo zone)
    {
        var from = monday.AddDays(-7).ToDateTime(TimeOnly.MinValue);
        var samePoint = localNow.AddDays(-7);
        return workouts.Where(w =>
        {
            var local = LocalWeek.Local(w.StartedAt, zone);
            return local >= from && local <= samePoint;
        });
    }

    private static WeekTally Tally(IEnumerable<WorkoutTally> inWindow)
    {
        var workouts = inWindow.ToList();
        return new WeekTally(
            workouts.Count,
            workouts.Aggregate(TimeSpan.Zero, (sum, w) => sum + (w.FinishedAt - w.StartedAt)),
            workouts.Sum(w => w.Sets));
    }
}
