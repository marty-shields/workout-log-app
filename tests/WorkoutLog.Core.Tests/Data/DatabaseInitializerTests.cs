using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.Core.Tests.Data;

public class DatabaseInitializerTests
{
    private readonly List<string> calls = [];
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private Mock<IExerciseSeeder> seeder = null!;
    private FakeLogger<DatabaseInitializer> logger = null!;

    [SetUp]
    public void SetUp()
    {
        calls.Clear();
        logger = new FakeLogger<DatabaseInitializer>();
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        seeder = new Mock<IExerciseSeeder>();
        seeder.Setup(s => s.SeedAsync()).Callback(() => calls.Add("seed")).Returns(Task.CompletedTask);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task InitializeAsync_NewDatabase_CreatesTheTablesInTheDataModel()
    {
        await Initializer(DatabaseInitializer.CurrentVersion).InitializeAsync();

        Assert.That(await Columns(), Is.EquivalentTo(new[]
        {
            ("CardioEntry", "Id", "integer"),
            ("CardioEntry", "WorkoutExerciseId", "integer"),
            ("CardioEntry", "Duration", "bigint"),
            ("CardioEntry", "Distance", "float"),
            ("CardioEntry", "Floors", "integer"),
            ("CardioEntry", "Level", "integer"),
            ("CardioEntry", "Incline", "float"),
            ("CardioEntry", "Calories", "integer"),
            ("CardioEntry", "Rate", "float"),
            ("CardioEntry", "Pace", "bigint"),
            ("Exercise", "Id", "integer"),
            ("Exercise", "Name", "varchar"),
            ("Exercise", "Type", "integer"),
            ("StrengthSet", "Id", "integer"),
            ("StrengthSet", "WorkoutExerciseId", "integer"),
            ("StrengthSet", "Position", "integer"),
            ("StrengthSet", "Reps", "integer"),
            ("StrengthSet", "WeightKg", "float"),
            ("StrengthSet", "Duration", "bigint"),
            ("StrengthSet", "Distance", "float"),
            ("Workout", "Id", "integer"),
            ("Workout", "StartedAt", "bigint"),
            ("Workout", "FinishedAt", "bigint"),
            ("WorkoutExercise", "Id", "integer"),
            ("WorkoutExercise", "WorkoutId", "integer"),
            ("WorkoutExercise", "ExerciseId", "integer"),
            ("WorkoutExercise", "Position", "integer"),
        }));
        Assert.That(await Indexes(), Is.EquivalentTo(new[]
        {
            ("CardioEntry", "WorkoutExerciseId", true),
            ("StrengthSet", "WorkoutExerciseId", false),
            ("Workout", "StartedAt", false),
            ("WorkoutExercise", "WorkoutId", false),
        }));
    }

    [Test]
    public async Task InitializeAsync_NewDatabase_SetsTheCurrentVersionWithoutRunningSteps()
    {
        await Initializer(2, Step(2)).InitializeAsync();

        Assert.That(await Version(), Is.EqualTo(2));
        Assert.That(calls, Is.EqualTo(new[] { "seed" }));
    }

    [Test]
    public async Task InitializeAsync_OlderDatabase_RunsTheMissingStepsInOrderThenSeeds()
    {
        await SetVersion(1);

        await Initializer(3, Step(3), Step(2)).InitializeAsync();

        Assert.That(await Version(), Is.EqualTo(3));
        Assert.That(calls, Is.EqualTo(new[] { "step 2", "step 3", "seed" }));
    }

    [Test]
    public async Task InitializeAsync_OlderDatabase_RunsOnlyTheStepsAfterTheStoredVersion()
    {
        await SetVersion(2);

        await Initializer(3, Step(2), Step(3)).InitializeAsync();

        Assert.That(await Version(), Is.EqualTo(3));
        Assert.That(calls, Is.EqualTo(new[] { "step 3", "seed" }));
    }

    [Test]
    public async Task InitializeAsync_OlderDatabase_RunsTheStepsBeforeCreatingTables()
    {
        await SetVersion(1);
        List<string> tablesSeenByStep = [];

        await Initializer(2, Step(2, db => tablesSeenByStep = TableNames(db))).InitializeAsync();

        Assert.That(tablesSeenByStep, Is.Empty);
        Assert.That(await Tables(), Has.Member("Workout"));
    }

    [Test]
    public async Task InitializeAsync_CurrentDatabaseWithoutPace_AddsThePaceColumnAndKeepsTheEntries()
    {
        await connection.ExecuteAsync(
            "CREATE TABLE CardioEntry (Id integer primary key autoincrement not null, WorkoutExerciseId integer not null, " +
            "Duration bigint not null, Distance float, Floors integer, Level integer, Incline float, Calories integer, Rate float)");
        await connection.ExecuteAsync("INSERT INTO CardioEntry (WorkoutExerciseId, Duration) VALUES (1, 6000000000)");
        await SetVersion(DatabaseInitializer.CurrentVersion);

        await Initializer(DatabaseInitializer.CurrentVersion).InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await Columns(), Does.Contain(("CardioEntry", "Pace", "bigint")));
            Assert.That(await connection.Table<CardioEntryRow>().CountAsync(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task InitializeAsync_CurrentDatabaseWithoutStartedAtIndex_AddsTheIndexAndKeepsTheWorkouts()
    {
        await connection.ExecuteAsync(
            "CREATE TABLE Workout (Id integer primary key autoincrement not null, StartedAt bigint not null, FinishedAt bigint not null)");
        await connection.ExecuteAsync("INSERT INTO Workout (StartedAt, FinishedAt) VALUES (1, 2)");
        await SetVersion(DatabaseInitializer.CurrentVersion);

        await Initializer(DatabaseInitializer.CurrentVersion).InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await Indexes(), Does.Contain(("Workout", "StartedAt", false)));
            Assert.That(await connection.Table<WorkoutRow>().CountAsync(), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task InitializeAsync_CurrentDatabase_RunsNoSteps()
    {
        await SetVersion(2);

        await Initializer(2, Step(2)).InitializeAsync();

        Assert.That(await Version(), Is.EqualTo(2));
        Assert.That(calls, Is.EqualTo(new[] { "seed" }));
    }

    [Test]
    public async Task InitializeAsync_NewDatabase_LogsTheVersionsAndTheNewDatabase()
    {
        await Initializer(2, Step(2)).InitializeAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.SchemaVersion, LogLevel.Information, "Database schema version 0, app schema version 2"),
            (CoreEvent.DatabaseCreated, LogLevel.Information, "New database created at schema version 2"),
        }));
    }

