namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>A workout is in the week of its local start date.</summary>
internal sealed class StreakRule
{
    /// <summary>
    /// Consecutive weeks in <paramref name="zone"/> with a workout, ending the week before the one <paramref name="now"/> is in, plus
    /// that week once it has one. <paramref name="starts"/> are workout start times, in any order.
    /// </summary>
    public int Streak(IEnumerable<DateTimeOffset> starts, DateTimeOffset now, TimeZoneInfo zone)
    {
        var weeks = starts.Select(start => LocalWeek.Monday(start, zone)).ToHashSet();
        var monday = LocalWeek.Monday(now, zone);
        var streak = weeks.Contains(monday) ? 1 : 0;
        for (var week = monday.AddDays(-7); weeks.Contains(week); week = week.AddDays(-7))
        {
            streak++;
        }

        return streak;
    }
}
