using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A carry session with its chart values.</summary>
/// <param name="Session">The session.</param>
/// <param name="Heaviest">Heaviest set over 0 kg with its distance; tie → longest. Null when no set is over 0 kg.</param>
/// <param name="Longest">Longest set, with its weight when over 0 kg; tie → heaviest. Null when no set has a distance.</param>
public sealed record CarrySessionProgress(StrengthSession Session, ExerciseBest? Heaviest, ExerciseBest? Longest);
