namespace WorkoutLog.Core.Services;

/// <summary>The service has logged why the read failed.</summary>
public sealed record ReadFailed;

public union ReadResult<T>(T, ReadFailed);
