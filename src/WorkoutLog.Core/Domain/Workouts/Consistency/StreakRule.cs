namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>A week in progress never breaks the streak. The streak is at most the number of weeks given.</summary>
internal sealed class StreakRule : IStreakRule
{
    /// <inheritdoc />
    public int Streak(IReadOnlyList<int> weekCounts)
    {
        var streak = weekCounts[^1] > 0 ? 1 : 0;
        for (var i = weekCounts.Count - 2; i >= 0 && weekCounts[i] > 0; i--)
        {
            streak++;
        }

        return streak;
    }
}
