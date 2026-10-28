using SQLite;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Data;

/// <summary>Creates a <see cref="WorkoutWriter"/>.</summary>
internal sealed class WorkoutWriterFactory(IExerciseCatalog catalog) : IWorkoutWriterFactory
{
    /// <inheritdoc />
    public IWorkoutWriter Create(SQLiteConnection db) => new WorkoutWriter(db, catalog);
}
