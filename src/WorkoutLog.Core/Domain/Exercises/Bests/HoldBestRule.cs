using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Holds (<see cref="ExerciseCategory.Hold"/>). Best is the longest (<see cref="IHoldRule.Longest"/>).</summary>
internal sealed class HoldBestRule(IHoldRule holdRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Hold;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        holdRule.Longest([.. logged.OfType<StrengthExercise>().SelectMany(e => e.Sets)]) is { } longest
            ? new ExerciseBest(null, null, longest, null)
            : null;
}
