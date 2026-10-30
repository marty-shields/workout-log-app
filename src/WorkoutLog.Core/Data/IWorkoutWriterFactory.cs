using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Creates an <see cref="IWorkoutWriter"/> on a connection.</summary>
internal interface IWorkoutWriterFactory
{
    IWorkoutWriter Create(SQLiteConnection db);
}
