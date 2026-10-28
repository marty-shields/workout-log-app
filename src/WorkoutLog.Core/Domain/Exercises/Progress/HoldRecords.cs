using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A hold's records in a range.</summary>
/// <param name="LongestHold">Longest duration. Null when no set has one.</param>
/// <param name="HeaviestWeight">Heaviest set over 0 kg with its duration; tie → longest. Null when no set is over 0 kg.</param>
public sealed record HoldRecords(ProgressRecord<TimeSpan>? LongestHold, ProgressRecord<ExerciseBest>? HeaviestWeight);
