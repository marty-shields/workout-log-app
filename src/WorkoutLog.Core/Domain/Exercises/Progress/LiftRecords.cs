using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A lift's records in a range.</summary>
/// <param name="HeaviestWeight">Heaviest top set; tie → most reps. Null when no set has a weight.</param>
/// <param name="BestE1rmKg">Best estimated one-rep max, unrounded. Null when no session has one.</param>
public sealed record LiftRecords(ProgressRecord<ExerciseBest>? HeaviestWeight, ProgressRecord<decimal>? BestE1rmKg);
