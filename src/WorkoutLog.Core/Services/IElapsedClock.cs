namespace WorkoutLog.Core.Services;

/// <summary>A steady clock for timing a workout: counts through sleep and isn't moved by changes to the phone's clock. The host registers it.</summary>
public interface IElapsedClock
{
    /// <summary>Time since a fixed point, such as the phone's start.</summary>
    TimeSpan Elapsed { get; }
}
