using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the last workout.</summary>
/// <param name="Workout">The workout with the latest start, or null if there are none or loading failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record LastWorkoutResult(WorkoutOutline? Workout, bool Failed);
