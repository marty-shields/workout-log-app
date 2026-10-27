using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Creates an <see cref="IWorkoutWriter"/> on a connection.</summary>
internal interface IWorkoutWriterFactory
{
    /// <summary>Returns a writer that writes on <paramref name="db"/>.</summary>
    IWorkoutWriter Create(SQLiteConnection db);
}
