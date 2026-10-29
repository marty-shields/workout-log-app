using Microsoft.Extensions.Logging;
using SQLite;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Data;

/// <summary>
/// Schema step 4, in order: fails on a cardio exercise twice in a workout; merges a strength exercise that is twice in a workout into its first block;
/// sets blank strength weights to 0; blanks cardio floors, level, calories and rate of 0, and the rate of exercises with worked-out speed;
/// fails on any row that breaks a rule of <see cref="WorkoutTables"/>, a carry set without duration, or a cardio entry without distance where the exercise has distance;
/// then rebuilds the four workout tables with their CHECK rules and indexes, which drops the Pace column, keeping each table's highest id used.
/// </summary>
internal sealed partial class ValidationRulesSchema(IExerciseCatalog catalog, ILogger<ValidationRulesSchema> logger) : ISchemaUpgrade
{
    /// <inheritdoc />
    public int Version => 4;

    /// <inheritdoc />
    /// <remarks>Logs each broken rule with its row count, then throws <see cref="InvalidOperationException"/>.</remarks>
    public void Apply(SQLiteConnection db)
    {
        var repeatedCardio = db.ExecuteScalar<int>($"""
            SELECT COUNT(*) FROM (
                SELECT 1 FROM WorkoutExercise WHERE ExerciseId IN ({Ids(e => e.Type == ExerciseType.Cardio)})
                GROUP BY WorkoutId, ExerciseId HAVING COUNT(*) > 1)
            """);
        if (repeatedCardio > 0)
        {
            Fail([("cardio exercise once per workout", repeatedCardio)]);
        }

        var merged = MergeRepeatedStrength(db);
        var weights = db.Execute("UPDATE StrengthSet SET WeightKg = 0 WHERE WeightKg IS NULL");
        var blanked = db.Execute("""
            UPDATE CardioEntry
            SET Floors = NULLIF(Floors, 0), Level = NULLIF(Level, 0), Calories = NULLIF(Calories, 0), Rate = NULLIF(Rate, 0)
            WHERE Floors = 0 OR Level = 0 OR Calories = 0 OR Rate = 0
            """);
        var rates = db.Execute($"""
            UPDATE CardioEntry SET Rate = NULL
            WHERE Rate IS NOT NULL AND WorkoutExerciseId IN (SELECT Id FROM WorkoutExercise WHERE ExerciseId IN ({Ids(WorkedOut.HasSpeed)}))
            """);

        var broken = BrokenRules(db);
        if (broken.Count > 0)
        {
            Fail(broken);
        }

        foreach (var table in WorkoutTables.All)
        {
            Rebuild(db, table);
        }

        Log.Applied(logger, merged, weights, blanked, rates);
    }

    /// <summary>Moves the sets of each later block of a repeated exercise to its first block, in block order; renumbers sets and exercises from 0.</summary>
    /// <returns>Blocks merged away.</returns>
    private static int MergeRepeatedStrength(SQLiteConnection db)
    {
        var repeated = db.Query<BlockRow>("""
            SELECT we.Id, we.WorkoutId, we.ExerciseId FROM WorkoutExercise we
            WHERE (SELECT COUNT(*) FROM WorkoutExercise o WHERE o.WorkoutId = we.WorkoutId AND o.ExerciseId = we.ExerciseId) > 1
            ORDER BY we.WorkoutId, we.Position, we.Id
            """);
        foreach (var blocks in repeated.GroupBy(b => (b.WorkoutId, b.ExerciseId)))
        {
            var first = blocks.First().Id;
            var sets = blocks.SelectMany(b => db.Query<IdRow>("SELECT Id FROM StrengthSet WHERE WorkoutExerciseId = ? ORDER BY Position, Id", b.Id)).ToList();
            for (var i = 0; i < sets.Count; i++)
            {
                db.Execute("UPDATE StrengthSet SET WorkoutExerciseId = ?, Position = ? WHERE Id = ?", first, i, sets[i].Id);
            }

            foreach (var later in blocks.Skip(1))
            {
                db.Execute("DELETE FROM WorkoutExercise WHERE Id = ?", later.Id);
            }
        }

        foreach (var workoutId in repeated.Select(b => b.WorkoutId).Distinct())
        {
            var exercises = db.Query<IdRow>("SELECT Id FROM WorkoutExercise WHERE WorkoutId = ? ORDER BY Position, Id", workoutId);
            for (var i = 0; i < exercises.Count; i++)
            {
                db.Execute("UPDATE WorkoutExercise SET Position = ? WHERE Id = ?", i, exercises[i].Id);
            }
        }

        return repeated.Count - repeated.Select(b => (b.WorkoutId, b.ExerciseId)).Distinct().Count();
    }

