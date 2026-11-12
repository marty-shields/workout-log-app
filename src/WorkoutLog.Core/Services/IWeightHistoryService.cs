namespace WorkoutLog.Core.Services;

/// <summary>Reads an exercise's last and best weights and its last note from the saved workouts.</summary>
public interface IWeightHistoryService
{
    /// <summary>
    /// Returns the best weight in the last saved workout with the exercise and in any saved workout, and the exercise's
    /// note in that last workout. The last workout has the latest start. Best is the heaviest, or the lowest for an assisted machine.
    /// <paramref name="before"/>, when given, counts only workouts that started before it.
    /// </summary>
    Task<ReadResult<ExerciseHistory>> GetAsync(int exerciseId, DateTimeOffset? before);
}
