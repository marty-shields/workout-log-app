using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a carry session's chart values.</summary>
internal interface ICarryRule
{
    /// <summary>The heaviest set of <paramref name="sets"/> with its distance, or null when no set is over 0 kg.</summary>
    ExerciseBest? Heaviest(IReadOnlyList<StrengthSet> sets);

    /// <summary>The longest set of <paramref name="sets"/>, with its weight when over 0 kg, or null when no set has a distance.</summary>
    ExerciseBest? Longest(IReadOnlyList<StrengthSet> sets);
}
