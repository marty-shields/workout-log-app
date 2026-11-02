using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>New and Edit Workout against Core's real services and a temporary SQLite database.</summary>
public class NotesWithCoreTests : ComponentTestBase
{
    private const int Squat = 6;

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private string databasePath = null!;
    private ServiceProvider core = null!;

    [SetUp]
    public async Task SetUp()
    {
        databasePath = Path.Combine(Path.GetTempPath(), $"workoutlog-ui-{Guid.NewGuid():N}.db3");
        core = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton<IExportEnvironment, StubEnvironment>()
            .AddSingleton<IElapsedClock, StubClock>()
            .AddCore(databasePath)
            .BuildServiceProvider();
        await core.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
        Context.Services.AddSingleton<TimeProvider>(new FakeTimeProvider(DateTimeOffset.UtcNow));
        Context.Services.AddSingleton(core.GetRequiredService<IWorkoutService>());
        Context.Services.AddSingleton(core.GetRequiredService<IWeightHistoryService>());
    }

    [TearDown]
    public void TearDown() => core.Dispose();

    [Test]
    public async Task NotesSavedOnNewWorkout_ShowInEditWorkoutAndAsLastTimesNoteOnTheNextWorkout()
    {
        var workoutId = await SaveNewWorkoutAsync("Slept badly", "Belt on from set 3");

        var edit = Context.Render<EditWorkoutPage>(p => p.Add(x => x.WorkoutId, workoutId));
        edit.WaitForState(() => edit.FindAll("[data-testid=save-workout]").Count == 1, Timeout);
        edit.Find("[data-testid=exercise-1-edit]").Click();
        edit.WaitForState(() => edit.FindAll("[data-testid=exercise-1-note-open]").Count == 1, Timeout);

        Assert.Multiple(() =>
        {
            Assert.That(edit.Find("[data-testid=workout-note-open] .text").TextContent, Is.EqualTo("Slept badly"));
            Assert.That(edit.Find("[data-testid=exercise-1-note-open] .text").TextContent, Is.EqualTo("Belt on from set 3"));
            Assert.That(edit.FindAll("[data-testid=exercise-1-last-note]"), Is.Empty);
        });

        await Task.Delay(50);
        var next = Context.Render<NewWorkoutPage>();
        AddExercise(next, Squat);

        next.WaitForState(() => next.FindAll("[data-testid=exercise-1-last-note]").Count == 1, Timeout);
        Assert.That(next.Find("[data-testid=exercise-1-last-note] .text").TextContent, Is.EqualTo("Belt on from set 3"));
    }

    private async Task<int> SaveNewWorkoutAsync(string workoutNote, string exerciseNote)
    {
        int? savedId = null;
        var cut = Context.Render<NewWorkoutPage>(p => p.Add(x => x.OnSaved, id => savedId = id));
        AddExercise(cut, Squat);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("60");
        TypeNote(cut, "workout-note-add", "workout-note", workoutNote);
        TypeNote(cut, "exercise-1-add-note", "exercise-1-note", exerciseNote);

        cut.Find("[data-testid=save-workout]").Click();

        cut.WaitForState(() => savedId is not null, Timeout);
        await WaitForRendererAsync();
        return savedId!.Value;
    }

    private static void AddExercise(IRenderedComponent<NewWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=exercises-row-{exerciseId}]").Click();
    }

    private static void TypeNote(IRenderedComponent<NewWorkoutPage> cut, string addButton, string note, string text)
    {
        cut.Find($"[data-testid={addButton}]").Click();
        cut.WaitForState(() => cut.FindAll($"[data-testid={note}-input]").Count == 1, Timeout);
        var input = cut.Find($"[data-testid={note}-input]");
        input.Input(text);
        input.Blur();
    }

    private sealed class StubEnvironment : IExportEnvironment
    {
        public string AppVersion => "test";

        public string CacheFolder => Path.GetTempPath();
    }

    private sealed class StubClock : IElapsedClock
    {
        private readonly System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

        public TimeSpan Elapsed => watch.Elapsed;
    }
}
