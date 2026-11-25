using SQLite;
using WorkoutLog.Core.Data;

namespace WorkoutLog.Core.Tests.Data;

public class GatedUnfinishedWorkoutRepositoryTests
{
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private DatabaseGate gate = null!;
    private IUnfinishedWorkoutRepository repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.RunInTransactionAsync(UnfinishedTables.CreateMissing);
        gate = new DatabaseGate();
        repository = new GatedUnfinishedWorkoutRepository(new UnfinishedWorkoutRepository(connection), gate);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task Calls_PassThroughToTheRepository()
    {
        await repository.ReplaceAsync(UnfinishedWorkoutRepositoryTests.Typed());
        Assert.That(await repository.GetAsync(), Is.Not.Null);

        await repository.DeleteAsync();

        Assert.That(await repository.GetAsync(), Is.Null);
    }

    [TestCase("Get")]
    [TestCase("Replace")]
    [TestCase("Delete")]
    public async Task Call_WhileTheGateIsHeld_WaitsForIt(string call)
    {
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        Task pending = call switch
        {
            "Get" => repository.GetAsync(),
            "Replace" => repository.ReplaceAsync(UnfinishedWorkoutRepositoryTests.Typed()),
            _ => repository.DeleteAsync(),
        };

        Assert.That(pending.IsCompleted, Is.False);
        release.SetResult();
        await Task.WhenAll(holder, pending);
    }
}
