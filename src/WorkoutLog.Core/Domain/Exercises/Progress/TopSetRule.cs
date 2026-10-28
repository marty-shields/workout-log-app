using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>The heaviest set; tie → most reps (<see cref="ExerciseBest.TopSet"/>).</summary>
internal sealed class TopSetRule : ITopSetRule
{
    /// <inheritdoc />
    public ExerciseBest? TopSet(IReadOnlyList<StrengthSet> sets) => ExerciseBest.TopSet(sets);
}
