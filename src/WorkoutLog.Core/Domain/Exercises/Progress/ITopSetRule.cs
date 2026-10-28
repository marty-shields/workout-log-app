using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a lift session's top set.</summary>
internal interface ITopSetRule
{
    /// <summary>The top set of <paramref name="sets"/>, or null when no set has a weight.</summary>
    ExerciseBest? TopSet(IReadOnlyList<StrengthSet> sets);
}
