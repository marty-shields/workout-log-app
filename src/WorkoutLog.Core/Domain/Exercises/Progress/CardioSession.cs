using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A saved workout's entry of one cardio exercise.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Entry">The exercise's entry in the workout.</param>
public sealed record CardioSession(int WorkoutId, DateTimeOffset StartedAt, CardioEntry Entry);
