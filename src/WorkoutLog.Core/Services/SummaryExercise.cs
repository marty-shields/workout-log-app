using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Services;

/// <summary>An exercise's best in a saved workout.</summary>
/// <param name="Best">The workout's best.</param>
/// <param name="IsRecord">True when the best beats the exercise's best in every earlier session.</param>
/// <param name="Change">The best against the exercise's last session before the workout.</param>
public sealed record SummaryExercise(int ExerciseId, ExerciseBest Best, bool IsRecord, BestChange Change);
