using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>A logged exercises service for the stories app, which shows views only: every call throws.</summary>
public sealed class SampleLoggedExercisesService : ILoggedExercisesService
{
    /// <inheritdoc />
    public Task<LoggedExercisesResult> GetAsync() => throw new NotSupportedException("The stories don't load logged exercises.");
}
