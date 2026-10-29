using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Data;

public class ValidationRulesSchemaTests
{
    private const int Squat = 6;
    private const int Bench = 8;
    private const int FarmersWalk = 187;
    private const int Treadmill = 300;
    private const int Rowing = 305;
    private const int StairClimber = 309;
    private const int Run = 311;
    private const int OutdoorCycle = 313;
    private const long Minutes30 = 18_000_000_000;

    private static readonly ExerciseCatalog Catalog = new();

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private FakeLogger<ValidationRulesSchema> logger = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        logger = new FakeLogger<ValidationRulesSchema>();
        await CreateVersion3Async();
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Squat, Bench, FarmersWalk, Treadmill, Rowing, StairClimber, Run, OutdoorCycle }.Select(id => Catalog.All.Single(e => e.Id == id).Name),
            Is.EqualTo(new[] { "Barbell Back Squat", "Barbell Bench Press", "Farmer's Walk", "Treadmill", "Rowing Machine", "Stair Climber", "Outdoor Run", "Outdoor Cycle" }));

    [Test]
    public async Task Initialize_ValidDatabase_IsVersion4WithEveryRowAndNoPace()
    {
        await AddWorkoutAsync(1, (Squat, Sets("(5, 60, NULL, NULL)")), (Run, Entry($"({Minutes30}, 5.2, NULL, NULL, NULL, 300, NULL, 3460000000)")));

        await InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await Version(), Is.EqualTo(4));
            Assert.That(await Count("Workout"), Is.EqualTo(1));
            Assert.That(await Count("WorkoutExercise"), Is.EqualTo(2));
            Assert.That(await Count("StrengthSet"), Is.EqualTo(1));
            Assert.That(await Count("CardioEntry"), Is.EqualTo(1));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT name FROM pragma_table_info('CardioEntry')"), Has.No.Member("Pace"));
            Assert.That(await connection.ExecuteScalarAsync<double>("SELECT Distance FROM CardioEntry"), Is.EqualTo(5.2));
        });
    }

    [Test]
    public async Task Initialize_RebuildsEveryIndexWithTheUniqueExercisePerWorkout()
    {
        await InitializeAsync();

        Assert.That(await connection.QueryScalarsAsync<string>("SELECT name FROM sqlite_master WHERE type = 'index' AND name NOT LIKE 'sqlite_%' ORDER BY name"),
            Is.EqualTo(new[]
            {
                "CardioEntry_WorkoutExerciseId",
                "StrengthSet_WorkoutExerciseId",
                "WorkoutExercise_WorkoutId",
                "WorkoutExercise_WorkoutId_ExerciseId",
                "Workout_StartedAt",
            }));
    }

    [TestCase("INSERT INTO Workout (StartedAt, FinishedAt) VALUES (500, 500)", TestName = "Finish at the start")]
    [TestCase("INSERT INTO Workout (StartedAt, FinishedAt) VALUES (100, 900)", TestName = "Repeated start")]
    [TestCase("INSERT INTO WorkoutExercise (WorkoutId, ExerciseId, Position) VALUES (1, 6, 0)", TestName = "Repeated exercise")]
    [TestCase("INSERT INTO StrengthSet (WorkoutExerciseId, Position, Reps) VALUES (1, 0, 5)", TestName = "No weight")]
    [TestCase("INSERT INTO CardioEntry (WorkoutExerciseId, Duration, Rate) VALUES (9, 6000000000, 0)", TestName = "Zero rate")]
    public async Task Initialize_TablesHaveTheirRules(string insert)
    {
        await AddWorkoutAsync(1, (Squat, Sets("(5, 60, NULL, NULL)")));
        await InitializeAsync();

        Assert.That(() => connection.ExecuteAsync(insert), Throws.InstanceOf<SQLiteException>());
    }

    [Test]
    public async Task Initialize_StrengthExerciseTwice_MergesIntoTheFirstBlockInOrder()
    {
        await AddWorkoutAsync(1,
            (Squat, Sets("(5, 60, NULL, NULL)", "(5, 65, NULL, NULL)")),
            (Bench, Sets("(8, 40, NULL, NULL)")),
            (Squat, Sets("(3, 70, NULL, NULL)")));

        await InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await connection.QueryScalarsAsync<int>("SELECT ExerciseId FROM WorkoutExercise ORDER BY Position"), Is.EqualTo(new[] { Squat, Bench }));
            Assert.That(await connection.QueryScalarsAsync<int>("SELECT Position FROM WorkoutExercise ORDER BY Position"), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(await connection.QueryScalarsAsync<double>(
                "SELECT s.WeightKg FROM StrengthSet s JOIN WorkoutExercise e ON e.Id = s.WorkoutExerciseId WHERE e.ExerciseId = 6 ORDER BY s.Position"),
                Is.EqualTo(new[] { 60d, 65, 70 }));
            Assert.That(await connection.QueryScalarsAsync<int>(
                "SELECT s.Position FROM StrengthSet s JOIN WorkoutExercise e ON e.Id = s.WorkoutExerciseId WHERE e.ExerciseId = 6 ORDER BY s.Position"),
                Is.EqualTo(new[] { 0, 1, 2 }));
        });
    }

    [Test]
    public async Task Initialize_StrengthExerciseTwice_MergesIntoTheLowestPositionBlockByPosition()
    {
        await AddWorkoutAsync(1,
            (Squat, Sets("(5, 60, NULL, NULL)", "(5, 65, NULL, NULL)")),
            (Squat, Sets("(3, 70, NULL, NULL)", "(2, 75, NULL, NULL)")),
            (Squat, Sets("(1, 80, NULL, NULL)")));
        await connection.ExecuteAsync("UPDATE WorkoutExercise SET Position = 2 - Position");
        await connection.ExecuteAsync("UPDATE StrengthSet SET Position = 1 - Position WHERE Reps IN (3, 2)");

        await InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await Count("WorkoutExercise"), Is.EqualTo(1));
            Assert.That(await connection.QueryScalarsAsync<int>("SELECT Reps FROM StrengthSet ORDER BY Position"), Is.EqualTo(new[] { 1, 2, 3, 5, 5 }));
            Assert.That(LoggedEvents.Of(logger).Single().Message, Does.StartWith("Schema step 4: 2 repeated blocks merged"));
        });
    }

    [Test]
    public async Task Initialize_KeepsEachTablesHighestIdUsed()
    {
        await AddWorkoutAsync(1, (Squat, Sets("(5, 60, NULL, NULL)")));
        await AddWorkoutAtAsync(300, 1, (Bench, Sets("(5, 60, NULL, NULL)")));
        await connection.ExecuteAsync("DELETE FROM StrengthSet WHERE Id = 2");
        await connection.ExecuteAsync("DELETE FROM WorkoutExercise WHERE Id = 2");
        await connection.ExecuteAsync("DELETE FROM Workout WHERE Id = 2");

        await InitializeAsync();
        await AddWorkoutAtAsync(500, 1, (Squat, Sets("(5, 60, NULL, NULL)")));

        Assert.Multiple(async () =>
        {
            Assert.That(await connection.ExecuteScalarAsync<int>("SELECT MAX(Id) FROM Workout"), Is.EqualTo(3));
            Assert.That(await connection.ExecuteScalarAsync<int>("SELECT MAX(Id) FROM WorkoutExercise"), Is.EqualTo(3));
            Assert.That(await connection.ExecuteScalarAsync<int>("SELECT MAX(Id) FROM StrengthSet"), Is.EqualTo(3));
        });
    }

    [Test]
    public async Task Initialize_BrokenRule_RollsBackEveryChangeAndLogsNoCounts()
    {
        await AddWorkoutAsync(1,
            (Squat, Sets("(5, NULL, NULL, NULL)")),
            (Squat, Sets("(3, 70, NULL, NULL)")),
            (StairClimber, Entry($"({Minutes30}, NULL, NULL, NULL, NULL, 0, 10, NULL)")),
            (Run, Entry($"({Minutes30}, NULL, NULL, NULL, NULL, NULL, NULL, NULL)")));
        var before = await Snapshot();

        Assert.That(() => InitializeAsync(), Throws.Exception);

        Assert.Multiple(async () =>
        {
            Assert.That(await Snapshot(), Is.EqualTo(before));
            Assert.That(LoggedEvents.Of(logger).Select(e => e.Id), Has.No.Member(CoreEvent.ValidationRulesApplied));
        });
    }

    [Test]
    public async Task Initialize_BlankStrengthWeights_AreZero()
    {
        await AddWorkoutAsync(1, (Squat, Sets("(5, NULL, NULL, NULL)", "(5, 60, NULL, NULL)")), (FarmersWalk, Sets("(NULL, NULL, 450000000, 20)")));

        await InitializeAsync();

        Assert.That(await connection.QueryScalarsAsync<double>("SELECT WeightKg FROM StrengthSet ORDER BY Id"), Is.EqualTo(new[] { 0d, 60, 0 }));
    }

    [Test]
    public async Task Initialize_CardioZeros_AreBlankAndIncline0IsKept()
    {
        await AddWorkoutAsync(1, (StairClimber, Entry($"({Minutes30}, NULL, 0, 0, 0, 0, 0, NULL)")));

        await InitializeAsync();

        Assert.That(await connection.QueryAsync<EntryRow>("SELECT Floors, Level, Incline, Calories, Rate FROM CardioEntry"),
            Is.EqualTo(new[] { new EntryRow { Incline = 0 } }).UsingPropertiesComparer());
    }

    [Test]
    public async Task Initialize_RateOfExercisesWithWorkedOutSpeed_IsBlank()
    {
        await AddWorkoutAsync(1,
            (Treadmill, Entry($"({Minutes30}, 5, NULL, NULL, NULL, NULL, 10, NULL)")),
            (OutdoorCycle, Entry($"({Minutes30}, 15, NULL, NULL, NULL, NULL, 30, NULL)")),
            (Rowing, Entry($"({Minutes30}, 6000, NULL, 5, NULL, NULL, 24, NULL)")));

        await InitializeAsync();

        Assert.That(await connection.QueryScalarsAsync<double?>("SELECT Rate FROM CardioEntry ORDER BY Id"), Is.EqualTo(new double?[] { null, null, 24 }));
    }

    [Test]
    public async Task Initialize_Changes_LogTheirCounts()
    {
        await AddWorkoutAsync(1,
            (Squat, Sets("(5, NULL, NULL, NULL)")),
            (Bench, Sets("(8, NULL, NULL, NULL)")),
            (Squat, Sets("(3, 70, NULL, NULL)")),
            (Treadmill, Entry($"({Minutes30}, 5, NULL, NULL, NULL, 0, 10, NULL)")));

        await InitializeAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.ValidationRulesApplied, LogLevel.Information,
                "Schema step 4: 1 repeated blocks merged, 2 blank weights set to 0, 1 cardio entries with 0 values blanked, 1 rates cleared"),
        }));
    }

    [TestCaseSource(nameof(BrokenDatabases))]
    public async Task Initialize_BrokenRule_FailsLogsTheRuleAndChangesNothing(Func<ValidationRulesSchemaTests, Task> arrange, string rule, int rows)
    {
        await arrange(this);
        var before = await Snapshot();

        Assert.That(() => InitializeAsync(), Throws.Exception);

        Assert.Multiple(async () =>
        {
            Assert.That(await Version(), Is.EqualTo(3));
            Assert.That(await Snapshot(), Is.EqualTo(before));
            Assert.That(LoggedEvents.Of(logger), Does.Contain((CoreEvent.ValidationRuleBroken, LogLevel.Warning, $"Schema step 4: {rows} rows break the rule {rule}")));
        });
    }

    private static IEnumerable<TestCaseData> BrokenDatabases()
    {
        TestCaseData Case(string name, string rule, int rows, Func<ValidationRulesSchemaTests, Task> arrange) =>
            new TestCaseData(arrange, rule, rows).SetName($"Initialize_{name}_Fails");

        yield return Case("CardioTwice", "cardio exercise once per workout", 1,
            t => t.AddWorkoutAsync(1, (Run, Entry($"({Minutes30}, 5, NULL, NULL, NULL, NULL, NULL, NULL)")), (Run, Entry($"({Minutes30}, 3, NULL, NULL, NULL, NULL, NULL, NULL)"))));
        yield return Case("CardioWithoutDuration", "CardioEntry: Duration 1 s to 23:59:59, whole seconds", 1,
            t => t.AddWorkoutAsync(1, (Run, Entry("(NULL, 5, NULL, NULL, NULL, NULL, NULL, NULL)"))));
        yield return Case("CarryWithoutDuration", "StrengthSet: carry duration required", 1,
            t => t.AddWorkoutAsync(1, (FarmersWalk, Sets("(NULL, 32, NULL, 20)"))));
        yield return Case("CarryWithZeroDuration", "StrengthSet: Duration 1 s to 23:59:59, whole seconds", 1,
            t => t.AddWorkoutAsync(1, (FarmersWalk, Sets("(NULL, 32, 0, 20)"))));
        yield return Case("CardioWithoutDistance", "CardioEntry: distance required", 1,
            t => t.AddWorkoutAsync(1, (Run, Entry($"({Minutes30}, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))));
        yield return Case("CardioWithZeroDistance", "CardioEntry: Distance over 0 to 100000, 2 decimal places", 1,
            t => t.AddWorkoutAsync(1, (Run, Entry($"({Minutes30}, 0, NULL, NULL, NULL, NULL, NULL, NULL)"))));
        yield return Case("ThreeDecimalPlaces", "StrengthSet: WeightKg 0 to 1000, 2 decimal places", 1,
            t => t.AddWorkoutAsync(1, (Squat, Sets("(5, 22.755, NULL, NULL)"))));
        yield return Case("FinishAtTheStart", "Workout: finish after start", 1,
            t => t.AddWorkoutAsync(0, (Squat, Sets("(5, 60, NULL, NULL)"))));
        yield return Case("ElevenExercises", "WorkoutExercise: Position 0 to 9", 1,
            t => t.AddWorkoutAsync(1, [.. Enumerable.Range(1, 11).Select(i => (i * 10, Sets("(5, 60, NULL, NULL)")))]));
        yield return Case("ElevenSetsAfterTheMerge", "StrengthSet: Position 0 to 9", 1,
            t => t.AddWorkoutAsync(1, (Squat, Sets([.. Enumerable.Repeat("(5, 60, NULL, NULL)", 6)])), (Squat, Sets([.. Enumerable.Repeat("(5, 60, NULL, NULL)", 5)]))));
    }

    private Task InitializeAsync() =>
        new DatabaseInitializer(connection,
            [new WorkoutStartedAtUniqueIndex(), new BlankAssistToZero(Catalog), new ValidationRulesSchema(Catalog, logger)],
            Mock.Of<IExerciseSeeder>(), NullLogger<DatabaseInitializer>.Instance).InitializeAsync();

    /// <summary>The version 3 schema: as sqlite-net made it, with the unique start index and the Pace column.</summary>
    private async Task CreateVersion3Async()
    {
        string[] statements =
        [
            "CREATE TABLE Exercise (Id integer primary key not null, Name varchar, Type integer)",
            "CREATE TABLE Workout (Id integer primary key autoincrement not null, StartedAt bigint, FinishedAt bigint)",
            "CREATE UNIQUE INDEX Workout_StartedAt ON Workout(StartedAt)",
            "CREATE TABLE WorkoutExercise (Id integer primary key autoincrement not null, WorkoutId integer, ExerciseId integer, Position integer)",
            "CREATE INDEX WorkoutExercise_WorkoutId ON WorkoutExercise(WorkoutId)",
            "CREATE TABLE StrengthSet (Id integer primary key autoincrement not null, WorkoutExerciseId integer, Position integer, " +
            "Reps integer, WeightKg float, Duration bigint, Distance float)",
            "CREATE INDEX StrengthSet_WorkoutExerciseId ON StrengthSet(WorkoutExerciseId)",
            "CREATE TABLE CardioEntry (Id integer primary key autoincrement not null, WorkoutExerciseId integer, Duration bigint, " +
            "Distance float, Floors integer, Level integer, Incline float, Calories integer, Rate float, Pace bigint)",
            "CREATE UNIQUE INDEX CardioEntry_WorkoutExerciseId ON CardioEntry(WorkoutExerciseId)",
        ];
        foreach (var statement in statements)
        {
            await connection.ExecuteAsync(statement);
        }

        await connection.ExecuteAsync("PRAGMA user_version = 3");
    }

    /// <summary>Adds a workout at ticks 100 that lasts <paramref name="lengthTicks"/>, with each exercise's sets or entry in order.</summary>
    private Task AddWorkoutAsync(long lengthTicks, params (int ExerciseId, Func<int, string> Rows)[] exercises) =>
        AddWorkoutAtAsync(100, lengthTicks, exercises);

    /// <summary>Adds a workout at <paramref name="startedAt"/> ticks, as <see cref="AddWorkoutAsync"/> does.</summary>
    private async Task AddWorkoutAtAsync(long startedAt, long lengthTicks, params (int ExerciseId, Func<int, string> Rows)[] exercises)
    {
        await connection.ExecuteAsync("INSERT INTO Workout (StartedAt, FinishedAt) VALUES (?, ?)", startedAt, startedAt + lengthTicks);
        var workoutId = await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
        for (var i = 0; i < exercises.Length; i++)
        {
            await connection.ExecuteAsync("INSERT INTO WorkoutExercise (WorkoutId, ExerciseId, Position) VALUES (?, ?, ?)", workoutId, exercises[i].ExerciseId, i);
            await connection.ExecuteAsync(exercises[i].Rows(await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()")));
        }
    }

    /// <summary>Strength set rows, each <c>(Reps, WeightKg, Duration, Distance)</c>.</summary>
    private static Func<int, string> Sets(params string[] sets) => id =>
        "INSERT INTO StrengthSet (WorkoutExerciseId, Position, Reps, WeightKg, Duration, Distance) VALUES " +
        string.Join(", ", sets.Select((s, i) => $"({id}, {i}, {s[1..]}"));

    /// <summary>A cardio entry row, <c>(Duration, Distance, Floors, Level, Incline, Calories, Rate, Pace)</c>.</summary>
    private static Func<int, string> Entry(string entry) => id =>
        $"INSERT INTO CardioEntry (WorkoutExerciseId, Duration, Distance, Floors, Level, Incline, Calories, Rate, Pace) VALUES ({id}, {entry[1..]}";

    private Task<int> Version() => connection.ExecuteScalarAsync<int>("PRAGMA user_version");

    private Task<int> Count(string table) => connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM \"{table}\"");

    private async Task<string> Snapshot() => string.Join("\n",
        await connection.QueryScalarsAsync<string>("SELECT sql FROM sqlite_master WHERE sql IS NOT NULL ORDER BY name"),
        string.Join(";", await connection.QueryScalarsAsync<string>("SELECT ExerciseId || ':' || Position FROM WorkoutExercise ORDER BY Id")),
        string.Join(";", await connection.QueryScalarsAsync<string>(
            "SELECT WorkoutExerciseId || ':' || Position || ':' || IFNULL(WeightKg, 'null') || ':' || IFNULL(Duration, 'null') FROM StrengthSet ORDER BY Id")),
        string.Join(";", await connection.QueryScalarsAsync<string>(
            "SELECT IFNULL(Distance, 'null') || ':' || IFNULL(Calories, 'null') || ':' || IFNULL(Rate, 'null') FROM CardioEntry ORDER BY Id")));

    private sealed class EntryRow
    {
        public int? Floors { get; set; }

        public int? Level { get; set; }

        public double? Incline { get; set; }

        public int? Calories { get; set; }

        public double? Rate { get; set; }
    }
}
