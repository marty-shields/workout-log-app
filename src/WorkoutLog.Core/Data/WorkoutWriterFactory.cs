using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Creates a <see cref="WorkoutWriter"/>.</summary>
internal sealed class WorkoutWriterFactory
{
    /// <summary>Creates a writer on <paramref name="db"/>.</summary>
    public IWorkoutWriter Create(SQLiteConnection db) => new WorkoutWriter(db);
}
