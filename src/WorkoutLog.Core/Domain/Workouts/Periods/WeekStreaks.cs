namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A run of consecutive weeks with a workout.</summary>
/// <param name="Weeks">0 when there is none.</param>
/// <param name="Running">The run reaches this week, or last week while this one has no workout yet.</param>
public sealed record LongestRun(int Weeks, bool Running);

/// <summary>Streaks for This week and Trends.</summary>
/// <param name="Current">Consecutive weeks with a workout, ending last week, plus this week once it has one.</param>
/// <param name="WeekCounts">Workouts per week for the last 12 weeks, oldest first; the last is this week.</param>
/// <param name="Longest">The longest run among weeks that start in the year asked for.</param>
public sealed record WeekStreaks(int Current, IReadOnlyList<int> WeekCounts, LongestRun Longest);
