namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>The number of strength sets logged for one exercise in a saved workout.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="Sets">Strength sets, at least 1.</param>
internal sealed record ExerciseSetCount(int WorkoutId, int ExerciseId, int Sets);
