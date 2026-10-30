using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the Consistency card.</summary>
/// <param name="Comparison">This week so far against last week up to the same day and time. Zeros when loading failed.</param>
/// <param name="WeekCounts">Workouts per week, oldest first; the last is this week. 12 zeros when loading failed.</param>
/// <param name="Streak">Consecutive weeks with a workout, ending last week, plus this week once it has one. 0 when loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record ConsistencyResult(WeekComparison Comparison, IReadOnlyList<int> WeekCounts, int Streak, bool Failed)
{
    /// <summary>The result of a failed read.</summary>
    public static ConsistencyResult FailedRead { get; } =
        new(new WeekComparison(WeekTally.None, WeekTally.None), new int[LocalWeek.Shown], 0, Failed: true);
}
