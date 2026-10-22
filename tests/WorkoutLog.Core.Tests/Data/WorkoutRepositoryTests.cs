using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

public class WorkoutRepositoryTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Finish = Start.AddMinutes(62);

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private WorkoutRepository repository = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.CreateTablesAsync<WorkoutRow, WorkoutExerciseRow, StrengthSetRow, CardioEntryRow>();
        repository = new WorkoutRepository(connection);
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task AddAsync_ThenGetAsync_ReturnsTheWorkoutInOrder()
    {
        var exercises = Exercises();

        var id = await repository.AddAsync(new NewWorkout(Start, Finish, exercises));

        Assert.That(await repository.GetAsync(id),
            Is.EqualTo(new Workout(id, Start, Finish, exercises)).UsingPropertiesComparer());
    }

    [Test]
    public async Task AddAsync_StrengthExerciseWithNoSets_ReadsBackAsStrength()
    {
        WorkoutExercise[] exercises = [new StrengthExercise(1, [])];

        var id = await repository.AddAsync(new NewWorkout(Start, Finish, exercises));

        Assert.That((await repository.GetAsync(id))!.Exercises,
            Is.EqualTo(exercises).UsingPropertiesComparer());
    }

    [Test]
    public async Task AddAsync_TwoWorkouts_ReturnsDifferentIds()
    {
        var first = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var second = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public async Task AddAsync_TimesWithAnOffset_AreStoredAsUtc()
    {
        var offset = TimeSpan.FromHours(1);
        var started = Start.ToOffset(offset);

        var id = await repository.AddAsync(new NewWorkout(started, Finish.ToOffset(offset), Exercises()));
        var workout = (await repository.GetAsync(id))!;

        Assert.That(workout.StartedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(workout.StartedAt.UtcDateTime, Is.EqualTo(Start.UtcDateTime));
        Assert.That(workout.FinishedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(workout.FinishedAt.UtcDateTime, Is.EqualTo(Finish.UtcDateTime));
        Assert.That(await connection.ExecuteScalarAsync<long>("SELECT StartedAt FROM Workout WHERE Id = ?", id),
            Is.EqualTo(Start.UtcTicks));
        Assert.That(await connection.ExecuteScalarAsync<long>("SELECT FinishedAt FROM Workout WHERE Id = ?", id),
            Is.EqualTo(Finish.UtcTicks));
    }

    [Test]
    public async Task AddAsync_NoExercises_ReadsBackWithNoExercises()
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Finish, []));

        Assert.That(await repository.GetAsync(id),
            Is.EqualTo(new Workout(id, Start, Finish, [])).UsingPropertiesComparer());
    }

    [Test]
    public async Task AddAsync_UnknownExerciseType_ThrowsAndWritesNothing()
    {
        Assert.That(() => repository.AddAsync(new NewWorkout(Start, Finish, [new OtherExercise(1)])),
            Throws.TypeOf<ArgumentOutOfRangeException>());

        Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public async Task AddAsync_CardioEntryWithNoDuration_ThrowsAndWritesNothing()
    {
        Assert.That(() => repository.AddAsync(new NewWorkout(Start, Finish, [new CardioExercise(300, Entry() with { Duration = null })])),
            Throws.ArgumentException);

        Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public async Task GetAsync_MissingId_ReturnsNull()
    {
        Assert.That(await repository.GetAsync(1), Is.Null);
    }

    [Test]
    public async Task ReplaceExercisesAsync_ReplacesTheChildrenAndKeepsTheTimes()
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        WorkoutExercise[] replacement = [new StrengthExercise(5, [new StrengthSet(8, 40m, null, null)])];

        await repository.ReplaceExercisesAsync(id, replacement);

        Assert.That(await repository.GetAsync(id),
            Is.EqualTo(new Workout(id, Start, Finish, replacement)).UsingPropertiesComparer());
        Assert.That(await RowCounts(), Is.EqualTo((1, 1, 1, 0)));
    }

    [Test]
    public async Task ReplaceExercisesAsync_LeavesOtherWorkoutsAlone()
    {
        var replaced = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var kept = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));

        await repository.ReplaceExercisesAsync(replaced, [new StrengthExercise(5, [new StrengthSet(8, 40m, null, null)])]);

        Assert.That(await repository.GetAsync(kept),
            Is.EqualTo(new Workout(kept, Start, Finish, Exercises())).UsingPropertiesComparer());
        Assert.That(await RowCounts(), Is.EqualTo((2, 4, 4, 1)));
    }

    [Test]
    public async Task ReplaceExercisesAsync_MissingId_ThrowsAndWritesNothing()
    {
        Assert.That(() => repository.ReplaceExercisesAsync(1, Exercises()), Throws.TypeOf<KeyNotFoundException>());

        Assert.That(await RowCounts(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public async Task DeleteAsync_RemovesTheWorkoutAndItsChildrenOnly()
    {
        var deleted = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var kept = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));

        await repository.DeleteAsync(deleted);

        Assert.That(await repository.GetAsync(deleted), Is.Null);
        Assert.That(await repository.GetAsync(kept),
            Is.EqualTo(new Workout(kept, Start, Finish, Exercises())).UsingPropertiesComparer());
        Assert.That(await RowCounts(), Is.EqualTo((1, 3, 3, 1)));
    }

    [Test]
    public void DeleteAsync_MissingId_Throws()
    {
        Assert.That(() => repository.DeleteAsync(1), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task CardioEntry_SecondEntryForOneExercise_IsRejected()
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Finish, [new CardioExercise(300, Entry())]));
        var exerciseRowId = (await connection.Table<WorkoutExerciseRow>().FirstAsync(e => e.WorkoutId == id)).Id;

        Assert.That(() => connection.InsertAsync(new CardioEntryRow { WorkoutExerciseId = exerciseRowId, Duration = TimeSpan.FromMinutes(5) }),
            Throws.InstanceOf<SQLiteException>());
    }

    private static WorkoutExercise[] Exercises() =>
    [
        new StrengthExercise(1,
        [
            new StrengthSet(10, 22.75m, null, null),
            new StrengthSet(8, 25m, null, null),
        ]),
        new CardioExercise(300, Entry()),
        new StrengthExercise(1, [new StrengthSet(null, 20m, TimeSpan.FromSeconds(45), 30.5m)]),
    ];

    private static CardioEntry Entry() =>
        new(new TimeSpan(0, 25, 30), 5.2m, 12, 8, 1.5m, 310, 5.5m);

    private async Task<(int Workouts, int Exercises, int Sets, int Entries)> RowCounts() => (
        await connection.Table<WorkoutRow>().CountAsync(),
        await connection.Table<WorkoutExerciseRow>().CountAsync(),
        await connection.Table<StrengthSetRow>().CountAsync(),
        await connection.Table<CardioEntryRow>().CountAsync());

    private sealed record OtherExercise(int ExerciseId) : WorkoutExercise(ExerciseId);
}
