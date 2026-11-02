namespace WorkoutLog.Core.Services;

/// <summary>A new workout that hasn't been saved yet. Created by <see cref="IWorkoutService.StartNew"/>.</summary>
public sealed record WorkoutDraft
{
    /// <summary>A draft started at <paramref name="startedAt"/>. Call <see cref="IWorkoutService.StartNew"/> instead outside the stories.</summary>
    public WorkoutDraft(DateTimeOffset startedAt, TimeSpan startedElapsed)
    {
        StartedAt = startedAt;
        StartedElapsed = startedElapsed;
    }

    /// <summary>Start time, UTC.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>The <see cref="IElapsedClock"/> reading at the start.</summary>
    internal TimeSpan StartedElapsed { get; }
}
