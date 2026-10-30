namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Works out the weekly streak.</summary>
internal interface IStreakRule
{
    /// <summary>
    /// Consecutive weeks with a workout ending last week, plus this week once it has one.
    /// <paramref name="weekCounts"/> is workouts per week, oldest first; the last is this week.
    /// </summary>
    int Streak(IReadOnlyList<int> weekCounts);
}
