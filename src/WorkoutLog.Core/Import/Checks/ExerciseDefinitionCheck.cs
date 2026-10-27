using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Import.Checks;

/// <summary>Each known exercise has the same type, distance unit, rate unit and pace unit as in the exercise list. Names aren't checked.</summary>
internal sealed class ExerciseDefinitionCheck(IExerciseCatalog catalog) : IImportCheck
{
    private readonly Dictionary<int, ExerciseDefinition> definitions = catalog.All.ToDictionary(e => e.Id);

    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Exercises!.Any(e => definitions.TryGetValue(e.Exercise.Id, out var definition) && !Matches(e.Exercise, definition))
            ? [ImportSkipReason.ExerciseMismatch]
            : [];

    private static bool Matches(ImportExerciseInfo info, ExerciseDefinition definition) =>
        info.Type == definition.Type
        && info.DistanceUnit == definition.DistanceUnit
        && info.RateUnit == definition.RateUnit
        && info.PaceUnit == definition.PaceUnit;
}
