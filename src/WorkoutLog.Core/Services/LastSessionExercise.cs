using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>An exercise as logged in its last saved workout.</summary>
/// <param name="Best">For a cardio entry, its distance, duration, pace, speed, floors and calories as the Exercise page works them out; otherwise null.</param>
public sealed record LastSessionExercise(WorkoutExercise Exercise, CardioBest? Best);
