using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>The Discard prompt and its run-once rule, which New and Edit Workout share through <see cref="WorkoutEditor"/>.</summary>
[TestFixture(WorkoutMode.New)]
[TestFixture(WorkoutMode.Edit)]
public class WorkoutDiscardTests(WorkoutMode mode) : ComponentTestBase
{
    private const int Id = 7;

    private static readonly DateTimeOffset Start = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    /// <summary>Squat with one set.</summary>
    private static readonly Workout Saved = new(Id, Start, Start + new TimeSpan(1, 4, 37),
        [new StrengthExercise(6, [new StrengthSet(8, 60m, null, null)])]);

    private StubWorkoutService workouts = null!;
    private FakeLogger logger = null!;
    private int leaves;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider(Start);
        Context.Services.AddSingleton<TimeProvider>(clock);
        workouts = new StubWorkoutService(new SampleWorkoutService(clock, Catalog));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        var newLogger = new FakeLogger<NewWorkoutPage>();
        var editLogger = new FakeLogger<EditWorkoutPage>();
        Context.Services.AddSingleton<ILogger<NewWorkoutPage>>(newLogger);
        Context.Services.AddSingleton<ILogger<EditWorkoutPage>>(editLogger);
        logger = mode == WorkoutMode.New ? newLogger : editLogger;
        leaves = 0;
    }

    [Test]
    public async Task AndroidBack_WithThePromptOpen_CancelsIt()
    {
        var cut = Render(() => leaves++);
        Edit(cut);
        cut.Find("[data-testid=back]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value"), Is.EqualTo("9"));
            Assert.That(leaves, Is.Zero);
        });
    }

    [TestCase("workout-note", "workout-note-add")]
    [TestCase("exercise-1-note", "exercise-1-add-note")]
    public void NoteEdit_BackAsksAndCancelKeepsTheNote(string note, string addNote)
    {
        var cut = Render(() => leaves++);
        cut.Find($"[data-testid={addNote}]").Click();
        cut.Find($"[data-testid={note}-input]").Input("Slept badly");
        cut.Find($"[data-testid={note}-input]").Blur();

        cut.Find("[data-testid=back]").Click();
        var asked = cut.FindAll("[data-testid=confirm-dialog]").Count;
        cut.Find("[data-testid=confirm-dialog-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(asked, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(cut.Find($"[data-testid={note}-open] .text").TextContent, Is.EqualTo("Slept badly"));
            Assert.That(leaves, Is.Zero);
        });
    }

    [TestCase("workout-note", "workout-note-add")]
    [TestCase("exercise-1-note", "exercise-1-add-note")]
    public void NoteEdit_Discard_LogsNoNoteTextAndRaisesOnce(string note, string addNote)
    {
        var cut = Render(() => leaves++);
        cut.Find($"[data-testid={addNote}]").Click();
        cut.Find($"[data-testid={note}-input]").Input("Slept badly");
        cut.Find($"[data-testid={note}-input]").Blur();
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(leaves, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger).Select(e => e.Message), Has.All.Not.Contain("Slept badly"));
        });
    }

    [Test]
    public async Task Discard_Twice_LogsAndRaisesOnceAndBackIsIgnored()
    {
        var cut = Render(() => leaves++);
        Edit(cut);
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);
        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(leaves, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Back_WhileSaving_IsIgnored(bool edited)
    {
        if (!edited && mode == WorkoutMode.New)
        {
            Assert.Ignore("A new workout is always edited once an exercise is added.");
        }

        var cut = Render(() => leaves++);
        workouts.Save = () => new TaskCompletionSource<SaveResult>().Task;
        if (edited)
        {
            Edit(cut);
        }
        else
        {
            MakeValid(cut);
        }

        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(leaves, Is.Zero);
        });
    }

    [Test]
    public void Discard_WhileSaving_ClosesThePromptAndDoesNothing()
    {
        var cut = Render(() => leaves++);
        workouts.Save = () => new TaskCompletionSource<SaveResult>().Task;
        Edit(cut);
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(leaves, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void Discard_AfterASuccessfulSave_DoesNothing()
    {
        var cut = Render(() => leaves++);
        Edit(cut);
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(leaves, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
            Assert.That(workouts.Saves, Is.EqualTo(1));
        });
    }

    [Test]
    public void Discard_HandlerFails_ShowsTheErrorScreen()
    {
        var cut = Render(() => throw new InvalidOperationException("Navigation failed"));
        Edit(cut);
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
    }

    private IRenderedComponent<AppErrorBoundary> Render(Action onLeave)
    {
        var cut = Context.Render<AppErrorBoundary>(p =>
        {
            p.Add(x => x.OnRestart, () => { });
            if (mode == WorkoutMode.New)
            {
                p.AddChildContent<NewWorkoutPage>(page => page.Add(x => x.OnDiscarded, onLeave));
            }
            else
            {
                p.AddChildContent<EditWorkoutPage>(page => page.Add(x => x.WorkoutId, Id).Add(x => x.OnBack, onLeave));
            }
        });
        if (mode == WorkoutMode.New)
        {
            cut.Find("[data-testid=add-exercise]").Click();
            cut.Find("[data-testid=exercises-search]").Input(Catalog.All.Single(e => e.Id == 132).Name);
            cut.Find("[data-testid=exercises-row-132]").Click();
        }
        else
        {
            cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=save-workout]"), Has.Count.EqualTo(1)));
            cut.Find("[data-testid=exercise-1-edit]").Click();
        }

        return cut;
    }

    /// <summary>Types a valid set without changing what the Edit page loaded.</summary>
    private static void MakeValid(IRenderedComponent<AppErrorBoundary> cut)
    {
        if (cut.FindAll("[data-testid=exercise-1-set-1-reps]").Count > 0 && cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value") == "")
        {
            Type(cut, "8");
        }
    }

    /// <summary>Changes the set, so there is something to discard.</summary>
    private static void Edit(IRenderedComponent<AppErrorBoundary> cut) => Type(cut, "9");

    private static void Type(IRenderedComponent<AppErrorBoundary> cut, string reps)
    {
        if (cut.FindAll("[data-testid=exercise-1-set-1-edit]").Count > 0)
        {
            cut.Find("[data-testid=exercise-1-set-1-edit]").Click();
        }

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input(reps);
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
    }

    /// <summary>Passes through to <paramref name="inner"/>, recording saves; a load returns <see cref="Saved"/>.</summary>
    private sealed class StubWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        public int Saves { get; private set; }

        /// <summary>When set, a save returns its result instead of the inner service's.</summary>
        public Func<Task<SaveResult>>? Save { get; set; }

        public WorkoutDraft StartNew() => inner.StartNew();

        public TimeSpan Elapsed(WorkoutDraft draft) => inner.Elapsed(draft);

        public WorkoutErrors Validate(string? note, IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(note, exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, string? note, IReadOnlyList<WorkoutExercise> exercises)
        {
            Saves++;
            return Save?.Invoke() ?? inner.SaveNewAsync(draft, note, exercises);
        }

        public Task<WorkoutResult> GetAsync(int workoutId) => Task.FromResult(new WorkoutResult(Saved, WorkoutLoad.Loaded));

        public Task<SaveResult> SaveAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises)
        {
            Saves++;
            return Save?.Invoke() ?? inner.SaveAsync(workoutId, note, exercises);
        }

        public Task<WorkoutDelete> DeleteAsync(int workoutId) => inner.DeleteAsync(workoutId);
    }
}
