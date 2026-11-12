namespace WorkoutLog.Core.Services;

/// <summary>The read failed. The service has logged why.</summary>
public sealed record ReadFailed;

/// <summary>What a read of the saved workouts returned, or <see cref="ReadFailed"/>.</summary>
public union ReadResult<T>(T, ReadFailed);
