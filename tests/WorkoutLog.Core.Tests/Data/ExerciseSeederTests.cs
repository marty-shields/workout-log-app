using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Data;

public class ExerciseSeederTests
{
    private readonly ExerciseCatalog catalog = new();
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private ExerciseSeeder seeder = null!;

    [SetUp]
    public void SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        seeder = new ExerciseSeeder(connection, catalog);
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
        await connection.CreateTableAsync<Exercise>();
        await connection.InsertAsync(new Exercise { Id = first.Id, Name = "Old name", Type = ExerciseType.Cardio });

        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected()));
    }

    [Test]
    public async Task SeedAsync_RowNotInTheFile_IsKept()
    {
        var extra = new Exercise { Id = catalog.All.Count + 1, Name = "Retired", Type = ExerciseType.Strength };
        await connection.CreateTableAsync<Exercise>();
        await connection.InsertAsync(extra);

        await seeder.SeedAsync();

        Assert.That(await Rows(), Is.EqualTo(Expected().Append((extra.Id, extra.Name, extra.Type))));
    }

    private IEnumerable<(int Id, string Name, ExerciseType Type)> Expected() =>
        catalog.All.Select(e => (e.Id, e.Name, e.Type));

    private async Task<List<(int Id, string Name, ExerciseType Type)>> Rows() =>
        (await connection.Table<Exercise>().OrderBy(e => e.Id).ToListAsync()).Select(e => (e.Id, e.Name, e.Type)).ToList();
}
