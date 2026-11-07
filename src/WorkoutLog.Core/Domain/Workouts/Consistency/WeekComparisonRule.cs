namespace WorkoutLog.Core.Domain.Workouts.Consistency;

internal sealed class WeekComparisonRule
{
    /// <summary>
    /// Totals of the <paramref name="workouts"/> started from this week's Monday 00:00 to <paramref name="now"/> inclusive, and in
    /// the week before it up to the end of the same weekday, in <paramref name="zone"/>.
    /// </summary>
    public WeekTallies Compare(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var monday = LocalWeek.Monday(now, zone);
        var thisWeekStart = LocalWeek.Start(monday, zone);
        return new WeekTallies(
            Tally(workouts.Where(w => w.StartedAt >= thisWeekStart && w.StartedAt <= now)),
            Tally(LastWeekToDate(workouts, monday, LocalWeek.Local(now, zone), zone)));
    }

    /// <summary>Compared on the local wall clock, not by instant, so a clock change between the weeks doesn't move the cut.</summary>
    private static IEnumerable<WorkoutTally> LastWeekToDate(IReadOnlyList<WorkoutTally> workouts, DateOnly monday, DateTime localNow, TimeZoneInfo zone)
    {
        var from = monday.AddDays(-7).ToDateTime(TimeOnly.MinValue);
        var sameDayLastWeek = DateOnly.FromDateTime(localNow).AddDays(-7);
        var endOfSameDay = sameDayLastWeek.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return workouts.Where(w =>
        {
            var local = LocalWeek.Local(w.StartedAt, zone);
            return local >= from && local < endOfSameDay;
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
