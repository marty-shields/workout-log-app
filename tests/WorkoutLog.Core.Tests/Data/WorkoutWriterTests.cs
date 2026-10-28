using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

/// <summary>
/// Replace, DeleteAll and the blank assist on each write. Add, ReplaceExercises and Delete are otherwise covered through <see cref="WorkoutRepositoryTests"/>.
/// </summary>
public class WorkoutWriterTests
{
    /// <summary>Machine Assisted Pull-up: weight labelled Assist.</summary>
    private const int AssistedPullUp = 218;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);
    private static readonly DateTimeOffset Finish = Start.AddMinutes(62);

    private SQLiteConnection db = null!;
    private WorkoutWriter writer = null!;
    private WorkoutRepository repository = null!;
    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.CreateTablesAsync<WorkoutRow, WorkoutExerciseRow, StrengthSetRow, CardioEntryRow>();
        db = new SQLiteConnection(databasePath);
        writer = new WorkoutWriter(db, Catalog);
        repository = new WorkoutRepository(connection, new WorkoutWriterFactory(Catalog));
    }

    [TearDown]
    public async Task TearDown()
    {
        db.Dispose();
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task Replace_SetsTheFinishAndReplacesTheChildrenOfTheWorkoutWithTheStart()
    {
        var id = writer.Add(new NewWorkout(Start, Finish, Exercises()));
        var other = writer.Add(new NewWorkout(Start.AddHours(3), Finish.AddHours(3), Exercises()));
        WorkoutExercise[] replacement = [new StrengthExercise(5, [new StrengthSet(8, 40m, null, null)])];

        writer.Replace(new NewWorkout(Start, Finish.AddMinutes(5), replacement));

        Assert.That(await repository.GetAllAsync(), Is.EqualTo(new[]
        {
            new Workout(id, Start, Finish.AddMinutes(5), replacement),
            new Workout(other, Start.AddHours(3), Finish.AddHours(3), Exercises()),
        }).UsingPropertiesComparer());
        Assert.That(RowCounts(), Is.EqualTo((2, 3, 3, 1)));
    }

    [Test]
    public void Replace_NoWorkoutHasTheStart_ThrowsAndWritesNothing()
    {
        writer.Add(new NewWorkout(Start, Finish, Exercises()));

        Assert.That(() => writer.Replace(new NewWorkout(Start.AddTicks(1), Finish, [])), Throws.TypeOf<KeyNotFoundException>());

        Assert.That(RowCounts(), Is.EqualTo((1, 2, 2, 1)));
    }

    [Test]
    public void DeleteAll_RemovesEveryRow()
    {
        writer.Add(new NewWorkout(Start, Finish, Exercises()));
        writer.Add(new NewWorkout(Start.AddHours(3), Finish.AddHours(3), Exercises()));

        writer.DeleteAll();

        Assert.That(RowCounts(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public void DeleteAll_NoWorkouts_DoesNothing()
    {
        writer.DeleteAll();

        Assert.That(RowCounts(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public async Task Add_BlankWeight_IsZeroOnAnAssistedMachineOnly()
    {
        writer.Add(new NewWorkout(Start, Finish, BlankWeights()));

        await AssertBlankAssistIsZeroAsync();
    }

    [Test]
    public async Task Replace_BlankWeight_IsZeroOnAnAssistedMachineOnly()
    {
        writer.Add(new NewWorkout(Start, Finish, []));

        writer.Replace(new NewWorkout(Start, Finish, BlankWeights()));

        await AssertBlankAssistIsZeroAsync();
    }

    [Test]
    public async Task ReplaceExercises_BlankWeight_IsZeroOnAnAssistedMachineOnly()
    {
        var id = writer.Add(new NewWorkout(Start, Finish, []));

        writer.ReplaceExercises(id, BlankWeights());

        await AssertBlankAssistIsZeroAsync();
    }

    /// <summary>An assisted pull-up with a blank and a 25 kg set, and exercise 1 with a blank weight.</summary>
    private static WorkoutExercise[] BlankWeights() =>
    [
        new StrengthExercise(AssistedPullUp, [new StrengthSet(8, null, null, null), new StrengthSet(6, 25m, null, null)]),
        new StrengthExercise(1, [new StrengthSet(5, null, null, null)]),
    ];

    /// <summary>The one saved workout holds <see cref="BlankWeights"/> with the assisted blank as 0.</summary>
    private async Task AssertBlankAssistIsZeroAsync() =>
        Assert.That((await repository.GetAllAsync()).Single().Exercises, Is.EqualTo(new WorkoutExercise[]
        {
            new StrengthExercise(AssistedPullUp, [new StrengthSet(8, 0m, null, null), new StrengthSet(6, 25m, null, null)]),
            new StrengthExercise(1, [new StrengthSet(5, null, null, null)]),
        }).UsingPropertiesComparer());

    private static WorkoutExercise[] Exercises() =>
    [
        new StrengthExercise(1, [new StrengthSet(10, 22.75m, null, null), new StrengthSet(8, 25m, null, null)]),
        new CardioExercise(300, new CardioEntry(new TimeSpan(0, 25, 30), 5.2m, null, null, null, null, null, null)),
    ];

    private (int Workouts, int Exercises, int Sets, int Entries) RowCounts() => (
        db.Table<WorkoutRow>().Count(),
        db.Table<WorkoutExerciseRow>().Count(),
        db.Table<StrengthSetRow>().Count(),
        db.Table<CardioEntryRow>().Count());
}
