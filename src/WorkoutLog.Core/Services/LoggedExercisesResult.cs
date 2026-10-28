namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading the logged exercises.</summary>
/// <param name="Exercises">Logged exercises, latest first. Empty when loading failed.</param>
/// <param name="Failed">True when the exercises couldn't be read.</param>
public sealed record LoggedExercisesResult(IReadOnlyList<LoggedExercise> Exercises, bool Failed);
