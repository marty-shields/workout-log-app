using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>
/// Bodyweight-style: strength with required reps, not assisted, equipment Bodyweight, Other or Band.
/// Best is the top set when a set has a weight, else the set with the most reps.
/// </summary>
internal sealed class BodyweightBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) =>
        exercise.Type == ExerciseType.Strength
        && !exercise.IsAssisted
        && exercise.Metrics.Any(m => m is { Metric: Metric.Reps, Required: true })
        && IsBodyweightEquipment(exercise.Equipment);

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged)
    {
        var sets = logged.OfType<StrengthExercise>().SelectMany(e => e.Sets).ToList();
        return ExerciseBest.TopSet(sets)
            ?? sets.Where(s => s.Reps is not null).OrderByDescending(s => s.Reps).Select(s => new ExerciseBest(null, s.Reps, null, null)).FirstOrDefault();
    }

    /// <summary>True for Bodyweight, Other and Band.</summary>
    internal static bool IsBodyweightEquipment(Equipment? equipment) =>
        equipment is Equipment.Bodyweight or Equipment.Other or Equipment.Band;
}
