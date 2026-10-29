using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Heaviest: the heaviest set over 0 kg; tie → longest. Longest: the longest set; tie → heaviest.
/// A weight of 0 kg is left out.
/// </summary>
internal sealed class CarryRule : ICarryRule
{
    /// <inheritdoc />
    public ExerciseBest? Heaviest(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s.WeightKg > 0m)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Distance)
            .Select(s => new ExerciseBest(s.WeightKg, null, null, s.Distance))
            .FirstOrDefault();

    /// <inheritdoc />
    public ExerciseBest? Longest(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s.Distance is not null)
            .OrderByDescending(s => s.Distance)
            .ThenByDescending(s => s.WeightKg ?? 0m)
            .Select(s => new ExerciseBest(s.WeightKg > 0m ? s.WeightKg : null, null, null, s.Distance))
            .FirstOrDefault();
}
