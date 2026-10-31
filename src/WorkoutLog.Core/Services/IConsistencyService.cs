namespace WorkoutLog.Core.Services;

/// <summary>Reads how often the user trains, for the Dashboard's Consistency card.</summary>
public interface IConsistencyService
{
    /// <summary>
    /// Returns this week against last week, workouts per week for the last 12 weeks and the weekly streak, with weeks from Monday 00:00
    /// in the phone's time zone. A failed read returns <see cref="ConsistencyResult.FailedRead"/>.
    /// </summary>
    Task<ConsistencyResult> GetAsync();
}