    /// <summary>Each rule some rows break, with how many.</summary>
    private List<(string Rule, int Rows)> BrokenRules(SQLiteConnection db)
    {
        var counts = WorkoutTables.All
            .SelectMany(t => t.Checks.Select(c => ($"{t.Name}: {c.Name}", $"SELECT COUNT(*) FROM \"{t.Name}\" WHERE NOT ({c.Sql})")))
            .Append(("StrengthSet: carry duration required", $"""
                SELECT COUNT(*) FROM StrengthSet WHERE Duration IS NULL
                AND WorkoutExerciseId IN (SELECT Id FROM WorkoutExercise WHERE ExerciseId IN ({Ids(e => e.Category == ExerciseCategory.Carry)}))
                """))
            .Append(("CardioEntry: distance required", $"""
                SELECT COUNT(*) FROM CardioEntry WHERE Distance IS NULL
                AND WorkoutExerciseId IN (SELECT Id FROM WorkoutExercise WHERE ExerciseId IN ({Ids(e => e.Type == ExerciseType.Cardio && e.DistanceUnit is not null)}))
                """));
        return counts.Select(c => (c.Item1, db.ExecuteScalar<int>(c.Item2))).Where(c => c.Item2 > 0).ToList();
    }

    private static void Rebuild(SQLiteConnection db, WorkoutTables.Table table)
    {
        var columns = string.Join(", ", table.ColumnNames.Select(c => $"\"{c}\""));
        var rebuilt = $"{table.Name}_rebuilt";
        var sequence = db.ExecuteScalar<long?>("SELECT seq FROM sqlite_sequence WHERE name = ?", table.Name);
        db.Execute(table.CreateSql(rebuilt, ifNotExists: false));
        db.Execute($"INSERT INTO \"{rebuilt}\" ({columns}) SELECT {columns} FROM \"{table.Name}\"");
        db.Execute($"DROP TABLE \"{table.Name}\"");
        db.Execute($"ALTER TABLE \"{rebuilt}\" RENAME TO \"{table.Name}\"");
        if (sequence is { } highest)
        {
            db.Execute("DELETE FROM sqlite_sequence WHERE name = ?", table.Name);
            db.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES (?, ?)", table.Name, highest);
        }
        foreach (var index in table.Indexes)
        {
            db.Execute(string.Format(index, ""));
        }
    }

    private void Fail(List<(string Rule, int Rows)> broken)
    {
        foreach (var (rule, rows) in broken)
        {
            Log.RuleBroken(logger, rule, rows);
        }

        throw new InvalidOperationException($"Schema step 4 failed: {broken.Count} rules broken");
    }

    /// <summary>Ids of the exercises that match, as a SQL list; <c>NULL</c> when none.</summary>
    private string Ids(Func<ExerciseDefinition, bool> match) =>
        catalog.All.Where(match).Select(e => e.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)).DefaultIfEmpty("NULL")
            .Aggregate((a, b) => $"{a}, {b}");

    private sealed class BlockRow
    {
        public int Id { get; set; }

        public int WorkoutId { get; set; }

        public int ExerciseId { get; set; }
    }

    private sealed class IdRow
    {
        public int Id { get; set; }
    }

    private static partial class Log
    {
        [LoggerMessage((int)CoreEvent.ValidationRulesApplied, LogLevel.Information,
            "Schema step 4: {Merged} repeated blocks merged, {Weights} blank weights set to 0, {Blanked} cardio entries with 0 values blanked, {Rates} rates cleared")]
        public static partial void Applied(ILogger logger, int merged, int weights, int blanked, int rates);

        [LoggerMessage((int)CoreEvent.ValidationRuleBroken, LogLevel.Warning, "Schema step 4: {Rows} rows break the rule {Rule}")]
        public static partial void RuleBroken(ILogger logger, string rule, int rows);
    }
}
