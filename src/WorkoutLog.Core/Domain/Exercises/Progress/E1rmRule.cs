using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Epley: weight × (1 + reps / 30); a 1-rep set is its weight. Uses sets with a weight and 1 to <see cref="MaxReps"/> reps.</summary>
internal sealed class E1rmRule : IE1rmRule
{
    /// <summary>Most reps a set can have to be used.</summary>
    public const int MaxReps = 12;

    /// <inheritdoc />
    public decimal? Best(IReadOnlyList<StrengthSet> sets) =>
        sets.Where(s => s is { WeightKg: not null, Reps: >= 1 and <= MaxReps })
            .Select(s => s.Reps == 1 ? s.WeightKg : s.WeightKg * (1 + s.Reps / 30m))
            .Max();
}
