namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>An exercise's last saved workout: the one with the latest start that has the exercise.</summary>
/// <param name="ExerciseId">Exercise id.</param>
/// <param name="StartedAt">Start time of that workout, UTC.</param>
/// <param name="Exercise">The exercise as logged in that workout.</param>
internal sealed record LastSession(int ExerciseId, DateTimeOffset StartedAt, WorkoutExercise Exercise);
