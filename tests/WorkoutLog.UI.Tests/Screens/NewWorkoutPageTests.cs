using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class NewWorkoutPageTests : ComponentTestBase
{
    /// <summary>Tue 20 Oct 2026 23:30 UTC; Wed 21 Oct 00:30 in the test's UTC+1 zone.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    private RecordingClock clock = null!;

    [SetUp]
    public void SetUp()
    {
        clock = new RecordingClock(Start);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    [Test]
    public void Open_StartsOneWorkout()
    {
        var workouts = UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();

        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.RenderCount, Is.GreaterThan(1)));
        Assert.That(workouts.Started, Is.EqualTo(1));
    }

    [Test]
    public void Open_ShowsTheStartDateInThePhonesTimeZone()
    {
        UseWorkouts(clock);

        var cut = Context.Render<NewWorkoutPage>();

        Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Wed 21 Oct 2026"));
    }

    [Test]
    public void Timer_CountsFromTheStart()
    {
        UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));

        clock.Advance(TimeSpan.FromSeconds(1));
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:01")));

        clock.Advance(new TimeSpan(1, 2, 3));
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 1:02:04")));
    }

    [Test]
    public void ClockBeforeTheStart_ShowsZero()
    {
        UseWorkouts(new FakeTimeProvider(Start.AddMinutes(5)));

        var cut = Context.Render<NewWorkoutPage>();
        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.RenderCount, Is.GreaterThan(1)));
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));
    }

    [Test]
    public async Task Back_RaisesOnBack()
    {
        UseWorkouts(clock);
        var backs = 0;
        var cut = Context.Render<NewWorkoutPage>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(2));
    }

    [Test]
    public async Task Disposed_DisposesTheTimer()
    {
        UseWorkouts(clock);
        Context.Render<NewWorkoutPage>();
        Assert.That(clock.Timers, Has.Count.EqualTo(1));
        Assert.That(clock.Timers[0].Disposed, Is.False);

        await Context.DisposeComponentsAsync();

        Assert.That(clock.Timers[0].Disposed, Is.True);
    }

    [Test]
    public void TickFails_ShowsTheErrorScreen()
    {
        UseWorkouts(clock);
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent<NewWorkoutPage>());

        clock.FailReads = true;
        clock.Advance(TimeSpan.FromSeconds(1));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public void AddExercise_OpensThePickerWithTheFullList()
    {
        var cut = RenderPage();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=picker-search]").GetAttribute("value"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=picker-group-]"), Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid^=picker-item-]"), Has.Count.EqualTo(Catalog.All.Count));
        });
    }

    [Test]
    public void Search_FiltersByNickname()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=picker-search]").Input("gravitron");

        Assert.That(cut.FindAll("[data-testid^=picker-item-]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "picker-item-218" }));
    }

    [Test]
    public void Pick_StrengthAddsAtTheBottomWithOneEmptySetAndClosesThePicker()
    {
        var cut = RenderPage();
        Add(cut, 132);

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl", "Barbell Back Squat" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-2-set-][data-testid$=-reps]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=exercise-2-set-1-reps]").GetAttribute("value"), Is.Null.Or.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-picker]"), Is.Empty);
        });
    }

    [Test]
    public void Pick_CardioAddsItsEmptyEntry()
    {
        var cut = RenderPage();

        Add(cut, 305);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-duration-h]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=exercise-1]").TextContent, Does.Contain("Damper"));
            Assert.That(cut.FindAll("[data-testid=exercise-1-add-set]"), Is.Empty);
        });
    }

    [Test]
    public void PickAgain_Strength_AddsASetToItsCardAndFocusesIt()
    {
        var cut = RenderPage();
        Add(cut, 132);
        Add(cut, 6);

        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl", "Barbell Back Squat" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Has.Count.EqualTo(1));
        });
        cut.WaitForAssertion(() => LastFocus().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-reps]")));
    }

    [Test]
    public void PickAgain_Cardio_FocusesItsEntryWithoutAdding()
    {
        var cut = RenderPage();
        Add(cut, 305);
        Add(cut, 132);

        Add(cut, 305);

        Assert.That(Names(cut), Is.EqualTo(new[] { "Rowing Machine", "Dumbbell Curl" }));
        cut.WaitForAssertion(() => LastFocus().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-duration-h]")));
    }

    [Test]
    public void PickAgain_AfterRemoving_AddsItAgain()
    {
        var cut = RenderPage();
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-remove]").Click();

        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Is.Empty);
        });
    }

    [Test]
    public void Pick_FocusesTheNewExercisesFirstField()
    {
        var cut = RenderPage();

        Add(cut, 187);

        Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2)[1].Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-1-distance]"));
    }

    [Test]
    public void AddSet_AddsASet()
    {
        var cut = RenderPage();
        Add(cut, 132);

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Remove_RemovesOnlyThatExerciseAndKeepsTheOrder()
    {
        var cut = RenderPage();
        Add(cut, 132);
        Add(cut, 6);
        Add(cut, 305);

        cut.Find("[data-testid=exercise-2-remove]").Click();

        Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl", "Rowing Machine" }));
    }

    [Test]
    public void RemoveLast_ShowsTheEmptyState()
    {
        var cut = RenderPage();
        Add(cut, 132);

        cut.Find("[data-testid=exercise-1-remove]").Click();

        Assert.That(cut.FindAll("[data-testid=no-exercises]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_ClosesThePickerAndStays()
    {
        var backs = 0;
        var cut = RenderPage(() => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-picker]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public void PickerBackArrow_ClosesThePickerAndStays()
    {
        var backs = 0;
        var cut = RenderPage(() => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=picker-back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-picker]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public void Reopen_ClearsTheSearch()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=picker-search]").Input("zumba");
        cut.Find("[data-testid=picker-back]").Click();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=picker-search]").GetAttribute("value"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=picker-item-]"), Has.Count.EqualTo(Catalog.All.Count));
        });
    }

    [Test]
    public void TimerTick_DoesNotRunTheSearchAgain()
    {
        var catalog = new CountingCatalog(Catalog);
        Context.Services.AddSingleton<IExerciseCatalog>(catalog);
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=picker-search]").Input("curl");
        var searches = catalog.Searches;

        clock.Advance(TimeSpan.FromSeconds(3));

        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:03")));
        Assert.That(catalog.Searches, Is.EqualTo(searches));
    }

    [Test]
    public void LongSearch_IsCutToTheMaxLength()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=picker-search]").Input("pec" + new string('x', 100_000));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=picker-search]").GetAttribute("value"), Has.Length.EqualTo(ExerciseCatalog.MaxQueryLength));
            Assert.That(cut.FindAll("[data-testid=picker-none]"), Has.Count.EqualTo(1));
        });
    }

    private static string Subtitle(IRenderedComponent<NewWorkoutPage> cut) => cut.Find("[data-testid=screen-subtitle]").TextContent;

    private static IEnumerable<string> Names(IRenderedComponent<NewWorkoutPage> cut) => cut.FindAll(".card .name").Select(n => n.TextContent);

    /// <summary>The latest focus call; the picker search and added exercises each take focus.</summary>
    private JSRuntimeInvocation LastFocus() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"][^1];

    private IRenderedComponent<NewWorkoutPage> RenderPage(Action? onBack = null)
    {
        UseWorkouts(clock);
        return Context.Render<NewWorkoutPage>(p => p.Add(x => x.OnBack, onBack ?? (() => { })));
    }

    /// <summary>Opens the picker, searches for the exercise by name and picks it.</summary>
    private static void Add(IRenderedComponent<NewWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=picker-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=picker-item-{exerciseId}]").Click();
    }

    /// <summary>Registers a workout service whose drafts start at <paramref name="draftClock"/>'s current time.</summary>
    private CountingWorkoutService UseWorkouts(TimeProvider draftClock)
    {
        var workouts = new CountingWorkoutService(new SampleWorkoutService(draftClock));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        return workouts;
    }

    /// <summary>A <see cref="FakeTimeProvider"/> that keeps the timers it creates and can fail reads of the time.</summary>
    private sealed class RecordingClock(DateTimeOffset start) : FakeTimeProvider(start)
    {
        public List<RecordingTimer> Timers { get; } = [];

        /// <summary>When set, <see cref="GetUtcNow"/> throws.</summary>
        public bool FailReads { get; set; }

        public override DateTimeOffset GetUtcNow() =>
            FailReads ? throw new InvalidOperationException("Clock read failed") : base.GetUtcNow();

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new RecordingTimer(base.CreateTimer(callback, state, dueTime, period));
            Timers.Add(timer);
            return timer;
        }
    }

    /// <summary>Records whether it was disposed; the rest is passed to <paramref name="inner"/>.</summary>
    private sealed class RecordingTimer(ITimer inner) : ITimer
    {
        public bool Disposed { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period) => inner.Change(dueTime, period);

        public void Dispose()
        {
            Disposed = true;
            inner.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return inner.DisposeAsync();
        }
    }

    /// <summary>Counts <see cref="Search"/> calls; the rest is passed to <paramref name="inner"/>.</summary>
    private sealed class CountingCatalog(IExerciseCatalog inner) : IExerciseCatalog
    {
        public int Searches { get; private set; }

        public IReadOnlyList<ExerciseDefinition> All => inner.All;

        public IReadOnlyList<ExerciseDefinition> Search(string? query)
        {
            Searches++;
            return inner.Search(query);
        }
    }

    /// <summary>Counts <see cref="StartNew"/> calls; the rest is passed to <paramref name="inner"/>.</summary>
    private sealed class CountingWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        public int Started { get; private set; }

        public WorkoutDraft StartNew()
        {
            Started++;
            return inner.StartNew();
        }

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, exercises);

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, exercises);
    }
}
