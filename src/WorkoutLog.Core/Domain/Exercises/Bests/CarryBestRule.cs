using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Carries (<see cref="ExerciseCategory.Carry"/>). Best is the heaviest set over 0 kg with its distance (<see cref="ICarryRule.Heaviest"/>); heavier beats lighter, tie → farther.</summary>
internal sealed class CarryBestRule(ICarryRule carryRule) : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Carry;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, WorkoutExercise logged) =>
        carryRule.Heaviest((logged as StrengthExercise)?.Sets ?? []);

    /// <inheritdoc />
    public int Compare(ExerciseBest x, ExerciseBest y) => (x.WeightKg, x.Distance).CompareTo((y.WeightKg, y.Distance));
}
