using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Schema step 6: adds the <see cref="UnfinishedTables"/> where missing, so an older app refuses a database that holds an unfinished workout.</summary>
internal sealed class UnfinishedSchema : ISchemaUpgrade
{
    /// <inheritdoc />
    public int Version => 6;

    /// <inheritdoc />
    public void Apply(SQLiteConnection db) => UnfinishedTables.CreateMissing(db);
}
