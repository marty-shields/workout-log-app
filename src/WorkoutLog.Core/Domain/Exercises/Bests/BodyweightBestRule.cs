using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Bodyweight moves (<see cref="ExerciseCategory.Bodyweight"/>). Best is the top set when a set has more than 0 kg, else the set with the most reps.</summary>
internal sealed class BodyweightBestRule : IBestRule
{
    /// <inheritdoc />
    public bool AppliesTo(ExerciseDefinition exercise) => exercise.Category == ExerciseCategory.Bodyweight;

    /// <inheritdoc />
    public ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged)
    {
        var sets = logged.OfType<StrengthExercise>().SelectMany(e => e.Sets).ToList();
        return ExerciseBest.TopSet(sets.Where(s => s.WeightKg > 0m))
            ?? sets.Where(s => s.Reps is not null).OrderByDescending(s => s.Reps).Select(s => new ExerciseBest(null, s.Reps, null, null)).FirstOrDefault();
    }
}
