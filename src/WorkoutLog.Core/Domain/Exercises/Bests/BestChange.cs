namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>A session's best against an earlier one.</summary>
/// <param name="Kind">How the best compares.</param>
/// <param name="Difference">How much better or worse, as the one value that decides it (weight, else reps, else duration, else distance), never negative. Null unless <paramref name="Kind"/> is Up or Down.</param>
public sealed record BestChange(BestChangeKind Kind, ExerciseBest? Difference);
