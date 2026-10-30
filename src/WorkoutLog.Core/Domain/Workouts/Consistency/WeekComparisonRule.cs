namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Last week's window ends at the same local day and time as now, whatever the offset was then.</summary>
internal sealed class WeekComparisonRule : IWeekComparisonRule
{
    /// <inheritdoc />
    public WeekTallies Compare(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var monday = LocalWeek.Monday(now, zone);
        var sameTimeLastWeek = LocalWeek.Last(TimeZoneInfo.ConvertTime(now, zone).DateTime.AddDays(-7), zone);
        return new WeekTallies(
            Tally(workouts, LocalWeek.Start(monday, zone), now),
            Tally(workouts, LocalWeek.Start(monday.AddDays(-7), zone), sameTimeLastWeek));
    }

    private static WeekTally Tally(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset from, DateTimeOffset to)
    {
        var inWindow = workouts.Where(w => w.StartedAt >= from && w.StartedAt <= to).ToList();
        return new WeekTally(
            inWindow.Count,
            inWindow.Aggregate(TimeSpan.Zero, (sum, w) => sum + (w.FinishedAt - w.StartedAt)),
            inWindow.Sum(w => w.Sets));
    }
}
