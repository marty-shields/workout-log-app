namespace WorkoutLog.Core.Services;

/// <summary>Every write of an import was made.</summary>
/// <param name="Added">Workouts added.</param>
/// <param name="Replaced">Phone workouts replaced by the file's.</param>
/// <param name="Skipped">Workouts that can't be imported.</param>
public sealed record Imported(int Added, int Replaced, int Skipped);

/// <summary>The write failed; nothing was written.</summary>
public sealed record ImportFailed;

public union ImportResult(Imported, ImportFailed);
