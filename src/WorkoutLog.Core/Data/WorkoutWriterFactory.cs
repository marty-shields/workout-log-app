using SQLite;

namespace WorkoutLog.Core.Data;

internal sealed class WorkoutWriterFactory
{
    /// <summary>Creates a writer on <paramref name="db"/>.</summary>
    public IWorkoutWriter Create(SQLiteConnection db) => new WorkoutWriter(db);
}
