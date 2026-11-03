namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>This week so far against the whole of last week, with weeks bucketed as <see cref="WeekBarsRule"/> does.</summary>
internal sealed class WeekComparisonRule
{
    /// <summary>
    /// Totals of the <paramref name="workouts"/> started from this week's Monday 00:00 to <paramref name="now"/> inclusive, and in
    /// the whole week before it, in <paramref name="zone"/>.
    /// </summary>
    public WeekTallies Compare(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var monday = LocalWeek.Monday(now, zone);
        var lastMonday = monday.AddDays(-7);
        var thisWeekStart = LocalWeek.Start(monday, zone);
        return new WeekTallies(
            Tally(workouts.Where(w => w.StartedAt >= thisWeekStart && w.StartedAt <= now)),
            Tally(workouts.Where(w => LocalWeek.Monday(w.StartedAt, zone) == lastMonday)));
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
