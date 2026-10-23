using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Data;

public class ExerciseSeederTests
{
    private readonly ExerciseCatalog catalog = new();
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private FakeLogger<ExerciseSeeder> logger = null!;
    private ExerciseSeeder seeder = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.CreateTableAsync<Exercise>();
        logger = new FakeLogger<ExerciseSeeder>();
        seeder = new ExerciseSeeder(connection, catalog, logger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task SeedAsync_EmptyDatabase_InsertsEveryExercise()
    {
        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected()));
    }

    [Test]
    public async Task SeedAsync_Twice_AddsNoDuplicates()
    {
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected()));
    }

    [Test]
    public async Task SeedAsync_ChangedNameAndType_UpdatesThemById()
    {
        var first = catalog.All[0];
        await connection.InsertAsync(new Exercise { Id = first.Id, Name = "Old name", Type = ExerciseType.Cardio });

        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected()));
    }

    [Test]
    public async Task SeedAsync_RowNotInTheFile_IsKept()
    {
        var extra = new Exercise { Id = catalog.All.Count + 1, Name = "Retired", Type = ExerciseType.Strength };
        await connection.InsertAsync(extra);

        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected().Append((extra.Id, extra.Name, extra.Type))));
    }

    [Test]
    public async Task SeedAsync_EmptyDatabase_LogsEveryExerciseAdded()
    {
        await seeder.SeedAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (1101, LogLevel.Information, $"Exercise list seeded: {catalog.All.Count} added, 0 updated"),
        }));
    }

    [Test]
    public async Task SeedAsync_ChangedAndMissingRows_LogsTheCounts()
    {
        await seeder.SeedAsync();
        await connection.ExecuteAsync("UPDATE Exercise SET Name = 'Old name' WHERE Id IN (1, 2)");
        await connection.ExecuteAsync("DELETE FROM Exercise WHERE Id = 3");
        logger.Collector.Clear();

        await seeder.SeedAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (1101, LogLevel.Information, "Exercise list seeded: 1 added, 2 updated"),
        }));
    }

    private IEnumerable<(int Id, string Name, ExerciseType Type)> Expected() =>
        catalog.All.Select(e => (e.Id, e.Name, e.Type));

    private async Task<List<(int Id, string Name, ExerciseType Type)>> Rows() =>
        (await connection.Table<Exercise>().OrderBy(e => e.Id).ToListAsync()).Select(e => (e.Id, e.Name, e.Type)).ToList();
}
