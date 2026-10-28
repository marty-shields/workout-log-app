using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>An assisted machine's records in a range.</summary>
/// <param name="LeastAssist">Least assist; tie → most reps. Null when no session has a set.</param>
public sealed record AssistedRecords(ProgressRecord<ExerciseBest>? LeastAssist);
