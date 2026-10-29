using Android.OS;
using WorkoutLog.Core.Services;

namespace WorkoutLog.App.Timing;

/// <summary>Android's <see cref="SystemClock.ElapsedRealtimeNanos"/>: time since the phone started, including sleep.</summary>
internal sealed class AndroidElapsedClock : IElapsedClock
{
    /// <inheritdoc />
    public TimeSpan Elapsed => TimeSpan.FromTicks(SystemClock.ElapsedRealtimeNanos() / 100);
}
