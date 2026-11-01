using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>
/// Cardio (<see cref="ExerciseCategory.Cardio"/>). Best is the entry's distance when the exercise has a distance metric, else its duration;
/// null when that value is empty (<see cref="ICardioSessionRule"/>). Higher beats lower.
/// </summary>
internal sealed class CardioBestRule(ICardioSessionRule sessionRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Cardio;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged)
    {
        if (logged is not CardioExercise cardio)
        {
            return null;
        }

        var best = sessionRule.Best(cardio.Entry);
        if (exercise.Metrics.Any(m => m.Metric == Metric.Distance))
        {
            return best.Distance is { } distance ? new ExerciseBest(null, null, null, distance) : null;
        }

        return best.Duration is { } duration ? new ExerciseBest(null, null, duration, null) : null;
    }

    /// <inheritdoc />
    public int Compare(ExerciseBest x, ExerciseBest y) => (x.Distance, x.Duration).CompareTo((y.Distance, y.Duration));
}
