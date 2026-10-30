using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

public class GatedWorkoutRepositoryTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly WorkoutExercise[] Exercises = [new StrengthExercise(1, [new StrengthSet(10, 20m, null, null)])];

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private DatabaseGate gate = null!;
    private IWorkoutRepository repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.RunInTransactionAsync(WorkoutTables.CreateMissing);
        gate = new DatabaseGate();
        repository = new GatedWorkoutRepository(new WorkoutRepository(connection, new WorkoutWriterFactory()), gate);
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
        var id = await repository.AddAsync(new NewWorkout(Start, Start.AddMinutes(30), Exercises));
        await repository.ReplaceExercisesAsync(id, []);

        Assert.That(await repository.GetAsync(id), Is.EqualTo(new Workout(id, Start, Start.AddMinutes(30), [])).UsingPropertiesComparer());
        Assert.That(await repository.ListAsync(), Is.EqualTo(new[] { new WorkoutSummary(id, Start) }));
        Assert.That(await repository.ListAsync(Start.AddTicks(1), null, null), Is.Empty);
        Assert.That(await repository.ListAsync(null, Start.AddTicks(-1), null), Is.Empty);
        Assert.That(await repository.ListAsync(null, null, 0), Is.Empty);
        Assert.That(await repository.GetLatestAsync(), Is.EqualTo(new WorkoutOutline(id, Start, Start.AddMinutes(30), [])).UsingPropertiesComparer());
        Assert.That(await repository.GetWeightsAsync(1), Is.Empty);
        Assert.That(await repository.GetLastSessionsAsync(), Is.Empty);
        Assert.That(await repository.GetSessionsAsync(1), Is.Empty);

        await repository.DeleteAsync(id);

        Assert.That(await repository.GetAsync(id), Is.Null);
    }

    [TestCase("Add")]
    [TestCase("Replace")]
    [TestCase("Get")]
    [TestCase("List")]
    [TestCase("Latest")]
    [TestCase("Weights")]
    [TestCase("LastSessions")]
    [TestCase("Sessions")]
    [TestCase("Delete")]
    public async Task Call_WhileTheGateIsHeld_WaitsForIt(string call)
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Start.AddMinutes(30), Exercises));
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        Task pending = call switch
        {
            "Add" => repository.AddAsync(new NewWorkout(Start.AddHours(1), Start.AddHours(1).AddMinutes(30), Exercises)),
            "Replace" => repository.ReplaceExercisesAsync(id, []),
            "Get" => repository.GetAsync(id),
            "List" => repository.ListAsync(),
            "Latest" => repository.GetLatestAsync(),
            "Weights" => repository.GetWeightsAsync(1),
            "LastSessions" => repository.GetLastSessionsAsync(),
            "Sessions" => repository.GetSessionsAsync(1),
            _ => repository.DeleteAsync(id),
        };

        Assert.That(pending.IsCompleted, Is.False);
        release.SetResult();
        await Task.WhenAll(holder, pending);
    }
}
