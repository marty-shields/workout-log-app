using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A bodyweight session with its chart values. A set with no weight or 0 kg is unweighted.</summary>
/// <param name="Session">The session.</param>
/// <param name="BestSetReps">Most reps in an unweighted set. Null when no unweighted set has reps.</param>
/// <param name="TotalReps">Reps of every unweighted set added up. Null when no unweighted set has reps.</param>
/// <param name="TopSet">Heaviest set over 0 kg; tie → most reps. Null when no set is over 0 kg.</param>
/// <param name="E1rmKg">Best estimated one-rep max of the sets over 0 kg, unrounded. Null when none has 12 reps or fewer.</param>
public sealed record BodyweightSessionProgress(StrengthSession Session, int? BestSetReps, int? TotalReps, ExerciseBest? TopSet, decimal? E1rmKg);
