using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Lifts (<see cref="ExerciseCategory.Lift"/>). Best is the top set.</summary>
internal sealed class LiftBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Lift;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        ExerciseBest.TopSet(logged.OfType<StrengthExercise>().SelectMany(e => e.Sets));
}
