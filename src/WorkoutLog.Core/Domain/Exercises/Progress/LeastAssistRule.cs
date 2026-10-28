using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>The set with the least assist, blank as 0 kg; tie → most reps (<see cref="ExerciseBest.LeastAssist"/>).</summary>
internal sealed class LeastAssistRule : ILeastAssistRule
{
    /// <inheritdoc />
    public ExerciseBest? LeastAssist(IReadOnlyList<StrengthSet> sets) => ExerciseBest.LeastAssist(sets);
}
