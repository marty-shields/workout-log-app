using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>The best of one session. Values the best doesn't show are null.</summary>
/// <param name="WeightKg">Weight in kg; assistance on assisted machines.</param>
/// <param name="Reps">Repetitions.</param>
/// <param name="Duration">Duration.</param>
/// <param name="Distance">Distance, in the exercise's unit.</param>
public sealed record ExerciseBest(decimal? WeightKg, int? Reps, TimeSpan? Duration, decimal? Distance)
{
    /// <summary>The heaviest set with its reps; tie → most reps. Null when no set has a weight.</summary>
    internal static ExerciseBest? TopSet(IEnumerable<StrengthSet> sets) =>
        sets.Where(s => s.WeightKg is not null)
            .OrderByDescending(s => s.WeightKg)
            .ThenByDescending(s => s.Reps)
            .Select(s => new ExerciseBest(s.WeightKg, s.Reps, null, null))
            .FirstOrDefault();

    /// <summary>The set with the least assist, blank as 0 kg, with its reps; tie → most reps. Null when there are no sets.</summary>
    internal static ExerciseBest? LeastAssist(IEnumerable<StrengthSet> sets) =>
        sets.OrderBy(s => s.WeightKg ?? 0m)
            .ThenByDescending(s => s.Reps)
            .Select(s => new ExerciseBest(s.WeightKg ?? 0m, s.Reps, null, null))
            .FirstOrDefault();
}
