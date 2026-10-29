using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>
/// A numbered schema upgrade step: a change sqlite-net can't make itself (rename, move or transform data, change an existing index, or rebuild a table).
/// </summary>
internal interface ISchemaUpgrade
{
    /// <summary>Schema version the step upgrades to. Steps start at 2.</summary>
    int Version { get; }

    /// <summary>
    /// Applies the step. Runs inside a transaction that also sets the schema version.
    /// Sees the schema left by the previous step; sqlite-net adds new tables and columns after the last step.
    /// </summary>
    void Apply(SQLiteConnection db);
}
