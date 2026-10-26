using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Schema step 2: replaces the plain <c>Workout_StartedAt</c> index with a unique one. Fails if two workouts have the same start.</summary>
internal sealed class WorkoutStartedAtUniqueIndex : ISchemaUpgrade
{
    /// <inheritdoc />
    public int Version => 2;

    /// <inheritdoc />
    public void Apply(SQLiteConnection db)
    {
        db.Execute("DROP INDEX IF EXISTS \"Workout_StartedAt\"");
        db.Execute("CREATE UNIQUE INDEX \"Workout_StartedAt\" ON \"Workout\"(\"StartedAt\")");
    }
}
