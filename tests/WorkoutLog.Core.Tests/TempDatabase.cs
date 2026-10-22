namespace WorkoutLog.Core.Tests;

/// <summary>Temp SQLite file paths for tests.</summary>
public static class TempDatabase
{
    /// <summary>Returns a new, unique .db3 path in the temp folder.</summary>
    public static string NewPath() => Path.Combine(Path.GetTempPath(), $"workoutlog-{Guid.NewGuid():N}.db3");
}
