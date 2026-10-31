using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Schema step 5: adds a nullable <c>Note</c> of 1 to 500 characters to Workout and WorkoutExercise, where missing.</summary>
internal sealed class NotesSchema : ISchemaUpgrade
{
    private static readonly string[] Tables = ["Workout", "WorkoutExercise"];

    /// <inheritdoc />
    public int Version => 5;

    /// <inheritdoc />
    public void Apply(SQLiteConnection db)
    {
        foreach (var table in Tables)
        {
            if (db.ExecuteScalar<int>($"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = 'Note'") == 0)
            {
                db.Execute($"ALTER TABLE \"{table}\" ADD COLUMN \"Note\" varchar CHECK (\"Note\" IS NULL OR length(\"Note\") BETWEEN 1 AND 500)");
            }
        }
    }
}
