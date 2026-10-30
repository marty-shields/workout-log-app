namespace WorkoutLog.Core.Services;

/// <summary>Reads the exercises that have a saved workout, with their last session.</summary>
public interface ILoggedExercisesService
{
    /// <summary>
    /// Returns each exercise with a saved workout, with the start and best of its last session:
    /// latest start first, exercises from the same workout A–Z. Ids not in the exercise list are left out.
    /// A failed read returns <see cref="LoggedExercisesResult.Failed"/> and no exercises.
    /// </summary>
    Task<LoggedExercisesResult> GetAsync();
}
