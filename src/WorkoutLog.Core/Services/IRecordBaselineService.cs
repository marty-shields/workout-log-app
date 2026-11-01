namespace WorkoutLog.Core.Services;

/// <summary>Loads what New Workout needs to show the Record badge for an exercise.</summary>
public interface IRecordBaselineService
{
    /// <summary>
    /// Reads the exercise's saved sessions. A failed read, or an exercise not in the exercise list, returns
    /// <see cref="RecordBaselineResult.Failed"/> and no baseline.
    /// </summary>
    Task<RecordBaselineResult> LoadAsync(int exerciseId);
}
