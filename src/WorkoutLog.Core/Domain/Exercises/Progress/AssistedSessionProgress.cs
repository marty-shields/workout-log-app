using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>An assisted machine session with its chart value.</summary>
/// <param name="Session">The session.</param>
/// <param name="LeastAssist">Set with the least assist, blank as 0 kg; tie → most reps. Null when the session has no sets.</param>
public sealed record AssistedSessionProgress(StrengthSession Session, ExerciseBest? LeastAssist);
