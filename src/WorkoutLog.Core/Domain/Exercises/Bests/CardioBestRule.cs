using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>
/// Cardio (<see cref="ExerciseCategory.Cardio"/>). Best is the longest distance when the exercise has one, null when not logged;
/// otherwise the longest duration (<see cref="ICardioSessionRule"/>).
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
}
