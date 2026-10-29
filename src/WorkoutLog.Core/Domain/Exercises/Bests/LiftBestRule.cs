using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Lifts (<see cref="ExerciseCategory.Lift"/>). Best is the top set (<see cref="ITopSetRule"/>).</summary>
internal sealed class LiftBestRule(ITopSetRule topSetRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Lift;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        topSetRule.TopSet([.. logged.OfType<StrengthExercise>().SelectMany(e => e.Sets)]);
}
