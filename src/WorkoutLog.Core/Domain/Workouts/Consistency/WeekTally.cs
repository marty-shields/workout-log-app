namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Totals of the workouts started in part of a week.</summary>
/// <param name="Workouts">Workouts started.</param>
/// <param name="Time">Sum of finish minus start.</param>
/// <param name="Sets">Strength sets.</param>
public sealed record WeekTally(int Workouts, TimeSpan Time, int Sets)
{
    /// <summary>No workouts.</summary>
    public static WeekTally None { get; } = new(0, TimeSpan.Zero, 0);
}
