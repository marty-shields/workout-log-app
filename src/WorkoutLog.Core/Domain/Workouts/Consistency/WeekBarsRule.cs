namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>A workout is in the week of its local start date.</summary>
internal sealed class WeekBarsRule : IWeekBarsRule
{
    /// <inheritdoc />
    public IReadOnlyList<int> Count(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var thisMonday = LocalWeek.Monday(now, zone);
        var counts = new int[LocalWeek.Shown];
        foreach (var workout in workouts)
        {
            var weeksAgo = (thisMonday.DayNumber - LocalWeek.Monday(workout.StartedAt, zone).DayNumber) / 7;
            if (weeksAgo is >= 0 and < LocalWeek.Shown)
            {
                counts[LocalWeek.Shown - 1 - weeksAgo]++;
            }
        }

        return counts;
    }
}
