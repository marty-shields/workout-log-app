namespace WorkoutLog.Core.Data;

/// <summary>Runs each call to the inner unit of work through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedUnitOfWork(IUnitOfWork inner, DatabaseGate gate) : IUnitOfWork
{
    /// <inheritdoc />
    public Task RunAsync(Action<IWorkoutWriter> write) => gate.RunAsync(() => inner.RunAsync(write));
}
