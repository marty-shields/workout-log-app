using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>No saved workout has the id.</summary>
public sealed record WorkoutNotFound;

/// <summary>The saved workout, or why it couldn't be opened.</summary>
public union WorkoutResult(Workout, WorkoutNotFound, ReadFailed);
