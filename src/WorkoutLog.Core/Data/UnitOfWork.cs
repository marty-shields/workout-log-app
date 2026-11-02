using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Runs the writes in a SQLite transaction with a writer from <see cref="WorkoutWriterFactory"/>.</summary>
internal sealed class UnitOfWork(SQLiteAsyncConnection connection, WorkoutWriterFactory writers) : IUnitOfWork
{
    /// <inheritdoc />
    public Task RunAsync(Action<IWorkoutWriter> write) =>
        connection.RunInTransactionAsync(db => write(writers.Create(db)));
}
