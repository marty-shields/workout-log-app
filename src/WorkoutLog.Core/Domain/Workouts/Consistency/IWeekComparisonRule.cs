namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Compares this week so far with last week up to the same day and time.</summary>
internal interface IWeekComparisonRule
{
    /// <summary>
    /// Totals of the <paramref name="workouts"/> started from Monday 00:00 to <paramref name="now"/>, and from last Monday 00:00 to
    /// the same day and time last week, both inclusive, in <paramref name="zone"/>.
    /// </summary>
    WeekTallies Compare(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone);
}
