using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>Outcome of loading an exercise's last session.</summary>
/// <param name="Exercise">The exercise as logged in that workout, or null if there is no such workout or loading failed.</param>
/// <param name="Best">For a cardio entry, its distance, duration, pace, speed, floors and calories as the Exercise page works them out; otherwise null.</param>
/// <param name="Failed">True when the workout couldn't be read.</param>
public sealed record LastSessionResult(WorkoutExercise? Exercise, CardioBest? Best, bool Failed);
