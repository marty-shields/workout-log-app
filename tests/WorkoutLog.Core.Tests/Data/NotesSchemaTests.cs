using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Data;

public class NotesSchemaTests
{
    private const string Note501 = "replace(hex(zeroblob(501)), '00', 'n')";
    private const string Note500 = "replace(hex(zeroblob(500)), '00', 'n')";

    private static readonly ExerciseCatalog Catalog = new();

    private string databasePath = "";
    private SQLiteAsyncConnection connection = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = TempDatabase.NewPath();
        connection = new SQLiteAsyncConnection(databasePath);
        await CreateVersion4Async();
    }

    [TearDown]
    public async Task TearDown()
    {
        await connection.CloseAsync();
        File.Delete(databasePath);
    }

    [Test]
    public async Task Initialize_Version4Database_AddsTheNoteColumnsAndKeepsEveryRow()
    {
        await InitializeAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await connection.ExecuteScalarAsync<int>("PRAGMA user_version"), Is.EqualTo(5));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT lower(type) FROM pragma_table_info('Workout') WHERE name = 'Note'"), Is.EqualTo(new[] { "varchar" }));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT lower(type) FROM pragma_table_info('WorkoutExercise') WHERE name = 'Note'"), Is.EqualTo(new[] { "varchar" }));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT StartedAt || ':' || FinishedAt || ':' || IFNULL(Note, 'null') FROM Workout"), Is.EqualTo(new[] { "100:200:null" }));
            Assert.That(await connection.QueryScalarsAsync<string>("SELECT ExerciseId || ':' || Position || ':' || IFNULL(Note, 'null') FROM WorkoutExercise ORDER BY Id"),
                Is.EqualTo(new[] { "6:0:null", "300:1:null" }));
            Assert.That(await connection.ExecuteScalarAsync<double>("SELECT WeightKg FROM StrengthSet"), Is.EqualTo(60));
            Assert.That(await connection.ExecuteScalarAsync<double>("SELECT Distance FROM CardioEntry"), Is.EqualTo(5.2));
        });
    }

    [TestCase("UPDATE Workout SET Note = ''", TestName = "Empty workout note")]
    [TestCase("UPDATE Workout SET Note = " + Note501, TestName = "501-character workout note")]
    [TestCase("UPDATE WorkoutExercise SET Note = ''", TestName = "Empty exercise note")]
    [TestCase("UPDATE WorkoutExercise SET Note = " + Note501, TestName = "501-character exercise note")]
    public async Task Initialize_Version4Database_NotesHaveTheirRule(string update)
    {
        await InitializeAsync();

        Assert.That(() => connection.ExecuteAsync(update), Throws.InstanceOf<SQLiteException>());
    }

    [TestCase("UPDATE Workout SET Note = " + Note500, TestName = "500-character workout note")]
    [TestCase("UPDATE Workout SET Note = 'n'", TestName = "1-character workout note")]
    [TestCase("UPDATE WorkoutExercise SET Note = " + Note500, TestName = "500-character exercise note")]
    [TestCase("UPDATE WorkoutExercise SET Note = NULL", TestName = "No exercise note")]
    public async Task Initialize_Version4Database_AcceptsNotesWithinTheRule(string update)
    {
        await InitializeAsync();

        Assert.That(await connection.ExecuteAsync(update), Is.GreaterThan(0));
    }

    [Test]
    public async Task Apply_Twice_AddsEachColumnOnce()
    {
        await connection.RunInTransactionAsync(db =>
        {
            new NotesSchema().Apply(db);
            new NotesSchema().Apply(db);
        });

        Assert.That(await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM pragma_table_info('Workout') WHERE name = 'Note'") + await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM pragma_table_info('WorkoutExercise') WHERE name = 'Note'"), Is.EqualTo(2));
    }

    private Task InitializeAsync() =>
        new DatabaseInitializer(connection,
            [new WorkoutStartedAtUniqueIndex(), new BlankAssistToZero(Catalog), new ValidationRulesSchema(Catalog, NullLogger<ValidationRulesSchema>.Instance), new NotesSchema()],
            Mock.Of<IExerciseSeeder>(), NullLogger<DatabaseInitializer>.Instance).InitializeAsync();

    /// <summary>The version 4 schema with one workout of a strength set and a cardio entry.</summary>
    private async Task CreateVersion4Async()
    {
        await connection.ExecuteAsync("CREATE TABLE Exercise (Id integer primary key not null, Name varchar, Type integer)");
        await connection.RunInTransactionAsync(db =>
        {
            foreach (var table in Version4Tables.All)
            {
                db.Execute(table.CreateSql(table.Name, ifNotExists: false));
                foreach (var index in table.Indexes)
                {
                    db.Execute(string.Format(index, ""));
                }
            }
        });
        string[] rows =
        [
            "INSERT INTO Workout (StartedAt, FinishedAt) VALUES (100, 200)",
            "INSERT INTO WorkoutExercise (WorkoutId, ExerciseId, Position) VALUES (1, 6, 0), (1, 300, 1)",
            "INSERT INTO StrengthSet (WorkoutExerciseId, Position, Reps, WeightKg) VALUES (1, 0, 5, 60)",
            "INSERT INTO CardioEntry (WorkoutExerciseId, Duration, Distance) VALUES (2, 18000000000, 5.2)",
            "PRAGMA user_version = 4",
        ];
        foreach (var row in rows)
        {
            await connection.ExecuteAsync(row);
        }
    }
}