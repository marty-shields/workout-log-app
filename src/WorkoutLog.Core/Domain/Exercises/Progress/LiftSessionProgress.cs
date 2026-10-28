using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A lift session with its chart values.</summary>
/// <param name="Session">The session.</param>
/// <param name="TopSet">Heaviest set; tie → most reps. Null when no set has a weight.</param>
/// <param name="E1rmKg">Best estimated one-rep max, unrounded. Null when no weighted set has 12 reps or fewer.</param>
public sealed record LiftSessionProgress(LiftSession Session, ExerciseBest? TopSet, decimal? E1rmKg);
