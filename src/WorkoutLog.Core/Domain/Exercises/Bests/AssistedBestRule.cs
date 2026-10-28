using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Assisted machines (<see cref="ExerciseCategory.Assisted"/>). Best is the least assist, blank as 0 kg; tie → most reps.</summary>
internal sealed class AssistedBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Assisted;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        ExerciseBest.LeastAssist(logged.OfType<StrengthExercise>().SelectMany(e => e.Sets));
}
