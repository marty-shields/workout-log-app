using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A bodyweight move's records in a range.</summary>
/// <param name="MostRepsInSet">Most reps in an unweighted set. Null when no session has one.</param>
/// <param name="MostRepsInSession">Most unweighted reps in a session. Null when no session has any.</param>
/// <param name="HeaviestWeight">Heaviest top set over 0 kg; tie → most reps. Null when no set is over 0 kg.</param>
/// <param name="BestE1rmKg">Best estimated one-rep max, unrounded. Null when no session has one.</param>
public sealed record BodyweightRecords(
    ProgressRecord<int>? MostRepsInSet,
    ProgressRecord<int>? MostRepsInSession,
    ProgressRecord<ExerciseBest>? HeaviestWeight,
    ProgressRecord<decimal>? BestE1rmKg);
