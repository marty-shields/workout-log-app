using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>The new workout is kept as it changes, can be resumed after a kill, and the kept one goes on Save and Discard.</summary>
public class NewWorkoutPageUnfinishedTests : ComponentTestBase
{
    private const int Curl = 132;
    private const int Treadmill = 300;

    /// <summary>Tue 20 Oct 2026 23:30 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    private FakeTimeProvider clock = null!;

    [SetUp]
    public void SetUp()
    {
        clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    [Test]
    public async Task Change_IsKeptAsTypedOnceFlushed()
    {
        var cut = Render();
        Add(cut, Curl);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.x");
        TypeNote(cut, null, "Slept badly");
        TypeNote(cut, 1, "Strict form");

        await Flush();

        var stored = UnfinishedWorkout.Stored!;
        Assert.Multiple(() =>
        {
            Assert.That(stored.StartedAt, Is.EqualTo(Now));
            Assert.That(stored.Note, Is.EqualTo("Slept badly"));
            Assert.That(stored.Exercises.Select(e => (e.ExerciseId, e.Note, e.IsDone)), Is.EqualTo(new (int, string?, bool)[] { (Curl, "Strict form", false) }));
            Assert.That(stored.Exercises.Single().Entries.Single().Values, Is.EqualTo(new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = "12.x" }));
        });
    }

    [Test]
    public async Task Changes_AreNotWrittenOnTheTypingPath()
    {
        var cut = Render();
        Add(cut, Curl);

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");

        Assert.That(UnfinishedWorkout.Saves, Is.Zero, "the write waits for the window or a flush");
        await Flush();
        Assert.That(UnfinishedWorkout.Saves, Is.EqualTo(1));
    }

    [Test]
    public async Task Open_WithNoChange_KeepsNothing()
    {
        Render();

        await Flush();

        Assert.That(UnfinishedWorkout.Saves, Is.Zero);
        Assert.That(UnfinishedWorkout.Stored, Is.Null);
    }

    [Test]
    public async Task EveryExerciseRemoved_KeepsNothing()
    {
        var cut = Render();
        Add(cut, Curl);
        await Flush();
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);

        cut.Find("[data-testid=exercise-1-remove]").Click();
        await Flush();

