using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>The heaviest set with its reps; tie → most reps. Sets with no weight are left out.</summary>
internal sealed class TopSetRule : ITopSetRule
{
    /// <inheritdoc />
    public ExerciseBest? TopSet(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s.WeightKg is not null)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Reps)
            .Select(s => new ExerciseBest(s.WeightKg, s.Reps, null, null))
            .FirstOrDefault();
}
