using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks an assisted machine session's least assist.</summary>
internal interface ILeastAssistRule
{
    /// <summary>The set of <paramref name="sets"/> with the least assist, or null when there are no sets.</summary>
    ExerciseBest? LeastAssist(IReadOnlyList<StrengthSet> sets);
}
