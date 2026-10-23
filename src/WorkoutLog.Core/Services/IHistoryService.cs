namespace WorkoutLog.Core.Services;

/// <summary>Reads the saved workouts for History.</summary>
public interface IHistoryService
{
    /// <summary>
    /// Returns every saved workout, by start time newest first, then by id newest first (FR-20).
    /// A failed read returns <see cref="HistoryResult.Failed"/> and no workouts.
    /// </summary>
    Task<HistoryResult> GetAllAsync();
}
