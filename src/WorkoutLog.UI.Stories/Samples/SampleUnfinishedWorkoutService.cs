using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// <see cref="IUnfinishedWorkoutService"/> for the stories and the UI tests: keeps the workout in memory in <see cref="Stored"/>.
/// <see cref="SaveAsync"/> and <see cref="DiscardAsync"/> fail while <see cref="FailWrites"/> is set; <see cref="GetSummaryAsync"/> and <see cref="GetAsync"/> return <see cref="ReadFailed"/> while <see cref="FailReads"/> is set.
/// </summary>
public sealed class SampleUnfinishedWorkoutService() : IUnfinishedWorkoutService
{
    /// <summary>The stored workout; null when there is none. Set it to start with one.</summary>
    public UnfinishedWorkout? Stored { get; set; }

    /// <summary>Writes fail and change nothing.</summary>
    public bool FailWrites { get; set; }

    /// <summary>Reads return <see cref="ReadFailed"/>.</summary>
    public bool FailReads { get; set; }

    /// <summary>Number of <see cref="SaveAsync"/> calls, whether they failed or not.</summary>
    public int Saves { get; private set; }

    /// <inheritdoc />
    public Task<UnfinishedSummaryResult> GetSummaryAsync() =>
        Task.FromResult<UnfinishedSummaryResult>((FailReads, Stored) switch
        {
            (true, _) => new ReadFailed(),
            (_, null) => new NoUnfinishedWorkout(),
            (_, var stored) => UnfinishedSummary.Of(stored),
        });

    /// <inheritdoc />
    public Task<UnfinishedWorkoutResult> GetAsync() =>
        Task.FromResult<UnfinishedWorkoutResult>((FailReads, Stored) switch
        {
            (true, _) => new ReadFailed(),
            (_, null) => new NoUnfinishedWorkout(),
            (_, var stored) => stored,
        });

    /// <inheritdoc />
    public Task<UnfinishedWrite> SaveAsync(UnfinishedWorkout workout)
    {
        Saves++;
        if (FailWrites)
        {
            return Task.FromResult(UnfinishedWrite.Failed);
        }

        Stored = workout.Exercises.Count == 0 ? null : workout;
        return Task.FromResult(UnfinishedWrite.Done);
    }

    /// <inheritdoc />
    public Task<UnfinishedWrite> DiscardAsync()
    {
        if (FailWrites)
        {
            return Task.FromResult(UnfinishedWrite.Failed);
        }

        Stored = null;
        return Task.FromResult(UnfinishedWrite.Done);
    }
}
