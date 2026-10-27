namespace WorkoutLog.Core.Services;

/// <summary>Outcome of an import. The counts are 0 when it failed.</summary>
/// <param name="Status">How the import ended.</param>
/// <param name="Added">Workouts added.</param>
/// <param name="Replaced">Phone workouts replaced by the file's.</param>
/// <param name="Skipped">Workouts that can't be imported.</param>
public sealed record ImportResult(ImportStatus Status, int Added, int Replaced, int Skipped);
