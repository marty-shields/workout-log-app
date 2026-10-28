using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A saved workout's entries of one cardio exercise.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Entries">Every entry of the exercise in the workout, in workout order; usually one.</param>
public sealed record CardioSession(int WorkoutId, DateTimeOffset StartedAt, IReadOnlyList<CardioEntry> Entries);
