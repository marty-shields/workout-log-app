namespace WorkoutLog.Core.Services;

/// <summary>A new workout that hasn't been saved yet. Created only by <see cref="IWorkoutService.StartNew"/>.</summary>
public sealed record WorkoutDraft
{
    internal WorkoutDraft(DateTimeOffset startedAt) => StartedAt = startedAt;

    /// <summary>Start time, UTC.</summary>
    public DateTimeOffset StartedAt { get; }
}
