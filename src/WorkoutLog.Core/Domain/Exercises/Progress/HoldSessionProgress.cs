using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A hold session with its chart values.</summary>
/// <param name="Session">The session.</param>
/// <param name="Longest">Longest duration. Null when no set has one.</param>
/// <param name="Heaviest">Heaviest set over 0 kg with its duration; tie → longest. Null when no set is over 0 kg.</param>
public sealed record HoldSessionProgress(StrengthSession Session, TimeSpan? Longest, ExerciseBest? Heaviest);
