namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>What one set adds to volume.</summary>
/// <param name="StartedAt">The workout's start, UTC.</param>
/// <param name="Kg">Weight times reps; 0 when the set adds nothing.</param>
internal sealed record SetVolume(DateTimeOffset StartedAt, decimal Kg);
