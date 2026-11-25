using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Data;

public class UnfinishedSchemaTests
{
    private static readonly ExerciseCatalog Catalog = new();
    private static readonly string[] UnfinishedTableNames = ["UnfinishedEntry", "UnfinishedExercise", "UnfinishedWorkout"];

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await CreateVersion5Async();
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task Initialize_Version5Database_AddsTheUnfinishedTablesAndKeepsEveryRow()
    {
        await InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await connection.ExecuteScalarAsync<int>("PRAGMA user_version"), Is.EqualTo(6));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE 'Unfinished%' ORDER BY name"),
                Is.EqualTo(UnfinishedTableNames));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT StartedAt || ':' || FinishedAt || ':' || Note FROM Workout"), Is.EqualTo(new[] { "100:200:felt good" }));
            Assert.That(await connection.ExecuteScalarAsync<double>("SELECT WeightKg FROM StrengthSet"), Is.EqualTo(60));
        });
    }

    [Test]
    public async Task Initialize_Version5Database_StartsWithNoUnfinishedWorkoutThatCanBeStored()
    {
        await InitializeAsync();
        var repository = new UnfinishedWorkoutRepository(connection);
        Assert.That(await repository.GetAsync(), Is.Null);

        await repository.ReplaceAsync(UnfinishedWorkoutRepositoryTests.Typed());

        Assert.That((await repository.GetAsync())!.Exercises, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task Initialize_Twice_KeepsTheUnfinishedWorkout()
    {
        await InitializeAsync();
        await new UnfinishedWorkoutRepository(connection).ReplaceAsync(UnfinishedWorkoutRepositoryTests.Typed());

        await InitializeAsync();

        Assert.That(await new UnfinishedWorkoutRepository(connection).GetAsync(), Is.Not.Null);
    }

    [Test]
    public async Task Apply_Twice_CreatesEachTableOnce()
    {
        await connection.RunInTransactionAsync(db =>
        {
            new UnfinishedSchema().Apply(db);
            new UnfinishedSchema().Apply(db);
        });

        Assert.That(await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name LIKE 'Unfinished%'"), Is.EqualTo(3));
    }

    [Test]
    public async Task Initialize_NewDatabase_CreatesTheUnfinishedTables()
    {
        var path = TempDatabase.NewPath();
        var fresh = new SQLiteAsyncConnection(path);
        try
        {
            await new DatabaseInitializer(fresh, Steps(), Mock.Of<IExerciseSeeder>(), NullLogger<DatabaseInitializer>.Instance).InitializeAsync();

            Assert.That(await fresh.QueryScalarsAsync<string>("SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE 'Unfinished%' ORDER BY name"),
                Is.EqualTo(UnfinishedTableNames));
        }
        finally
        {
            await fresh.CloseAsync();
            File.Delete(path);
        }
    }

    [Test]
    public async Task Initialize_UnfinishedWorkoutIsNotAWorkout()
    {
        await InitializeAsync();
        await new UnfinishedWorkoutRepository(connection).ReplaceAsync(UnfinishedWorkoutRepositoryTests.Typed());
        var repository = new WorkoutRepository(connection, new WorkoutWriterFactory());

        Assert.Multiple(async () =>
        {
            Assert.That(await repository.GetAllAsync(), Has.Count.EqualTo(1));
            Assert.That(await repository.ListStartsAsync(), Has.Count.EqualTo(1));
            Assert.That(await repository.GetSessionsAsync(1), Is.Empty);
        });
    }

    private Task InitializeAsync() =>
        new DatabaseInitializer(connection, Steps(), Mock.Of<IExerciseSeeder>(), NullLogger<DatabaseInitializer>.Instance).InitializeAsync();

    private static ISchemaUpgrade[] Steps() =>
    [
        new WorkoutStartedAtUniqueIndex(), new BlankAssistToZero(Catalog), new ValidationRulesSchema(Catalog, NullLogger<ValidationRulesSchema>.Instance),
        new NotesSchema(), new UnfinishedSchema(),
    ];

    /// <summary>The version 5 schema with one workout of a strength set and a note.</summary>
    private async Task CreateVersion5Async()
    {
        await connection.ExecuteAsync("CREATE TABLE Exercise (Id integer primary key not null, Name varchar, Type integer)");
        await connection.RunInTransactionAsync(WorkoutTables.CreateMissing);
        string[] rows =
        [
            "INSERT INTO Workout (StartedAt, FinishedAt, Note) VALUES (100, 200, 'felt good')",
            "INSERT INTO WorkoutExercise (WorkoutId, ExerciseId, Position) VALUES (1, 6, 0)",
            "INSERT INTO StrengthSet (WorkoutExerciseId, Position, Reps, WeightKg) VALUES (1, 0, 5, 60)",
            "PRAGMA user_version = 5",
        ];
        foreach (var row in rows)
        {
            await connection.ExecuteAsync(row);
        }
    }
}
