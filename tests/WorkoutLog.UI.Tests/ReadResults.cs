using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

internal static class ReadResults
{
    /// <summary>What the read returned; fails the test when it returned <see cref="ReadFailed"/> or null.</summary>
    public static T Loaded<T>(this ReadResult<T> result) =>
        result is T value ? value : throw new AssertionException($"Expected what the read returned, but it returned {result.Value ?? "null"}.");
}
