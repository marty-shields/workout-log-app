using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Assisted machines (<see cref="ExerciseCategory.Assisted"/>). Best is the least assist (<see cref="ILeastAssistRule"/>).</summary>
internal sealed class AssistedBestRule(ILeastAssistRule leastAssistRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Assisted;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged) =>
        leastAssistRule.LeastAssist((logged as StrengthExercise)?.Sets ?? []);
}
