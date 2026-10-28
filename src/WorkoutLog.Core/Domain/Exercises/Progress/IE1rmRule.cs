using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Estimates a lift session's one-rep max.</summary>
internal interface IE1rmRule
{
    /// <summary>The best estimate over <paramref name="sets"/>, unrounded, or null when no set can be used.</summary>
    decimal? Best(IReadOnlyList<StrengthSet> sets);
}
