namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>One saved workout's block of an exercise.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Exercise">The exercise as logged in that workout.</param>
internal sealed record WorkoutSession(int WorkoutId, DateTimeOffset StartedAt, WorkoutExercise Exercise);
