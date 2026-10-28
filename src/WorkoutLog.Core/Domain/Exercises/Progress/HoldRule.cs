using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Longest: the longest duration. Heaviest: the heaviest set over 0 kg; tie → longest.</summary>
internal sealed class HoldRule : IHoldRule
{
    /// <inheritdoc />
    public TimeSpan? Longest(IReadOnlyList<StrengthSet> sets) => sets.Max(s => s.Duration);

    /// <inheritdoc />
    public ExerciseBest? Heaviest(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s.WeightKg > 0m)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Duration)
            .Select(s => new ExerciseBest(s.WeightKg, null, s.Duration, null))
            .FirstOrDefault();
}
