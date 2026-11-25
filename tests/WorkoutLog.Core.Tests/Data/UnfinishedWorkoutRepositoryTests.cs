using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

public class UnfinishedWorkoutRepositoryTests
{
    private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private UnfinishedWorkoutRepository repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.RunInTransactionAsync(UnfinishedTables.CreateMissing);
        repository = new UnfinishedWorkoutRepository(connection);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task GetAsync_NothingStored_ReturnsNull()
    {
        Assert.That(await repository.GetAsync(), Is.Null);
    }

    [Test]
    public async Task ReplaceAsync_ThenGetAsync_ReturnsEverythingAsTypedInOrder()
    {
        var workout = Typed();

        await repository.ReplaceAsync(workout);

        var read = (await repository.GetAsync())!;
        Assert.Multiple(() =>
        {
            Assert.That(read.StartedAt, Is.EqualTo(Start));
            Assert.That(read.Note, Is.EqualTo("  felt strong\nnew line "));
            Assert.That(read.Exercises.Select(e => (e.ExerciseId, e.Note, e.IsDone)), Is.EqualTo(new (int, string?, bool)[] { (1, "tempo", true), (300, null, false), (8, "", false) }));
            Assert.That(read.Exercises[0].Entries.Select(e => e.Values), Is.EqualTo(new[]
            {
                new Dictionary<string, string?> { ["reps"] = "5", ["weight"] = "62.5" },
                new Dictionary<string, string?> { ["reps"] = "abc", ["weight"] = "-3", ["extra"] = null },
                new Dictionary<string, string?> { ["reps"] = "", ["weight"] = "6" },
            }));
            Assert.That(read.Exercises[1].Entries.Select(e => e.Values), Is.EqualTo(new[] { new Dictionary<string, string?> { ["duration"] = "0:20:5", ["distance"] = "5,2" } }));
            Assert.That(read.Exercises[2].Entries, Is.Empty);
        });
    }

    [Test]
    public async Task ReplaceAsync_Twice_KeepsOnlyTheLatest()
    {
        await repository.ReplaceAsync(Typed());
        var latest = new UnfinishedWorkout(Start.AddMinutes(1), null, [new UnfinishedExercise(8, null, false, [new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "9" })])]);

        await repository.ReplaceAsync(latest);

        var read = (await repository.GetAsync())!;
        Assert.Multiple(async () =>
        {
            Assert.That(read.StartedAt, Is.EqualTo(Start.AddMinutes(1)));
            Assert.That(read.Note, Is.Null);
            Assert.That(read.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { 8 }));
            Assert.That(await RowCounts(), Is.EqualTo((1, 1, 1)));
        });
    }

    [Test]
    public async Task ReplaceAsync_NoExercises_DeletesTheUnfinishedWorkout()
    {
        await repository.ReplaceAsync(Typed());

        await repository.ReplaceAsync(new UnfinishedWorkout(Start, "note", []));

        Assert.Multiple(async () =>
        {
            Assert.That(await repository.GetAsync(), Is.Null);
            Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0)));
        });
    }

    [Test]
    public async Task ReplaceAsync_NoExercisesAndNothingStored_StoresNothing()
    {
        await repository.ReplaceAsync(new UnfinishedWorkout(Start, null, []));

        Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0)));
    }

    [Test]
    public async Task ReplaceAsync_WriteFails_KeepsTheEarlierOneWhole()
    {
        await repository.ReplaceAsync(Typed());
        await connection.ExecuteAsync("CREATE TRIGGER fail_entry BEFORE INSERT ON UnfinishedEntry BEGIN SELECT RAISE(ABORT, 'disk full'); END");

        Assert.That(() => repository.ReplaceAsync(new UnfinishedWorkout(Start.AddHours(1), "other", [new UnfinishedExercise(8, null, false, [new UnfinishedEntry(new Dictionary<string, string?>())])])),
            Throws.InstanceOf<SQLiteException>());

        var read = (await repository.GetAsync())!;
        Assert.Multiple(async () =>
        {
            Assert.That(read.StartedAt, Is.EqualTo(Start));
            Assert.That(read.Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { 1, 300, 8 }));
            Assert.That(await RowCounts(), Is.EqualTo((1, 3, 4)));
        });
    }

    [Test]
    public async Task DeleteAsync_RemovesEveryRow()
    {
        await repository.ReplaceAsync(Typed());

        await repository.DeleteAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await repository.GetAsync(), Is.Null);
            Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0)));
        });
    }

    [Test]
    public async Task DeleteAsync_NothingStored_DoesNothing()
    {
        await repository.DeleteAsync();

        Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0)));
    }

    /// <summary>A workout with a note, three exercises (the last with no sets) and values that are blank, half-typed or invalid.</summary>
    internal static UnfinishedWorkout Typed() => new(
        Start,
        "  felt strong\nnew line ",
        [
            new UnfinishedExercise(1, "tempo", true,
            [
                new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "5", ["weight"] = "62.5" }),
                new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "abc", ["weight"] = "-3", ["extra"] = null }),
                new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "", ["weight"] = "6" }),
            ]),
            new UnfinishedExercise(300, null, false, [new UnfinishedEntry(new Dictionary<string, string?> { ["duration"] = "0:20:5", ["distance"] = "5,2" })]),
            new UnfinishedExercise(8, "", false, []),
        ]);

    private async Task<(int Workouts, int Exercises, int Entries)> RowCounts() =>
        (await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM UnfinishedWorkout"),
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM UnfinishedExercise"),
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM UnfinishedEntry"));
}
