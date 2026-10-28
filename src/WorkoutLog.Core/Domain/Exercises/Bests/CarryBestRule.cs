using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Carries: strength with a required distance. Best is the heaviest set with its distance; tie → longest. Null when no set has a weight.</summary>
internal sealed class CarryBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) =>
        exercise.Type == ExerciseType.Strength && exercise.Metrics.Any(m => m is { Metric: Metric.Distance, Required: true });

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        logged.OfType<StrengthExercise>().SelectMany(e => e.Sets)
            .Where(s => s.WeightKg is not null)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Distance)
            .Select(s => new ExerciseBest(s.WeightKg, null, null, s.Distance))
            .FirstOrDefault();
}
