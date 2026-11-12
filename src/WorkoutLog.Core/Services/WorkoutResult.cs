using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

public sealed record WorkoutNotFound;

public union WorkoutResult(Workout, WorkoutNotFound, ReadFailed);
