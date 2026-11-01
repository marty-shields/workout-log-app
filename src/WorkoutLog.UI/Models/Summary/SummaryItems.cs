using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Summary;

/// <summary>Builds what the Workout Summary shows from a saved workout's summary.</summary>
public static class SummaryItems
{
    /// <summary>
    /// The totals, a record row for each record and a change row for each exercise, in the workout's order, with each best as on the Exercises list.
    /// An exercise whose best the list can't show is left out. A workout with no earlier workout has no changes.
    /// </summary>
    public static WorkoutSummary Summary(SavedWorkoutSummary saved, IExerciseCatalog catalog)
    {
        var definitions = catalog.All.ToDictionary(e => e.Id);
        var shown = saved.Exercises
            .Select(e => (Exercise: e, Definition: definitions[e.ExerciseId]))
            .Select(e => (e.Exercise, e.Definition, Best: ExerciseListItems.Best(e.Definition, e.Exercise.Best)))
            .Where(e => e.Best is not null)
            .ToList();
        var records = shown
            .Where(e => e.Exercise.IsRecord)
            .Select(e => new SummaryRecord(e.Definition.Id, e.Definition.Name, e.Best!))
            .ToList();
        var changes = saved.HasEarlierWorkout
            ? shown.Select(e => new SummaryChange(e.Definition.Id, e.Definition.Name, e.Best!, Since(e.Definition, e.Exercise.Change))).ToList()
            : [];
        return new WorkoutSummary(saved.Duration, saved.ExerciseCount, saved.SetCount, records, changes);
    }

    private static SinceLastTime Since(ExerciseDefinition exercise, BestChange change) => change.Kind switch
    {
        BestChangeKind.Up => SinceLastTime.Up(Difference(exercise, change.Difference!)),
        BestChangeKind.Down => SinceLastTime.Down(Difference(exercise, change.Difference!)),
        BestChangeKind.Equal => SinceLastTime.Equal,
        _ => SinceLastTime.FirstTime,
    };

    private static string Difference(ExerciseDefinition exercise, ExerciseBest difference) => difference switch
    {
        { WeightKg: { } kg } => DisplayFormat.Weight(kg),
        { Reps: { } reps } => reps == 1 ? "1 rep" : $"{reps} reps",
        { Duration: { } duration } => DisplayFormat.Clock(duration),
        { Distance: { } distance } => ExerciseFields.Distance(exercise, distance),
        _ => throw new InvalidOperationException("A change has a difference."),
    };
}
