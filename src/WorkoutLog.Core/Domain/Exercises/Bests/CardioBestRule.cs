using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Cardio. Best is the distance when the exercise has one, null when not logged; otherwise the duration.</summary>
internal sealed class CardioBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Type == ExerciseType.Cardio;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged)
    {
        var entries = logged.OfType<CardioExercise>().Select(e => e.Entry).ToList();
        if (exercise.Metrics.Any(m => m.Metric == Metric.Distance))
        {
            return entries.Max(e => e.Distance) is { } distance ? new ExerciseBest(null, null, null, distance) : null;
        }

        return entries.Max(e => e.Duration) is { } duration ? new ExerciseBest(null, null, duration, null) : null;
    }
}
