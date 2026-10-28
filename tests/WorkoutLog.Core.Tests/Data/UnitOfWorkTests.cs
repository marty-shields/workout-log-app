using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

public class UnitOfWorkTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly WorkoutExercise[] Exercises = [new StrengthExercise(1, [new StrengthSet(10, 20m, null, null)])];

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;
    private IWorkoutRepository repository = null!;
    private IUnitOfWork unitOfWork = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await connection.CreateTablesAsync<WorkoutRow, WorkoutExerciseRow, StrengthSetRow, CardioEntryRow>();
        repository = new WorkoutRepository(connection, new WorkoutWriterFactory(new ExerciseCatalog()));
        unitOfWork = new UnitOfWork(connection, new WorkoutWriterFactory(new ExerciseCatalog()));
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task RunAsync_KeepsEveryWrite()
    {
        await repository.AddAsync(new NewWorkout(Start, Start.AddHours(1), Exercises));

        await unitOfWork.RunAsync(writer =>
        {
            writer.DeleteAll();
            writer.Add(new NewWorkout(Start.AddDays(1), Start.AddDays(1), Exercises));
            writer.Add(new NewWorkout(Start.AddDays(2), Start.AddDays(2), []));
        });

        Assert.That((await repository.ListAsync()).Select(w => w.StartedAt), Is.EqualTo(new[] { Start.AddDays(2), Start.AddDays(1) }));
    }

    [Test]
    public async Task RunAsync_WriteThrows_KeepsNothingAndRethrows()
    {
        var kept = new Workout(await repository.AddAsync(new NewWorkout(Start, Start.AddHours(1), Exercises)), Start, Start.AddHours(1), Exercises);

        Assert.That(() => unitOfWork.RunAsync(writer =>
        {
            writer.DeleteAll();
            writer.Add(new NewWorkout(Start.AddDays(1), Start.AddDays(1), Exercises));
            writer.Add(new NewWorkout(Start.AddDays(1), Start.AddDays(1), Exercises));
        }), Throws.InstanceOf<SQLiteException>());

        Assert.That(await repository.GetAllAsync(), Is.EqualTo(new[] { kept }).UsingPropertiesComparer());
    }
}
