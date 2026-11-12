using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Services;

/// <summary>What the Dashboard's Consistency card shows.</summary>
/// <param name="Comparison">This week so far against last week up to the end of the same weekday.</param>
/// <param name="WeekCounts">Workouts per week, oldest first; the last is this week.</param>
/// <param name="Streak">Consecutive weeks with a workout, ending last week, plus this week once it has one.</param>
public sealed record WeeklyConsistency(WeekTallies Comparison, IReadOnlyList<int> WeekCounts, int Streak)
{
    /// <summary>No workout in any week shown.</summary>
    public static WeeklyConsistency None { get; } = new(new WeekTallies(WeekTally.None, WeekTally.None), new int[LocalWeek.Shown], 0);
}
