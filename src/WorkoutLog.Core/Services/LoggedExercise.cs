using WorkoutLog.Core.Domain.Exercises.Bests;

namespace WorkoutLog.Core.Services;

/// <summary>An exercise with a saved workout.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="LastStartedAt">Start time of its last saved workout, UTC.</param>
/// <param name="Best">Best of that session. Null when there is nothing to show, e.g. a lift with no weight.</param>
public sealed record LoggedExercise(int ExerciseId, DateTimeOffset LastStartedAt, ExerciseBest? Best);
