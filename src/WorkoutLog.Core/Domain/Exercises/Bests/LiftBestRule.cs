using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Lifts: strength with required reps, not assisted, equipment other than Bodyweight, Other or Band. Best is the top set.</summary>
internal sealed class LiftBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) =>
        exercise.Type == ExerciseType.Strength
        && !exercise.IsAssisted
        && exercise.Metrics.Any(m => m is { Metric: Metric.Reps, Required: true })
        && !BodyweightBestRule.IsBodyweightEquipment(exercise.Equipment);

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        ExerciseBest.TopSet(logged.OfType<StrengthExercise>().SelectMany(e => e.Sets));
}
