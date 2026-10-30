using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Lifts (<see cref="ExerciseCategory.Lift"/>). Best is the top set (<see cref="ITopSetRule"/>); heavier beats lighter, tie → more reps.</summary>
internal sealed class LiftBestRule(ITopSetRule topSetRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Lift;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged) =>
        topSetRule.TopSet((logged as StrengthExercise)?.Sets ?? []);

    /// <inheritdoc />
    public int Compare(ExerciseBest x, ExerciseBest y) => (x.WeightKg, x.Reps).CompareTo((y.WeightKg, y.Reps));
}
