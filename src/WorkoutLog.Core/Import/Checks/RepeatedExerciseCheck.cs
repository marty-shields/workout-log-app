namespace WorkoutLog.Core.Import.Checks;

/// <summary>Each exercise id appears once in the workout.</summary>
internal sealed class RepeatedExerciseCheck : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Exercises!.DistinctBy(e => e.Exercise.Id).Count() < workout.Exercises!.Count ? [ImportSkipReason.RepeatedExercise] : [];
}
