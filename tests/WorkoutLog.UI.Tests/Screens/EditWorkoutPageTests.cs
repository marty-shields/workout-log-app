using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class EditWorkoutPageTests : ComponentTestBase
{
    private const int Id = 7;

    /// <summary>Tue 20 Oct 2026 23:30 UTC; Wed 21 Oct 00:30 in the test's UTC+1 zone.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    /// <summary>Squat with two sets, a carry with two sets, and a treadmill entry; 1:04:37 long.</summary>
    private static readonly Workout Saved = new(Id, Start, Start + new TimeSpan(1, 4, 37),
    [
        new StrengthExercise(6, [new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]),
        new StrengthExercise(187, [new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 40m), new StrengthSet(null, 32m, TimeSpan.FromSeconds(50), 40m)]),
        new CardioExercise(300, new CardioEntry(new TimeSpan(0, 20, 0), 3.2m, null, null, 1.5m, null, 9.6m, null)),
    ]);

    private static readonly WorkoutResult Loaded = new(Saved, WorkoutLoad.Loaded);

    private FakeWorkoutService workouts = null!;
    private FakeLogger<EditWorkoutPage> logger = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider();
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        workouts = new FakeWorkoutService(new SampleWorkoutService(clock, Catalog));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        logger = new FakeLogger<EditWorkoutPage>();
        Context.Services.AddSingleton<ILogger<EditWorkoutPage>>(logger);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerUnderWorkout()
    {
        var cut = RenderPage();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workout"));
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=save-workout]"), Is.Empty);
            Assert.That(workouts.Loads, Is.EqualTo(new[] { Id }));
        });
    }

    [Test]
    public void Loaded_ShowsTheStartDateAndTotalTimeInThePhonesTimeZone()
    {
        var cut = RenderLoaded();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Wed 21 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Total 1:04:37"));
            Assert.That(cut.FindAll("[data-testid=delete-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Loaded_ShowsTheSavedExercisesAndValuesInOrder()
    {
        var cut = RenderLoaded();

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Barbell Back Squat", "Farmer's Walk", "Treadmill" }));
            Assert.That(Value(cut, "exercise-1-set-1-reps"), Is.EqualTo("8"));
            Assert.That(Value(cut, "exercise-1-set-2-weight"), Is.EqualTo("62.5"));
            Assert.That(Value(cut, "exercise-3-duration-mm"), Is.EqualTo("20"));
            Assert.That(Value(cut, "exercise-3-distance"), Is.EqualTo("3.2"));
            Assert.That(Value(cut, "exercise-3-rate"), Is.EqualTo("9.6"));
        });
    }

    [Test]
    public void Loaded_CarrySetsStartClosed()
    {
        var cut = RenderLoaded();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-2-set-1-open]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-2-set-2-open]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".panel"), Is.Empty);
        });
    }

    [Test]
    public void NotFound_ShowsTheMessage()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(null, WorkoutLoad.NotFound));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-not-found]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=workout-retry]"), Is.Empty);
    }

    [Test]
    public void LoadFails_ShowsTheErrorThenTryAgainReloads()
    {
        var cut = RenderPage();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workout-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Loads, Is.EqualTo(new[] { Id, Id }));
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Is.Empty);
        });

        workouts.Complete(Loaded);

        cut.WaitForAssertion(() => Assert.That(Names(cut), Has.Count.EqualTo(3)));
    }

    [Test]
    public void TryAgain_FailsAgain_ShowsTheErrorAndFocusesTryAgainAgain()
    {
        var cut = RenderPage();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workout-retry]").Click();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
        Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1));
        Focuses()[^1].Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=workout-retry]"));
    }

    [Test]
    public void LoadFails_LogsNothingInTheUi()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public void UnknownExercise_LogsAnErrorAndShowsTheLoadFailure()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(Saved with { Exercises = [Saved.Exercises[0], new StrengthExercise(9999, [])] }, WorkoutLoad.Loaded));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout 7 failed: exercise 9999 is unknown or the wrong type"),
        }));
    }

    [Test]
    public void WrongType_LogsAnErrorAndShowsTheLoadFailure()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(Saved with { Exercises = [new CardioExercise(6, new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, null, null, null, null))] }, WorkoutLoad.Loaded));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout 7 failed: exercise 6 is unknown or the wrong type"),
        }));
    }

    [Test]
    public void Loaded_LogsNothingInTheUi()
    {
        RenderLoaded();

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public void Back_Loaded_DoesNothing()
    {
        var backs = 0;
        var cut = RenderLoaded(onBack: () => backs++);

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.Zero);
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [Test]
    public async Task AndroidBack_Loaded_IsTakenAndDoesNothing()
    {
        var backs = 0;
        var cut = RenderLoaded(onBack: () => backs++);

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(BackButton.HasHandler, Is.True);
            Assert.That(backs, Is.Zero);
            Assert.That(Names(cut), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Back_WhileLoading_RaisesOnBack()
    {
        var backs = 0;
        var cut = RenderPage(onBack: () => backs++);

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    [TestCase(WorkoutLoad.NotFound)]
    [TestCase(WorkoutLoad.Failed)]
    public async Task Back_NotLoaded_RaisesOnBack(WorkoutLoad status)
    {
        var backs = 0;
        var cut = RenderPage(onBack: () => backs++);
        workouts.Complete(new WorkoutResult(null, status));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-loading]"), Is.Empty));

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(2));
    }

    [Test]
    public void Delete_DoesNothing()
    {
        var backs = 0;
        var cut = RenderLoaded(onBack: () => backs++);

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public void Save_Unchanged_SavesTheSameRecordsUnderTheIdAndRaisesOnSaved()
    {
        var saved = 0;
        var cut = RenderLoaded(onSaved: () => saved++);

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves.Single().WorkoutId, Is.EqualTo(Id));
            Assert.That(workouts.Saves.Single().Exercises, Is.EqualTo(Saved.Exercises).Using<WorkoutExercise>(SameExercise));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public void Save_AfterTheIdParameterChanges_SavesUnderTheLoadedId()
    {
        var cut = RenderLoaded();
        cut.Render(p => p.Add(x => x.WorkoutId, Id + 1));

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves.Single().WorkoutId, Is.EqualTo(Id));
    }

    [Test]
    public void Save_Edited_SavesTheChanges()
    {
        var cut = RenderLoaded();
        cut.Find("[data-testid=exercise-1-set-2-weight]").Input("65");
        cut.Find("[data-testid=exercise-1-add-set]").Click();
        cut.Find("[data-testid=exercise-1-set-3-reps]").Input("5");
        cut.Find("[data-testid=exercise-2-remove]").Click();

        cut.Find("[data-testid=save-workout]").Click();

        var exercises = workouts.Saves.Single().Exercises;
        Assert.Multiple(() =>
        {
            Assert.That(exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { 6, 300 }));
            Assert.That(((StrengthExercise)exercises[0]).Sets, Is.EqualTo(new[]
            {
                new StrengthSet(8, 60m, null, null), new StrengthSet(6, 65m, null, null), new StrengthSet(5, null, null, null),
            }));
        });
    }

    [Test]
    public void Save_Invalid_ShowsTheErrorsAndDoesNotSave()
    {
        var saved = 0;
        var cut = RenderLoaded(onSaved: () => saved++);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-1-set-1-error]").TextContent, Does.Contain("Enter reps"));
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(saved, Is.Zero);
        });
    }

    [Test]
    public void Save_WriteFails_ShowsSaveFailedFocusesSaveAndKeepsTheWorkout()
    {
        var saved = 0;
        var cut = RenderLoaded(onSaved: () => saved++);
        var save = cut.Find("[data-testid=save-workout]").GetAttribute("blazor:elementreference");
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors(WorkoutError.SaveFailed, [])));

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't save the workout. Try again."));
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(saved, Is.Zero);
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(save)));
    }

    [Test]
    public void Save_WhileSaving_IsIgnored()
    {
        var saved = 0;
        var cut = RenderLoaded(onSaved: () => saved++);
        var pending = new TaskCompletionSource<SaveResult>();
        workouts.Save = () => pending.Task;

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();
        cut.InvokeAsync(() => pending.SetResult(new SaveResult(Id, new WorkoutErrors(null, []))));

        cut.WaitForAssertion(() => Assert.That(saved, Is.EqualTo(1)));
        Assert.That(workouts.Saves, Has.Count.EqualTo(1));
    }

    [Test]
    public void Save_AfterASuccessfulSave_IsIgnored()
    {
        var saved = 0;
        var cut = RenderLoaded(onSaved: () => saved++);

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public void Pick_AddsAtTheBottomWithOneEmptySet()
    {
        var cut = RenderLoaded();

        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Barbell Back Squat", "Farmer's Walk", "Treadmill", "Dumbbell Curl" }));
            Assert.That(Value(cut, "exercise-4-set-1-reps"), Is.Null.Or.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-picker]"), Is.Empty);
        });
    }

    [Test]
    public void PickAgain_Strength_AddsASetToItsCard()
    {
        var cut = RenderLoaded();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-3]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Pick_ClearsTheWorkoutError()
    {
        var cut = RenderLoaded();
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors(WorkoutError.SaveFailed, [])));
        cut.Find("[data-testid=save-workout]").Click();

        Add(cut, 132);

        Assert.That(cut.FindAll("[data-testid=workout-error]"), Is.Empty);
    }

    [Test]
    public async Task AndroidBack_ClosesThePickerAndStays()
    {
        var backs = 0;
        var cut = RenderLoaded(onBack: () => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-picker]"), Is.Empty);
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(backs, Is.Zero);
        });
    }

    private static IReadOnlyList<string> Names(IRenderedComponent<EditWorkoutPage> cut) =>
        cut.FindAll(".card .name").Select(n => n.TextContent).ToList();

    private static string? Value(IRenderedComponent<EditWorkoutPage> cut, string testId) =>
        cut.Find($"[data-testid={testId}]").GetAttribute("value");

    /// <summary>Same type, exercise id and sets or entry; the records hold their sets in a list, which compares by reference.</summary>
    private static bool SameExercise(WorkoutExercise x, WorkoutExercise y) => (x, y) switch
    {
        (StrengthExercise a, StrengthExercise b) => a.ExerciseId == b.ExerciseId && a.Sets.SequenceEqual(b.Sets),
        (CardioExercise a, CardioExercise b) => a == b,
        _ => false,
    };

    /// <summary>The latest focus call; the picker search and added exercises each take focus.</summary>
    private JSRuntimeInvocation LastFocus() => Focuses()[^1];

    private IReadOnlyList<JSRuntimeInvocation> Focuses() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    private IRenderedComponent<EditWorkoutPage> RenderPage(Action? onBack = null, Action? onSaved = null) =>
        Context.Render<EditWorkoutPage>(p => p
            .Add(x => x.WorkoutId, Id)
            .Add(x => x.OnBack, onBack ?? (() => { }))
            .Add(x => x.OnSaved, onSaved ?? (() => { })));

    private IRenderedComponent<EditWorkoutPage> RenderLoaded(Action? onBack = null, Action? onSaved = null)
    {
        var cut = RenderPage(onBack, onSaved);
        workouts.Complete(Loaded);
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=save-workout]"), Has.Count.EqualTo(1)));
        return cut;
    }

    /// <summary>Opens the picker, searches for the exercise by name and picks it.</summary>
    private static void Add(IRenderedComponent<EditWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=picker-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=picker-item-{exerciseId}]").Click();
    }

    /// <summary>Checks with the inner service. Each load waits until the test completes it; saves are recorded.</summary>
    private sealed class FakeWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        private TaskCompletionSource<WorkoutResult> pending = new();

        /// <summary>The id asked for by each load.</summary>
        public List<int> Loads { get; } = [];

        public List<(int WorkoutId, IReadOnlyList<WorkoutExercise> Exercises)> Saves { get; } = [];

        /// <summary>When set, <see cref="SaveAsync"/> returns its result instead of the inner service's.</summary>
        public Func<Task<SaveResult>>? Save { get; set; }

        /// <summary>Completes the latest load.</summary>
        public void Complete(WorkoutResult result) => pending.SetResult(result);

        public WorkoutDraft StartNew() => inner.StartNew();

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, exercises);

        public Task<WorkoutResult> GetAsync(int workoutId)
        {
            Loads.Add(workoutId);
            pending = new TaskCompletionSource<WorkoutResult>();
            return pending.Task;
        }

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises)
        {
            Saves.Add((workoutId, exercises));
            return Save?.Invoke() ?? inner.SaveAsync(workoutId, exercises);
        }
    }
}
