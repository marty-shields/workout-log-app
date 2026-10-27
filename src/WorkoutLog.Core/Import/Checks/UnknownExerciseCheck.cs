using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Import.Checks;

/// <summary>Every exercise id is in the exercise list.</summary>
internal sealed class UnknownExerciseCheck(IExerciseCatalog catalog) : IImportCheck
{
    private readonly HashSet<int> ids = catalog.All.Select(e => e.Id).ToHashSet();

    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Exercises!.Any(e => !ids.Contains(e.Exercise.Id)) ? [ImportSkipReason.UnknownExercise] : [];
}
