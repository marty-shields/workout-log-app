using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Creates a <see cref="WorkoutWriter"/>.</summary>
internal sealed class WorkoutWriterFactory : IWorkoutWriterFactory
{
    /// <inheritdoc />
    public IWorkoutWriter Create(SQLiteConnection db) => new WorkoutWriter(db);
}