        Assert.That(UnfinishedWorkout.Stored, Is.Null);
    }

    [Test]
    public async Task Resume_RestoresTheStartTimerExercisesNotesAndValuesAsTyped()
    {
        var start = Now.AddHours(-26);
        UnfinishedWorkout.Stored = new UnfinishedWorkout(start, "Slept badly",
        [
            new UnfinishedExercise(Curl, "Strict form", false,
            [
                new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = "12.5" }),
                new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "abc", ["weight"] = "" }),
            ]),
            new UnfinishedExercise(Treadmill, null, true, [new UnfinishedEntry(new Dictionary<string, string?> { ["duration"] = "0:20:5" })]),
        ]);

        var cut = Render(resume: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Mon 19 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 26:00:00"));
            Assert.That(cut.FindAll(".card > .header > .title").Select(n => n.TextContent), Is.EqualTo(new[] { "Treadmill", "Dumbbell Curl" }));
            Assert.That(cut.Find("[data-testid=workout-note-open]").TextContent, Does.Contain("Slept badly"));
            Assert.That(cut.Find("[data-testid=exercise-2-set-1]").TextContent, Does.Contain("8"));
            Assert.That(cut.Find("[data-testid=exercise-2-set-2-reps]").GetAttribute("value"), Is.EqualTo("abc"), "the last set is open with its half-typed value");
            Assert.That(cut.FindAll("[data-testid=exercise-1-duration-mm]"), Is.Empty, "the done treadmill shows no fields");
        });
    }

    [Test]
    public void Resume_TheTimerKeepsCountingFromTheOriginalStart()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-90));
        var cut = Render(resume: true);

        clock.Advance(TimeSpan.FromSeconds(30));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 1:30:30")));
    }

    [Test]
    public async Task Resume_WritesNothingUntilSomethingChanges()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-5));
        var cut = Render(resume: true);

        await Flush();
        Assert.That(UnfinishedWorkout.Saves, Is.Zero);

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("9");
        await Flush();

        Assert.That(UnfinishedWorkout.Saves, Is.EqualTo(1));
        Assert.That(UnfinishedWorkout.Stored!.StartedAt, Is.EqualTo(Now.AddMinutes(-5)), "the original start is kept");
    }

    [Test]
    public void Resume_NothingKept_StartsANewWorkout()
    {
        var cut = Render(resume: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Wed 21 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 0:00:00"));
            Assert.That(cut.FindAll(".card"), Is.Empty);
        });
    }

    [Test]
    public async Task Resume_ReadFails_LeavesWithoutStartingAndNeverOverwritesTheKeptWorkout()
    {
        var kept = Stored(Now.AddMinutes(-5));
        UnfinishedWorkout.Stored = kept;
        UnfinishedWorkout.FailReads = true;
        var left = 0;

        var cut = Render(resume: true, discarded: () => left++);
        await Flush();

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=screen-subtitle]"), Is.Empty, "no workout is started");
            Assert.That(UnfinishedWorkout.Saves, Is.Zero);
            Assert.That(UnfinishedWorkout.Stored, Is.SameAs(kept));
        });
    }

    [Test]
    public async Task MarkingAnExerciseDoneAndOpeningItAgain_IsKept()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-5));
        var cut = Render(resume: true);

        cut.Find("[data-testid=exercise-1-done]").Click();
        await Flush();
        var done = UnfinishedWorkout.Stored!.Exercises.Single().IsDone;
        cut.Find("[data-testid=exercise-1-edit]").Click();
        await Flush();

        Assert.That((done, UnfinishedWorkout.Stored!.Exercises.Single().IsDone), Is.EqualTo((true, false)));
    }

    [Test]
    public async Task Save_StopsTheAutosaveSoNothingIsWrittenAfterIt()
    {
        var cut = Render();
        Add(cut, Curl);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.5");

        cut.Find("[data-testid=save-workout]").Click();
        await Flush();

        Assert.That(UnfinishedWorkout.Saves, Is.Zero);
        Assert.That(UnfinishedWorkout.Stored, Is.Null);
    }

    [Test]
    public async Task Save_Fails_TheAutosaveStartsAgainWithTheWorkout()
    {
        var cut = Render();
        Add(cut, Curl);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.5");
        FailingSave.Fail = true;

        cut.Find("[data-testid=save-workout]").Click();
        await Flush();

        Assert.That(UnfinishedWorkout.Stored!.Exercises.Single().Entries.Single().Values, Is.EqualTo(new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = "12.5" }));
    }

    [Test]
    public async Task Discard_Confirmed_DeletesTheKeptWorkoutAndDropsWhatWasQueued()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-5));
        var discarded = 0;
        var cut = Render(resume: true, discarded: () => discarded++);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("9");

        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await Flush();

        Assert.Multiple(() =>
        {
            Assert.That(UnfinishedWorkout.Stored, Is.Null);
            Assert.That(UnfinishedWorkout.Saves, Is.Zero);
            Assert.That(discarded, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Discard_Confirmed_DeleteFails_StaysAndKeepsTheWorkoutStored()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-5));
        var discarded = 0;
        var cut = Render(resume: true, discarded: () => discarded++);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("9");
        UnfinishedWorkout.FailWrites = true;

        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await Flush();

        Assert.That(discarded, Is.Zero);
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
        UnfinishedWorkout.FailWrites = false;
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await Flush();
        Assert.That((discarded, UnfinishedWorkout.Stored), Is.EqualTo((1, (UnfinishedWorkout?)null)), "the prompt can confirm again");
    }

    [Test]
    public async Task Discard_Cancelled_KeepsTheKeptWorkout()
    {
        UnfinishedWorkout.Stored = Stored(Now.AddMinutes(-5));
        var cut = Render(resume: true);

        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        await Flush();

        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
    }

    [Test]
    public async Task AFailedWrite_KeepsTheWorkoutOnScreenAndTheNextChangeWritesAgain()
    {
        UnfinishedWorkout.FailWrites = true;
        var cut = Render();
        Add(cut, Curl);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");

        await Flush();

        Assert.That(cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value"), Is.EqualTo("8"));
        Assert.That(UnfinishedWorkout.Stored, Is.Null);

        UnfinishedWorkout.FailWrites = false;
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("9");
        await Flush();

        Assert.That(UnfinishedWorkout.Stored!.Exercises.Single().Entries.Single().Values["reps"], Is.EqualTo("9"));
    }

    private FailingSaveService FailingSave => (FailingSaveService)Context.Services.GetRequiredService<IWorkoutService>();

    private IRenderedComponent<NewWorkoutPage> Render(bool resume = false, Action? discarded = null)
    {
        var inner = new SampleWorkoutService(clock, Catalog);
        Context.Services.AddSingleton<IWorkoutService>(new FailingSaveService(inner));
        return Context.Render<NewWorkoutPage>(p => p
            .Add(x => x.Resume, resume)
            .Add(x => x.OnDiscarded, discarded ?? (() => { }))
            .Add(x => x.OnSaved, _ => { }));
    }

    private Task Flush() => Context.Services.GetRequiredService<UnfinishedAutosaver>().FlushAsync();

    /// <summary>A Curl with one set typed.</summary>
    private static UnfinishedWorkout Stored(DateTimeOffset start) =>
        new(start, null, [new UnfinishedExercise(Curl, null, false, [new UnfinishedEntry(new Dictionary<string, string?> { ["reps"] = "8", ["weight"] = "12.5" })])]);

    private static void Add(IRenderedComponent<NewWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=add-exercise-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=add-exercise-row-{exerciseId}]").Click();
    }

    private static void TypeNote(IRenderedComponent<NewWorkoutPage> cut, int? number, string text)
    {
        cut.Find(number is { } n ? $"[data-testid=exercise-{n}-add-note]" : "[data-testid=workout-note-add]").Click();
        cut.Find(number is { } m ? $"[data-testid=exercise-{m}-note-input]" : "[data-testid=workout-note-input]").Input(text);
        cut.Find(number is { } k ? $"[data-testid=exercise-{k}-add-note]" : "[data-testid=workout-note-add]").Click();
    }

    /// <summary>The sample workout service, whose <see cref="SaveNewAsync"/> fails while <see cref="Fail"/> is set.</summary>
    private sealed class FailingSaveService(SampleWorkoutService inner) : IWorkoutService
    {
        public bool Fail { get; set; }

        public WorkoutDraft StartNew() => inner.StartNew();

        public WorkoutDraft Resume(DateTimeOffset startedAt) => inner.Resume(startedAt);

        public TimeSpan Elapsed(WorkoutDraft draft) => inner.Elapsed(draft);

        public WorkoutErrors Validate(string? note, IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(note, exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, string? note, IReadOnlyList<WorkoutExercise> exercises) =>
            Fail ? Task.FromResult<SaveResult>(new WorkoutErrors([WorkoutError.SaveFailed], [])) : inner.SaveNewAsync(draft, note, exercises);

        public Task<WorkoutResult> GetAsync(int workoutId) => inner.GetAsync(workoutId);

        public Task<SaveResult> SaveAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, note, exercises);

        public Task<WorkoutDelete> DeleteAsync(int workoutId) => inner.DeleteAsync(workoutId);
    }
}
