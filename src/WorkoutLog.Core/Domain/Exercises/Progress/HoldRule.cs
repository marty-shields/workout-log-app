using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Longest: the longest duration. Heaviest: the heaviest set over 0 kg; tie → longest.</summary>
internal sealed class HoldRule
{
    /// <summary>The longest duration of <paramref name="sets"/>, or null when no set has one.</summary>
    public TimeSpan? Longest(IReadOnlyList<StrengthSet> sets) => sets.Max(s => s.Duration);

    /// <summary>The heaviest set of <paramref name="sets"/> with its duration, or null when no set is over 0 kg.</summary>
    public ExerciseBest? Heaviest(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s.WeightKg > 0m)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Duration)
            .Select(s => new ExerciseBest(s.WeightKg, null, s.Duration, null))
            .FirstOrDefault();
}
