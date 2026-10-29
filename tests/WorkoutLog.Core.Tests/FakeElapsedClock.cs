using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests;

/// <summary>An <see cref="IElapsedClock"/> the test sets and moves.</summary>
public sealed class FakeElapsedClock : IElapsedClock
{
    /// <inheritdoc />
    public TimeSpan Elapsed { get; set; } = TimeSpan.FromHours(5);

    /// <summary>Moves the reading on by <paramref name="time"/>.</summary>
    public void Advance(TimeSpan time) => Elapsed += time;
}
