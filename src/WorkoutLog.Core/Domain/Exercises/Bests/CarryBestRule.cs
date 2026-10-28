using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Carries (<see cref="ExerciseCategory.Carry"/>). Best is the heaviest set over 0 kg with its distance; tie → longest. Null when no set is over 0 kg.</summary>
internal sealed class CarryBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Carry;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged) =>
        logged.OfType<StrengthExercise>().SelectMany(e => e.Sets)
            .Where(s => s.WeightKg > 0m)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Distance)
            .Select(s => new ExerciseBest(s.WeightKg, null, null, s.Distance))
            .FirstOrDefault();
}
