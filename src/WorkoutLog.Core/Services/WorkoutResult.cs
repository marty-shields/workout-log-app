using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of opening a saved workout.</summary>
/// <param name="Workout">The workout; null unless <paramref name="Status"/> is <see cref="WorkoutLoad.Loaded"/>.</param>
/// <param name="Status">Whether it loaded, doesn't exist or couldn't be read.</param>
public sealed record WorkoutResult(Workout? Workout, WorkoutLoad Status);
