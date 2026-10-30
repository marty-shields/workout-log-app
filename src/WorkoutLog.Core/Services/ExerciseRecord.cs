using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Services;

/// <summary>A record: a session whose best beat the exercise's best in every earlier session.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="StartedAt">Start time of the session, UTC.</param>
/// <param name="Best">The session's best.</param>
public sealed record ExerciseRecord(int ExerciseId, DateTimeOffset StartedAt, ExerciseBest Best);
