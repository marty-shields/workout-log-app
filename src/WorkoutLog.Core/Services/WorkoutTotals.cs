namespace WorkoutLog.Core.Services;

/// <summary>What the workouts in a span of days add up to.</summary>
/// <param name="Workouts">Workouts that started in the span.</param>
/// <param name="Sets">Strength sets in those workouts.</param>
/// <param name="TimeTrained">Sum of start to finish; a workout with no finish, or one before its start, adds nothing.</param>
/// <param name="AveragePerWeek">Workouts per 7 days of the span; a span under 7 days counts as 7. 0 when there are no workouts.</param>
public sealed record WorkoutTotals(int Workouts, int Sets, TimeSpan TimeTrained, double AveragePerWeek);
