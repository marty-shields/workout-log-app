using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>The set with the least assist, with its reps; tie → most reps.</summary>
internal sealed class LeastAssistRule : ILeastAssistRule
{
    /// <inheritdoc />
    public ExerciseBest? LeastAssist(IReadOnlyList<StrengthSet> sets) =>
        sets.OrderBy(s => s.WeightKg ?? 0m)
            .ThenByDescending(s => s.Reps)
            .Select(s => new ExerciseBest(s.WeightKg ?? 0m, s.Reps, null, null))
            .FirstOrDefault();
}
