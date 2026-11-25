using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Services;

/// <summary>There is no unfinished workout.</summary>
public sealed record NoUnfinishedWorkout;

public union UnfinishedSummaryResult(UnfinishedSummary, NoUnfinishedWorkout, ReadFailed);

public union UnfinishedWorkoutResult(UnfinishedWorkout, NoUnfinishedWorkout, ReadFailed);
