namespace WorkoutLog.Core.Data;

/// <summary>Runs several workout writes as one transaction.</summary>
internal interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="write"/> in one transaction: all its writes are kept, or none if it throws. Rethrows the exception.
    /// <paramref name="write"/> must be synchronous: an async lambda's writes after its first await run outside the transaction.
    /// </summary>
    Task RunAsync(Action<IWorkoutWriter> write);
}
