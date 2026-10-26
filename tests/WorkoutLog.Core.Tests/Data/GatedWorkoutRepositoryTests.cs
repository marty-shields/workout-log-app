using SQLite;
using WorkoutLog.Core.Data;
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
        await connection.CreateTablesAsync<WorkoutRow, WorkoutExerciseRow, StrengthSetRow, CardioEntryRow>();
        gate = new DatabaseGate();
        repository = new GatedWorkoutRepository(new WorkoutRepository(connection), gate);
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
        var id = await repository.AddAsync(new NewWorkout(Start, Start, Exercises));
        await repository.ReplaceExercisesAsync(id, []);

        Assert.That(await repository.GetAsync(id), Is.EqualTo(new Workout(id, Start, Start, [])).UsingPropertiesComparer());
        Assert.That(await repository.ListAsync(), Is.EqualTo(new[] { new WorkoutSummary(id, Start) }));
        Assert.That(await repository.ListAsync(Start.AddTicks(1), null, null), Is.Empty);
        Assert.That(await repository.ListAsync(null, Start.AddTicks(-1), null), Is.Empty);
        Assert.That(await repository.ListAsync(null, null, 0), Is.Empty);
        Assert.That(await repository.GetWeightsAsync(1), Is.Empty);

        await repository.DeleteAsync(id);

        Assert.That(await repository.GetAsync(id), Is.Null);
    }

    [TestCase("Add")]
    [TestCase("Replace")]
    [TestCase("Get")]
    [TestCase("List")]
    [TestCase("Weights")]
    [TestCase("Delete")]
    public async Task Call_WhileTheGateIsHeld_WaitsForIt(string call)
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Start, Exercises));
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        Task pending = call switch
        {
            "Add" => repository.AddAsync(new NewWorkout(Start.AddHours(1), Start.AddHours(1), Exercises)),
            "Replace" => repository.ReplaceExercisesAsync(id, []),
            "Get" => repository.GetAsync(id),
            "List" => repository.ListAsync(),
            "Weights" => repository.GetWeightsAsync(1),
            _ => repository.DeleteAsync(id),
        };

        Assert.That(pending.IsCompleted, Is.False);
        release.SetResult();
        await Task.WhenAll(holder, pending);
    }
}
