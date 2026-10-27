namespace WorkoutLog.Core.Services;

/// <summary>Outcome of counting the workouts to export.</summary>
/// <param name="Count">Saved workouts. 0 when counting failed.</param>
/// <param name="Failed">True when the workouts couldn't be read.</param>
public sealed record ExportCount(int Count, bool Failed);
