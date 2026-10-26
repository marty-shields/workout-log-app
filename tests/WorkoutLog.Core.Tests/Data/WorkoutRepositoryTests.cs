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
    private IWorkoutRepository repository = null!;

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
    public async Task AddAsync_TwoWorkoutsOnTheSameDay_BothReadBack()
    {
        var later = new NewWorkout(Start.AddHours(2), Finish.AddHours(2), Exercises());

        var first = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var second = await repository.AddAsync(later);

        var saved = new[] { await repository.GetAsync(first), await repository.GetAsync(second) };

        Assert.That(saved.Select(w => w!.StartedAt), Is.EqualTo(new[] { Start, later.StartedAt }));
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
    public async Task ListAsync_NoWorkouts_ReturnsEmpty()
    {
        Assert.That(await repository.ListAsync(), Is.Empty);
    }

    [Test]
    public async Task ListAsync_OrdersByStartTimeNewestFirstNotById()
    {
        var middle = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var oldest = await repository.AddAsync(new NewWorkout(Start.AddDays(-2), Finish.AddDays(-2), Exercises()));
        var newest = await repository.AddAsync(new NewWorkout(Start.AddHours(3), Finish.AddHours(3), []));

        Assert.That(await repository.ListAsync(), Is.EqualTo(new[]
        {
            new WorkoutSummary(newest, Start.AddHours(3)),
            new WorkoutSummary(middle, Start),
            new WorkoutSummary(oldest, Start.AddDays(-2)),
        }));
    }

    [Test]
    public async Task ListAsync_SameStartTime_NewestIdFirst()
    {
        var first = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));
        var second = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));

        Assert.That((await repository.ListAsync()).Select(w => w.Id), Is.EqualTo(new[] { second, first }));
    }

    [Test]
    public async Task ListAsync_TimesWithAnOffset_ReadBackAsUtc()
    {
        await repository.AddAsync(new NewWorkout(Start.ToOffset(TimeSpan.FromHours(1)), Finish, Exercises()));

        var started = (await repository.ListAsync()).Single().StartedAt;

        Assert.That(started.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(started.UtcDateTime, Is.EqualTo(Start.UtcDateTime));
    }

    [Test]
    public async Task ListAsync_From_ReturnsStartsAtOrAfterIt()
    {
        var ids = await AddStartsAsync(-2, -1, 0, 1);

        Assert.That((await repository.ListAsync(from: Start.AddHours(-1))).Select(w => w.Id), Is.EqualTo(new[] { ids[3], ids[2], ids[1] }));
    }

    [Test]
    public async Task ListAsync_To_ReturnsStartsAtOrBeforeIt()
    {
        var ids = await AddStartsAsync(-2, -1, 0, 1);

        Assert.That((await repository.ListAsync(to: Start)).Select(w => w.Id), Is.EqualTo(new[] { ids[2], ids[1], ids[0] }));
    }

    [Test]
    public async Task ListAsync_Limit_ReturnsTheNewest()
    {
        var ids = await AddStartsAsync(-2, -1, 0, 1);

        Assert.That((await repository.ListAsync(limit: 2)).Select(w => w.Id), Is.EqualTo(new[] { ids[3], ids[2] }));
    }

    [Test]
    public async Task ListAsync_FromToAndLimit_ApplyTogether()
    {
        var ids = await AddStartsAsync(-2, -1, 0, 1);

        Assert.That((await repository.ListAsync(Start.AddHours(-2), Start, 2)).Select(w => w.Id), Is.EqualTo(new[] { ids[2], ids[1] }));
    }

    [Test]
    public async Task ListAsync_BoundsWithAnOffset_CompareAsInstants()
    {
        var ids = await AddStartsAsync(-1, 0, 1);
        var offset = TimeSpan.FromHours(1);

        var workouts = await repository.ListAsync(Start.ToOffset(offset), Start.ToOffset(offset));

        Assert.That(workouts.Select(w => w.Id), Is.EqualTo(new[] { ids[1] }));
    }

    [Test]
    public async Task StartedAtIndex_ServesTheListOrderWithoutASort()
    {
        var plan = (await connection.QueryAsync<QueryPlanRow>(
                "EXPLAIN QUERY PLAN SELECT * FROM Workout ORDER BY StartedAt DESC, Id DESC"))
            .Select(r => r.Detail).ToList();

        Assert.That(plan, Has.Some.Contains("USING INDEX Workout_StartedAt"));
        Assert.That(plan, Has.None.Contains("TEMP B-TREE"));
    }

    [Test]
    public async Task GetWeightsAsync_NoWorkoutHasTheExercise_ReturnsEmpty()
    {
        await repository.AddAsync(new NewWorkout(Start, Finish, [new StrengthExercise(2, [new StrengthSet(5, 60m, null, null)])]));

        Assert.That(await repository.GetWeightsAsync(1), Is.Empty);
    }

    [Test]
    public async Task GetWeightsAsync_OrdersByStartTimeNewestFirstNotById()
    {
        var middle = await AddWeightsAsync(Start, 60m);
        var oldest = await AddWeightsAsync(Start.AddDays(-2), 50m);
        var newest = await AddWeightsAsync(Start.AddHours(3), 70m);

        Assert.That(await repository.GetWeightsAsync(1), Is.EqualTo(new[]
        {
            new WorkoutWeights(newest, [70m]),
            new WorkoutWeights(middle, [60m]),
            new WorkoutWeights(oldest, [50m]),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetWeightsAsync_SameStartTime_NewestIdFirst()
    {
        var first = await AddWeightsAsync(Start, 60m);
        var second = await AddWeightsAsync(Start, 70m);

        Assert.That((await repository.GetWeightsAsync(1)).Select(w => w.WorkoutId), Is.EqualTo(new[] { second, first }));
    }

    [Test]
    public async Task GetWeightsAsync_ReturnsEachSetsWeightAndSkipsSetsWithout()
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Finish,
        [
            new StrengthExercise(1, [new StrengthSet(5, 22.75m, null, null), new StrengthSet(5, null, null, null), new StrengthSet(5, 0m, null, null)]),
            new StrengthExercise(2, [new StrengthSet(5, 100m, null, null)]),
        ]));

        Assert.That(await repository.GetWeightsAsync(1), Is.EqualTo(new[] { new WorkoutWeights(id, [22.75m, 0m]) }).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetWeightsAsync_NoSetHasAWeight_ReturnsTheWorkoutWithNoWeights()
    {
        var noWeight = await repository.AddAsync(new NewWorkout(Start, Finish, [new StrengthExercise(1, [new StrengthSet(5, null, null, null)])]));
        var noSets = await repository.AddAsync(new NewWorkout(Start.AddHours(1), Finish, [new StrengthExercise(1, [])]));

        Assert.That(await repository.GetWeightsAsync(1), Is.EqualTo(new[]
        {
            new WorkoutWeights(noSets, []),
            new WorkoutWeights(noWeight, []),
        }).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetWeightsAsync_ExerciseTwiceInAWorkout_ReturnsOneEntryWithBothWeights()
    {
        var id = await repository.AddAsync(new NewWorkout(Start, Finish, Exercises()));

        Assert.That(await repository.GetWeightsAsync(1), Is.EqualTo(new[] { new WorkoutWeights(id, [22.75m, 25m, 20m]) }).UsingPropertiesComparer());
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
        new(new TimeSpan(0, 25, 30), 5.2m, 12, 8, 1.5m, 310, 5.5m, new TimeSpan(0, 5, 30));

    private async Task<(int Workouts, int Exercises, int Sets, int Entries)> RowCounts() => (
        await connection.Table<WorkoutRow>().CountAsync(),
        await connection.Table<WorkoutExerciseRow>().CountAsync(),
        await connection.Table<StrengthSetRow>().CountAsync(),
        await connection.Table<CardioEntryRow>().CountAsync());

    /// <summary>Adds a workout with no exercises at each hour from <see cref="Start"/>, in order. Returns their ids.</summary>
    private async Task<List<int>> AddStartsAsync(params int[] hours)
    {
        var ids = new List<int>();
        foreach (var hour in hours)
        {
            ids.Add(await repository.AddAsync(new NewWorkout(Start.AddHours(hour), Finish.AddHours(hour), [])));
        }

        return ids;
    }

    /// <summary>Adds a workout starting at <paramref name="started"/> with one set of exercise 1 at <paramref name="weightKg"/>. Returns its id.</summary>
    private Task<int> AddWeightsAsync(DateTimeOffset started, decimal weightKg) =>
        repository.AddAsync(new NewWorkout(started, started.AddHours(1), [new StrengthExercise(1, [new StrengthSet(5, weightKg, null, null)])]));

    private sealed record OtherExercise(int ExerciseId) : WorkoutExercise(ExerciseId);

    private sealed class QueryPlanRow
    {
        [Column("detail")]
        public string Detail { get; set; } = "";
    }
}
