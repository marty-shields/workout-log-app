using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Holds: strength with a required duration. Best is the longest.</summary>
internal sealed class HoldBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) =>
        exercise.Type == ExerciseType.Strength && exercise.Metrics.Any(m => m is { Metric: Metric.Duration, Required: true });

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        logged.OfType<StrengthExercise>().SelectMany(e => e.Sets)
            .Where(s => s.Duration is not null)
            .OrderByDescending(s => s.Duration)
            .Select(s => new ExerciseBest(null, null, s.Duration, null))
            .FirstOrDefault();
}
