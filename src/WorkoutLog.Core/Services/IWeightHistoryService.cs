namespace WorkoutLog.Core.Services;

/// <summary>Reads an exercise's last and best weights from the saved workouts.</summary>
public interface IWeightHistoryService
{
    /// <summary>
    /// Returns the best weight in the last saved workout with the exercise and in any saved workout. The last workout
    /// has the latest start. Best is the heaviest, or the lowest for an assisted machine.
    /// A failed read returns <see cref="WeightHistoryResult.Failed"/> and no weights.
    /// </summary>
    Task<WeightHistoryResult> GetAsync(int exerciseId);
}
