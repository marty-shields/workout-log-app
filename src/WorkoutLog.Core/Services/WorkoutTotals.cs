namespace WorkoutLog.Core.Services;

/// <summary>What was trained in a span of days.</summary>
/// <param name="TimeTrained">Finish less start, summed; a workout with no finish time or a finish before its start adds zero.</param>
/// <param name="AveragePerWeek">Workouts per 7 days, with a span under 7 days counted as 7; 0 with no workouts.</param>
public sealed record WorkoutTotals(int Workouts, int Sets, TimeSpan TimeTrained, double AveragePerWeek);
