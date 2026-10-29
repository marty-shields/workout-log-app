using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Data;

public class GatedDatabaseInitializerTests
{
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private DatabaseGate gate = null!;
    private GatedDatabaseInitializer initializer = null!;

    [SetUp]
    public void SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        gate = new DatabaseGate();
        ISchemaUpgrade[] steps =
        [
            new WorkoutStartedAtUniqueIndex(),
            new BlankAssistToZero(new ExerciseCatalog()),
            new ValidationRulesSchema(new ExerciseCatalog(), NullLogger<ValidationRulesSchema>.Instance),
        ];
        initializer = new GatedDatabaseInitializer(new DatabaseInitializer(connection, steps, Mock.Of<IExerciseSeeder>(), NullLogger<DatabaseInitializer>.Instance), gate);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task InitializeAsync_PassesThroughToTheInitializer()
    {
        await initializer.InitializeAsync();

        Assert.That(await connection.ExecuteScalarAsync<int>("PRAGMA user_version"), Is.EqualTo(DatabaseInitializer.CurrentVersion));
    }

    [Test]
    public async Task InitializeAsync_WhileTheGateIsHeld_WaitsForIt()
    {
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        var pending = initializer.InitializeAsync();

        Assert.That(pending.IsCompleted, Is.False);
        release.SetResult();
        await Task.WhenAll(holder, pending);
    }
}
