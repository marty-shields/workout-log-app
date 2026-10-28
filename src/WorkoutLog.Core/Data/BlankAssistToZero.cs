using SQLite;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Data;

/// <summary>Schema step 3: sets each blank weight on an assisted machine (<see cref="ExerciseDefinition.IsAssisted"/>) to 0 (D23).</summary>
internal sealed class BlankAssistToZero(IExerciseCatalog catalog) : ISchemaUpgrade
{
    /// <inheritdoc />
    public int Version => 3;

    /// <inheritdoc />
    public void Apply(SQLiteConnection db)
    {
        var ids = string.Join(", ", catalog.All.Where(e => e.IsAssisted).Select(e => e.Id));
        db.Execute($"""
            UPDATE StrengthSet SET WeightKg = 0
            WHERE WeightKg IS NULL
              AND WorkoutExerciseId IN (SELECT Id FROM WorkoutExercise WHERE ExerciseId IN ({ids}))
            """);
    }
}
