using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// Keeps the new workout being logged in <see cref="IUnfinishedWorkoutService"/> as it changes, without holding up typing: a change is queued, not written, and
/// the writes run off the caller's path. Changes within <see cref="Window"/> of each other are written together, as the newest one. One instance for the app, so
/// the host can <see cref="FlushAsync"/> it when the app is paused or stopped.
/// </summary>
/// <remarks>A failed write is logged by the service and left: the workout stays in memory, and the next change writes it again.</remarks>
public sealed class UnfinishedAutosaver(IUnfinishedWorkoutService unfinished, TimeProvider clock) : IDisposable
{
    /// <summary>How long a change waits for others before it is written.</summary>
    public static readonly TimeSpan Window = TimeSpan.FromMilliseconds(500);

    private readonly Lock gate = new();
    private readonly SemaphoreSlim writing = new(1, 1);
    private UnfinishedWorkout? pending;
    private bool active;
    private bool waiting;

    /// <summary>Starts keeping a new workout: queued changes are written from now, and any left from an earlier workout are dropped.</summary>
    public void Begin()
    {
        lock (gate)
        {
            active = true;
            pending = null;
        }
    }

    /// <summary>Queues <paramref name="workout"/> as the newest state to write. Ignored unless <see cref="Begin"/> has been called since the last <see cref="StopAsync"/>.</summary>
    public void Queue(UnfinishedWorkout workout)
    {
        lock (gate)
        {
            if (!active)
            {
                return;
            }

            pending = workout;
            if (waiting)
            {
                return;
            }

            waiting = true;
        }

        _ = WriteAfterWindowAsync();
    }

    /// <summary>Writes the queued state now, and waits for a write in progress. Does nothing when none is queued.</summary>
    public Task FlushAsync() => WritePendingAsync();

    /// <summary>
    /// Drops the queued state, waits for a write in progress and ignores changes until <see cref="Begin"/>, so nothing is written after the workout
    /// is saved or discarded.
    /// </summary>
    public async Task StopAsync()
    {
        lock (gate)
        {
            active = false;
            pending = null;
        }

        await writing.WaitAsync();
        writing.Release();
    }

    /// <inheritdoc />
    public void Dispose() => writing.Dispose();

    private async Task WriteAfterWindowAsync()
    {
        await Task.Delay(Window, clock);
        lock (gate)
        {
            waiting = false;
        }

        await WritePendingAsync();
    }

    private async Task WritePendingAsync()
    {
        await writing.WaitAsync();
        try
        {
            UnfinishedWorkout? next;
            lock (gate)
            {
                next = pending;
                pending = null;
            }

            if (next is not null)
            {
                await unfinished.SaveAsync(next);
            }
        }
        finally
        {
            writing.Release();
        }
    }
}
