namespace WorkoutLog.Core.Services;

/// <summary>What a saved workout did and what improved.</summary>
/// <param name="Duration">Finish minus start.</param>
/// <param name="ExerciseCount">Number of exercises.</param>
/// <param name="SetCount">Number of strength sets.</param>
/// <param name="HasEarlierWorkout">True when a workout started before this one.</param>
/// <param name="Exercises">The exercises that have a best, in the workout's order.</param>
public sealed record SavedWorkoutSummary(TimeSpan Duration, int ExerciseCount, int SetCount, bool HasEarlierWorkout, IReadOnlyList<SummaryExercise> Exercises);