    [Test]
    public async Task InitializeAsync_OlderDatabase_LogsTheVersionsAndEachStep()
    {
        await SetVersion(1);

        await Initializer(3, Step(2), Step(3)).InitializeAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.SchemaVersion, LogLevel.Information, "Database schema version 1, app schema version 3"),
            (CoreEvent.SchemaStepApplied, LogLevel.Information, "Schema upgrade step 2 applied"),
            (CoreEvent.SchemaStepApplied, LogLevel.Information, "Schema upgrade step 3 applied"),
        }));
    }

    [Test]
    public async Task InitializeAsync_NewerDatabase_ThrowsAndChangesNothing()
    {
        await SetVersion(3);

        Assert.That(() => Initializer(2, Step(2)).InitializeAsync(), Throws.InvalidOperationException);

        Assert.That(await Version(), Is.EqualTo(3));
        Assert.That(await Tables(), Is.Empty);
        Assert.That(calls, Is.Empty);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.SchemaVersion, LogLevel.Information, "Database schema version 3, app schema version 2"),
        }));
    }

    [Test]
    public async Task InitializeAsync_MissingStep_ThrowsAndChangesNothing()
    {
        await SetVersion(1);

        Assert.That(() => Initializer(3, Step(3)).InitializeAsync(), Throws.InvalidOperationException);

        Assert.That(await Version(), Is.EqualTo(1));
        Assert.That(await Tables(), Is.Empty);
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public async Task InitializeAsync_DuplicateStep_ThrowsAndChangesNothing()
    {
        await SetVersion(1);

        Assert.That(() => Initializer(2, Step(2), Step(2)).InitializeAsync(), Throws.InvalidOperationException);

        Assert.That(await Version(), Is.EqualTo(1));
        Assert.That(await Tables(), Is.Empty);
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public async Task InitializeAsync_StepAboveTheCurrentVersion_ThrowsAndChangesNothing()
    {
        await SetVersion(1);

        Assert.That(() => Initializer(2, Step(2), Step(3)).InitializeAsync(), Throws.InvalidOperationException);

        Assert.That(await Version(), Is.EqualTo(1));
        Assert.That(await Tables(), Is.Empty);
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public async Task InitializeAsync_NewDatabaseWithAMissingStep_ThrowsAndChangesNothing()
    {
        Assert.That(() => Initializer(3, Step(3)).InitializeAsync(), Throws.InvalidOperationException);

        Assert.That(await Version(), Is.Zero);
        Assert.That(await Tables(), Is.Empty);
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public async Task InitializeAsync_FailingStep_RollsBackThatStepOnlyAndCreatesNoTables()
    {
        await SetVersion(1);
        var failing = Step(3, db =>
        {
            db.Execute("INSERT INTO Marker (Id) VALUES (1)");
            throw new InvalidOperationException("Step failed");
        });

        Assert.That(() => Initializer(3, Step(2, db => db.Execute("CREATE TABLE Marker (Id INTEGER)")), failing).InitializeAsync(),
            Throws.InvalidOperationException);

        Assert.That(await Version(), Is.EqualTo(2));
        Assert.That(await Tables(), Is.EqualTo(new[] { "Marker" }));
        Assert.That(await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Marker"), Is.Zero);
        Assert.That(calls, Is.EqualTo(new[] { "step 2", "step 3" }));
    }

    [Test]
    public async Task InitializeAsync_Twice_SeedsEveryExerciseOnce()
    {
        var catalog = new ExerciseCatalog();
        var initializer = new DatabaseInitializer(
            connection, [], new ExerciseSeeder(connection, catalog, NullLogger<ExerciseSeeder>.Instance), logger);

        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        Assert.That(await Version(), Is.EqualTo(DatabaseInitializer.CurrentVersion));
        Assert.That(await connection.Table<Exercise>().CountAsync(), Is.EqualTo(catalog.All.Count));
    }

    private DatabaseInitializer Initializer(int currentVersion, params ISchemaUpgrade[] steps) =>
        new(connection, steps, seeder.Object, logger, currentVersion);

    private FakeStep Step(int version, Action<SQLiteConnection>? apply = null) => new(version, db =>
    {
        calls.Add($"step {version}");
        apply?.Invoke(db);
    });

    private Task SetVersion(int version) => connection.ExecuteAsync($"PRAGMA user_version = {version}");

    private Task<int> Version() => connection.ExecuteScalarAsync<int>("PRAGMA user_version");

    private Task<List<string>> Tables() =>
        connection.QueryScalarsAsync<string>("SELECT name FROM sqlite_master WHERE type = 'table'");

    private static List<string> TableNames(SQLiteConnection db) =>
        db.QueryScalars<string>("SELECT name FROM sqlite_master WHERE type = 'table'");

    private async Task<List<(string Table, string Column, string Type)>> Columns() =>
        (await connection.QueryAsync<SchemaRow>(
            "SELECT m.name AS TableName, p.name AS ColumnName, lower(p.type) AS ColumnType " +
            "FROM sqlite_master m, pragma_table_info(m.name) p WHERE m.type = 'table' AND m.name NOT LIKE 'sqlite_%'"))
        .Select(r => (r.TableName, r.ColumnName, r.ColumnType)).ToList();

    private async Task<List<(string Table, string Column, bool Unique)>> Indexes() =>
        (await connection.QueryAsync<SchemaRow>(
            "SELECT m.name AS TableName, i.name AS ColumnName, l.\"unique\" AS IsUnique " +
            "FROM sqlite_master m, pragma_index_list(m.name) l, pragma_index_info(l.name) i WHERE m.type = 'table'"))
        .Select(r => (r.TableName, r.ColumnName, r.IsUnique)).ToList();

    private sealed class FakeStep(int version, Action<SQLiteConnection> apply) : ISchemaUpgrade
    {
        public int Version => version;

        public void Apply(SQLiteConnection db) => apply(db);
    }

    private sealed class SchemaRow
    {
        public string TableName { get; set; } = "";

        public string ColumnName { get; set; } = "";

        public string ColumnType { get; set; } = "";

        public bool IsUnique { get; set; }
    }
}
