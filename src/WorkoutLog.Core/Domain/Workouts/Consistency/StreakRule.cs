namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>A workout is in the week of its local start date.</summary>
internal sealed class StreakRule : IStreakRule
{
    /// <inheritdoc />
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
