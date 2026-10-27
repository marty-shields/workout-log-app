using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Runs the writes in a SQLite transaction with a writer from <see cref="IWorkoutWriterFactory"/>.</summary>
internal sealed class UnitOfWork(SQLiteAsyncConnection connection, IWorkoutWriterFactory writers) : IUnitOfWork
{
    /// <inheritdoc />
    public Task RunAsync(Action<IWorkoutWriter> write) =>
        connection.RunInTransactionAsync(db => write(writers.Create(db)));
}
