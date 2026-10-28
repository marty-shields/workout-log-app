using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a hold session's chart values.</summary>
internal interface IHoldRule
{
    /// <summary>The longest duration of <paramref name="sets"/>, or null when no set has one.</summary>
    TimeSpan? Longest(IReadOnlyList<StrengthSet> sets);

    /// <summary>The heaviest set of <paramref name="sets"/> with its duration, or null when no set is over 0 kg.</summary>
    ExerciseBest? Heaviest(IReadOnlyList<StrengthSet> sets);
}
