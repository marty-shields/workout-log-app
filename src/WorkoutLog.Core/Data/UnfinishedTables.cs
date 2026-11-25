using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>
/// The tables of the one unfinished workout, kept apart from the saved workouts so nothing that reads workouts sees it.
/// They have no CHECK rules: the text is kept as typed.
/// </summary>
internal static class UnfinishedTables
{
    private static readonly string[] CreateStatements =
    [
        """
        CREATE TABLE IF NOT EXISTS "UnfinishedWorkout" (
        "Id" integer primary key not null CHECK ("Id" = 1),
        "StartedAt" bigint not null,
        "Note" varchar
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS "UnfinishedExercise" (
        "Id" integer primary key autoincrement not null,
        "ExerciseId" integer not null,
        "Position" integer not null,
        "Note" varchar,
        "IsDone" integer not null
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS "UnfinishedEntry" (
        "Id" integer primary key autoincrement not null,
        "UnfinishedExerciseId" integer not null,
        "Position" integer not null,
        "FieldValues" varchar not null
        )
        """,
    ];

    /// <summary>Creates each missing table.</summary>
    public static void CreateMissing(SQLiteConnection db)
    {
        foreach (var statement in CreateStatements)
        {
            db.Execute(statement);
        }
    }

    /// <summary>Deletes the unfinished workout with its exercises and entries.</summary>
    public static void Clear(SQLiteConnection db)
    {
        db.DeleteAll<UnfinishedEntryRow>();
        db.DeleteAll<UnfinishedExerciseRow>();
        db.DeleteAll<UnfinishedWorkoutRow>();
    }
}
