using WorkoutLog.Core.Domain.Workouts.Validation;

namespace WorkoutLog.Core.Import.Checks;

/// <summary>
/// The workout passes Save's checks (FR-09) in <see cref="IWorkoutValidator"/>.
/// Unknown exercises are left to <see cref="UnknownExerciseCheck"/>, and repeated exercises to <see cref="RepeatedExerciseCheck"/>.
/// </summary>
internal sealed class SaveRulesCheck(IWorkoutValidator validator) : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context)
    {
        var errors = validator.Validate(workout.Exercises!.Select(e => e.ToDomain()).ToList());
        return errors.Errors.Any(e => e != WorkoutError.RepeatedExercise) || errors.Exercises.Any(e => e.Error != ExerciseError.UnknownExercise && !e.IsValid)
            ? [ImportSkipReason.FailsSaveRules]
            : [];
    }
}
