namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>An exercise's best in a session and how it compares with the exercise's earlier sessions.</summary>
/// <param name="Best">The session's best.</param>
/// <param name="Change">The best against the last earlier session that has one.</param>
internal sealed record SessionChange(ExerciseBest Best, BestChange Change);
