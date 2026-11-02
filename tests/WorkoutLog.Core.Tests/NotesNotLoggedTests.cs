using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests;

/// <summary>Runs schema step 5, saves, edits, a failed save and an import with notes on one database, then reads every log record.</summary>
public class NotesNotLoggedTests
{
    private const string Sentinel = "sentinel-note-7f3a";

    [Test]
    public async Task NotesInSavesImportsAndTheSchemaStep_AreNeverLogged()
    {
        var path = TempDatabase.NewPath();
        await CreateVersion4Async(path);
        var time = new FakeTimeProvider(Start.AddDays(1));
        using var provider = new ServiceCollection()
            .AddFakeLogging(o => o.CollectRecordsForDisabledLogLevels = true)
            .AddCore(path)
            .AddSingleton<TimeProvider>(time)
            .AddSingleton<IElapsedClock>(new FakeElapsedClock())
            .BuildServiceProvider();
        try
        {
            await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var workouts = provider.GetRequiredService<IWorkoutService>();
            var draft = workouts.StartNew();
            WorkoutExercise[] exercises = [new StrengthExercise(8, [new StrengthSet(8, 60m, null, null)], Sentinel)];
            var id = (await workouts.SaveNewAsync(draft, Sentinel, exercises)).WorkoutId!.Value;
            await workouts.SaveAsync(id, $"{Sentinel} edited", exercises);
            var failed = await workouts.SaveNewAsync(draft, Sentinel, exercises);
            await workouts.GetAsync(id);
            await provider.GetRequiredService<IWeightHistoryService>().GetAsync(8, null);
            time.Advance(TimeSpan.FromDays(2));
            var import = provider.GetRequiredService<IImportService>();
            var review = await import.ReadAsync(new MemoryStream(Encoding.UTF8.GetBytes(File(
                WithNote(Workout(24, Bench.Replace("\"sets\"", $"\"note\":\"{Sentinel} file\",\"sets\"")), $"{Sentinel} file"),
                WithNote(Workout(0, Bench.Replace("\"sets\"", $"\"note\":\"{Sentinel}\",\"sets\"")), Sentinel),
                WithNote(Workout(1, Bench), Sentinel + new string('n', 500))))), "notes.json");
            await import.ImportAsync(review, ImportMode.Add, DifferentChoice.UseFiles);
            var records = provider.GetFakeLogCollector().GetSnapshot();

            Assert.Multiple(() =>
            {
                Assert.That(failed.WorkoutId, Is.Null, "the second save of the same start fails");
                Assert.That((review.New, review.Different, review.Skipped), Is.EqualTo((1, 1, 1)), "new, different on a note, and an over-long note");
                Assert.That(records.Select(r => (CoreEvent)r.Id.Id), Is.SupersetOf(new[]
                {
                    CoreEvent.SchemaStepApplied, CoreEvent.WorkoutAdded, CoreEvent.WorkoutUpdated, CoreEvent.WorkoutAddFailed,
                    CoreEvent.ImportWorkoutSkipped, CoreEvent.ImportDone,
                }));
                Assert.That(records.Where(r => r.Message.Contains(Sentinel, StringComparison.Ordinal) || r.Exception?.ToString().Contains(Sentinel, StringComparison.Ordinal) == true)
                    .Select(r => r.Message), Is.Empty);
            });
        }
        finally
        {
            SQLiteAsyncConnection.ResetPool();
            System.IO.File.Delete(path);
        }
    }

    private static async Task CreateVersion4Async(string path)
    {
        var connection = new SQLiteAsyncConnection(path);
        await connection.RunInTransactionAsync(db =>
        {
            foreach (var table in Version4Tables.All)
            {
                db.Execute(table.CreateSql(table.Name, ifNotExists: false));
            }

            db.Execute("PRAGMA user_version = 4");
        });
        await connection.CloseAsync();
    }
}