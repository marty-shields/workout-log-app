using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>
/// The Workout, WorkoutExercise, StrengthSet and CardioEntry tables, created by SQL with their CHECK rules and indexes.
/// A rule on a required column also fails on NULL, so a count of rows that break it finds them.
/// Column types and index names match sqlite-net's, so its table and index creation afterwards changes nothing.
/// </summary>
internal static class WorkoutTables
{
    private const long TicksPerSecond = TimeSpan.TicksPerSecond;

    private static readonly long MaxDurationTicks = Domain.Exercises.MetricLimits.MaxDuration.Ticks;

    /// <summary>Every table, parents first.</summary>
    public static readonly IReadOnlyList<Table> All =
    [
        new("Workout",
            """
            "Id" integer primary key autoincrement not null,
            "StartedAt" bigint not null,
            "FinishedAt" bigint not null
            """,
            ["Id", "StartedAt", "FinishedAt"],
            [new("finish after start", "\"StartedAt\" IS NOT NULL AND \"FinishedAt\" IS NOT NULL AND \"FinishedAt\" > \"StartedAt\"")],
            ["CREATE UNIQUE INDEX {0} \"Workout_StartedAt\" ON \"Workout\"(\"StartedAt\")"]),
        new("WorkoutExercise",
            """
            "Id" integer primary key autoincrement not null,
            "WorkoutId" integer not null,
            "ExerciseId" integer not null,
            "Position" integer not null
            """,
            ["Id", "WorkoutId", "ExerciseId", "Position"],
            [Required("WorkoutId"), Required("ExerciseId"), Position()],
            [
                "CREATE INDEX {0} \"WorkoutExercise_WorkoutId\" ON \"WorkoutExercise\"(\"WorkoutId\")",
                "CREATE UNIQUE INDEX {0} \"WorkoutExercise_WorkoutId_ExerciseId\" ON \"WorkoutExercise\"(\"WorkoutId\", \"ExerciseId\")",
            ]),
        new("StrengthSet",
            """
            "Id" integer primary key autoincrement not null,
            "WorkoutExerciseId" integer not null,
            "Position" integer not null,
            "Reps" integer,
            "WeightKg" float not null,
            "Duration" bigint,
            "Distance" float
            """,
            ["Id", "WorkoutExerciseId", "Position", "Reps", "WeightKg", "Duration", "Distance"],
            [
                Required("WorkoutExerciseId"),
                Position(),
                Whole("Reps", 1, 1_000),
                new("WeightKg 0 to 1000, 2 decimal places", $"\"WeightKg\" IS NOT NULL AND \"WeightKg\" BETWEEN 0 AND 1000 AND {TwoPlaces("WeightKg")}"),
                Duration(optional: true),
                Distance(),
            ],
            ["CREATE INDEX {0} \"StrengthSet_WorkoutExerciseId\" ON \"StrengthSet\"(\"WorkoutExerciseId\")"]),
        new("CardioEntry",
            """
            "Id" integer primary key autoincrement not null,
            "WorkoutExerciseId" integer not null,
            "Duration" bigint not null,
            "Distance" float,
            "Floors" integer,
            "Level" integer,
            "Incline" float,
            "Calories" integer,
            "Rate" float
            """,
            ["Id", "WorkoutExerciseId", "Duration", "Distance", "Floors", "Level", "Incline", "Calories", "Rate"],
            [
                Required("WorkoutExerciseId"),
                Duration(optional: false),
                Distance(),
                Whole("Floors", 1, 1_000),
                Whole("Level", 1, 100),
                new("Incline 0 to 100, 2 decimal places", $"\"Incline\" IS NULL OR (\"Incline\" BETWEEN 0 AND 100 AND {TwoPlaces("Incline")})"),
                Whole("Calories", 1, 10_000),
                new("Rate over 0 to 1000, 2 decimal places", $"\"Rate\" IS NULL OR (\"Rate\" > 0 AND \"Rate\" <= 1000 AND {TwoPlaces("Rate")})"),
            ],
            ["CREATE UNIQUE INDEX {0} \"CardioEntry_WorkoutExerciseId\" ON \"CardioEntry\"(\"WorkoutExerciseId\")"]),
    ];

    /// <summary>Creates each missing table and index.</summary>
    public static void CreateMissing(SQLiteConnection db)
    {
        foreach (var table in All)
        {
            db.Execute(table.CreateSql(table.Name, ifNotExists: true));
            foreach (var index in table.Indexes)
            {
                db.Execute(string.Format(index, "IF NOT EXISTS"));
            }
        }
    }

    private static Check Required(string column) => new($"{column} required", $"\"{column}\" IS NOT NULL");

    private static Check Position() => new("Position 0 to 9", "\"Position\" IS NOT NULL AND \"Position\" BETWEEN 0 AND 9");

    private static Check Whole(string column, int min, int max) =>
        new($"{column} {min} to {max}", $"\"{column}\" IS NULL OR \"{column}\" BETWEEN {min} AND {max}");

    private static Check Duration(bool optional) =>
        new($"Duration 1 s to {Domain.Exercises.MetricLimits.MaxDuration}, whole seconds",
            $"{(optional ? "\"Duration\" IS NULL OR " : "\"Duration\" IS NOT NULL AND ")}(\"Duration\" BETWEEN {TicksPerSecond} AND {MaxDurationTicks} AND \"Duration\" % {TicksPerSecond} = 0)");

    private static Check Distance() =>
        new("Distance over 0 to 100000, 2 decimal places",
            $"\"Distance\" IS NULL OR (\"Distance\" > 0 AND \"Distance\" <= 100000 AND {TwoPlaces("Distance")})");

    private static string TwoPlaces(string column) => $"ROUND(\"{column}\", 2) = \"{column}\"";

    /// <summary>A CHECK rule of a table.</summary>
    /// <param name="Name">What the rule allows, for the log.</param>
    /// <param name="Sql">The condition every row meets.</param>
    public sealed record Check(string Name, string Sql);

    /// <summary>A table with its columns, CHECK rules and indexes.</summary>
    /// <param name="Name">Table name.</param>
    /// <param name="Columns">Column definitions.</param>
    /// <param name="ColumnNames">Column names, in order.</param>
    /// <param name="Checks">CHECK rules.</param>
    /// <param name="Indexes">CREATE INDEX statements; <c>{0}</c> takes <c>IF NOT EXISTS</c> or nothing.</param>
    public sealed record Table(string Name, string Columns, IReadOnlyList<string> ColumnNames, IReadOnlyList<Check> Checks, IReadOnlyList<string> Indexes)
    {
        /// <summary>The CREATE TABLE statement, under <paramref name="tableName"/>.</summary>
        public string CreateSql(string tableName, bool ifNotExists) =>
            $"CREATE TABLE {(ifNotExists ? "IF NOT EXISTS " : "")}\"{tableName}\" (\n{Columns},\n"
            + string.Join(",\n", Checks.Select(c => $"CHECK ({c.Sql})"))
            + "\n)";
    }
}
