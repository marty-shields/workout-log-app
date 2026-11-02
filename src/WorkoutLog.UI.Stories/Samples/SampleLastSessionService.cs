using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>A last session service for the stories app, which shows views only: every call throws.</summary>
public sealed class SampleLastSessionService : ILastSessionService
{
    /// <inheritdoc />
    public Task<LastSessionResult> GetAsync(int exerciseId, DateTimeOffset? before = null) => throw new NotSupportedException("The stories don't load last sessions.");
}
