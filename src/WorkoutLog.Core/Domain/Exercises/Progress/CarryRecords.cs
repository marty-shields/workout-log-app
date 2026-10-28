using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A carry's records in a range.</summary>
/// <param name="HeaviestWeight">Heaviest set over 0 kg with its distance; tie → longest. Null when no set is over 0 kg.</param>
/// <param name="LongestDistance">Longest set, with its weight when over 0 kg; tie → heaviest. Null when no set has a distance.</param>
public sealed record CarryRecords(ProgressRecord<ExerciseBest>? HeaviestWeight, ProgressRecord<ExerciseBest>? LongestDistance);
