namespace WorkoutLog.Core.Services;

/// <summary>What the workouts in a span of days add up to.</summary>
/// <param name="Workouts">Number of workouts that started in the span.</param>
/// <param name="Sets">Strength sets of those workouts; cardio entries are not counted.</param>
/// <param name="TimeTrained">Sum of start to finish; a workout with no finish, or one before its start, adds zero.</param>
/// <param name="AveragePerWeek">Workouts per 7 days of the span, which counts as at least 7 days; 0 when there are no workouts.</param>
public sealed record WorkoutTotals(int Workouts, int Sets, TimeSpan TimeTrained, double AveragePerWeek);
