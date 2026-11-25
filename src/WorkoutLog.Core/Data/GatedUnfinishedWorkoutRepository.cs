using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Runs each call to the inner repository through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedUnfinishedWorkoutRepository(IUnfinishedWorkoutRepository inner, DatabaseGate gate) : IUnfinishedWorkoutRepository
{
    /// <inheritdoc />
    public Task<UnfinishedWorkout?> GetAsync() => gate.RunAsync(inner.GetAsync);

    /// <inheritdoc />
    public Task ReplaceAsync(UnfinishedWorkout workout) => gate.RunAsync(() => inner.ReplaceAsync(workout));

    /// <inheritdoc />
    public Task DeleteAsync() => gate.RunAsync(inner.DeleteAsync);
}
