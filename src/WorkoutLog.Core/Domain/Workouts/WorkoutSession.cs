namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>One saved workout's blocks of an exercise.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Exercises">The exercise as logged in that workout, in position order; usually one.</param>
internal sealed record WorkoutSession(int WorkoutId, DateTimeOffset StartedAt, IReadOnlyList<WorkoutExercise> Exercises);
