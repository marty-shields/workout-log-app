using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>
/// Bodyweight moves (<see cref="ExerciseCategory.Bodyweight"/>). Best is the top set when a set has more than 0 kg, else the
/// unweighted set with the most reps (<see cref="IBodyweightRule"/>). Heavier beats lighter, unweighted being 0 kg; tie → more reps.
/// </summary>
internal sealed class BodyweightBestRule(IBodyweightRule bodyweightRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Bodyweight;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged)
    {
        var progress = bodyweightRule.Progress(new StrengthSession(0, default, (logged as StrengthExercise)?.Sets ?? []));
        return progress.TopSet ?? (progress.BestSetReps is { } reps ? new ExerciseBest(null, reps, null, null) : null);
    }

    /// <inheritdoc />
    public int Compare(ExerciseBest x, ExerciseBest y) => (x.WeightKg ?? 0m, x.Reps).CompareTo((y.WeightKg ?? 0m, y.Reps));
}
