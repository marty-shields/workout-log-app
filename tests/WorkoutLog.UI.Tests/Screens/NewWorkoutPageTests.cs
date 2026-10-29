using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class NewWorkoutPageTests : ComponentTestBase
{
    /// <summary>Tue 20 Oct 2026 23:30 UTC; Wed 21 Oct 00:30 in the test's UTC+1 zone.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    private RecordingClock clock = null!;
    private CountingWorkoutService workouts = null!;
    private FakeLogger<NewWorkoutPage> logger = null!;

    [SetUp]
    public void SetUp()
    {
        clock = new RecordingClock(Start);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        logger = new FakeLogger<NewWorkoutPage>();
        Context.Services.AddSingleton<ILogger<NewWorkoutPage>>(logger);
    }

    [Test]
    public async Task Open_StartsOneWorkout()
    {
        var workouts = UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();

        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();

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
    public async Task Timer_CountsFromTheStart()
    {
        UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));

        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:01")));

        clock.Advance(new TimeSpan(1, 2, 3));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 1:02:04")));
    }

    [Test]
    public async Task Timer_ShowsTheServicesElapsedTime()
    {
        var workouts = UseWorkouts(clock);
        var cut = Context.Render<NewWorkoutPage>();

        workouts.ElapsedTime = new TimeSpan(0, 42, 5);
        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:42:05")));
    }

    [Test]
    public async Task ClockBeforeTheStart_ShowsZero()
    {
        UseWorkouts(new FakeTimeProvider(Start.AddMinutes(5)));

        var cut = Context.Render<NewWorkoutPage>();
        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.RenderCount, Is.GreaterThan(1)));
        Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:00"));
    }

    [Test]
    public void Back_ShowsTheDiscardPromptAndFocusesCancel()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=confirm-dialog] h2").TextContent, Is.EqualTo("Discard this workout?"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").TextContent, Is.EqualTo("Discard"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-cancel]").TextContent, Is.EqualTo("Cancel"));
            Assert.That(discards, Is.Zero);
        });
        var cancel = ReferenceId(cut, "confirm-dialog-cancel");
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(cancel)));
    }

    [Test]
    public async Task AndroidBack_ShowsTheDiscardPrompt()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
            Assert.That(discards, Is.Zero);
        });
    }

    [Test]
    public void Back_NoExercises_StillAsks()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
            Assert.That(discards, Is.Zero);
        });
    }

    [Test]
    public void Cancel_ClosesThePromptAndKeepsTheWorkout()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        var focusCalls = Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"].Count;

        cut.Find("[data-testid=confirm-dialog-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"], Has.Count.EqualTo(focusCalls));
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl" }));
            Assert.That(cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value"), Is.EqualTo("8"));
            Assert.That(discards, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task AndroidBack_WithThePromptOpen_CancelsIt()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);
        cut.Find("[data-testid=back]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl" }));
            Assert.That(discards, Is.Zero);
        });
    }

    [Test]
    public void Discard_LogsTheExerciseCountAndRaisesOnDiscarded()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);
        Add(cut, 305);
        cut.Find("[data-testid=exercise-2-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-2-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(discards, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.NewWorkoutDiscarded, LogLevel.Information, "New workout discarded with 2 exercises") }));
            Assert.That(workouts.Saves, Is.Empty);
        });
    }

    [Test]
    public async Task Back_WhileSaving_IsIgnored()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        workouts.SaveNew = () => new TaskCompletionSource<SaveResult>().Task;
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(discards, Is.Zero);
        });
    }

    [Test]
    public async Task Discard_Twice_LogsAndRaisesOnceAndBackIsIgnored()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);
        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(discards, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [Test]
    public void Discard_WhileSaving_ClosesThePromptAndDoesNothing()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        workouts.SaveNew = () => new TaskCompletionSource<SaveResult>().Task;
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(discards, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void Discard_AfterASuccessfulSave_DoesNothing()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(discards, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Discard_NoExercises_LogsZero()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.NewWorkoutDiscarded, LogLevel.Information, "New workout discarded with 0 exercises") }));
    }

    [Test]
    public async Task Back_AfterASuccessfulSave_IsIgnored()
    {
        var discards = 0;
        var cut = RenderPage(() => discards++);
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(discards, Is.Zero);
        });
    }

    [Test]
    public void Back_AfterASaveFails_Asks()
    {
        var cut = RenderPage();
        workouts.SaveNew = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();

        Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Discard_HandlerFails_ShowsTheErrorScreen()
    {
        UseWorkouts(clock);
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent<NewWorkoutPage>(page => page
                .Add(x => x.OnDiscarded, () => throw new InvalidOperationException("Navigation failed"))));
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
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
    public async Task TickFails_ShowsTheErrorScreen()
    {
        UseWorkouts(clock);
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent<NewWorkoutPage>());

        clock.FailReads = true;
        clock.Advance(TimeSpan.FromSeconds(1));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public void AddExercise_OpensTheListWithTheFullCatalog()
    {
        var cut = RenderPage();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercises-filter-all]").HasAttribute("checked"), Is.True);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(Catalog.Search(null).Select(e => $"exercises-row-{e.Id}")));
        });
    }

    [Test]
    public void AddExercise_ShowsTheSpinnerUntilTheLastSessionsLoad()
    {
        LoggedExercises.Hold = true;
        LoggedExercises.Result = new LoggedExercisesResult([new LoggedExercise(6, Start.AddDays(-1), null)], Failed: false);
        var cut = RenderPage();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
        });
        LoggedExercises.Complete();
        cut.WaitForAssertion(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercises-row-6] .detail").TextContent, Does.StartWith("Last "));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .detail"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void AddExercise_LoadFailed_ListsTheCatalogWithNoLastLinesOrMessage()
    {
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: true);
        var cut = RenderPage();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Has.Count.EqualTo(Catalog.All.Count));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .detail"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-error]"), Is.Empty);
        });
    }

    [Test]
    public void AddExercise_EachOpenLoadsTheLastSessions()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-back]").Click();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
    }

    [Test]
    public void Chip_FiltersTheList()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=exercises-filter-cardio]").Change(true);

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(Catalog.Search(null).Where(e => e.Type == ExerciseType.Cardio).Select(e => $"exercises-row-{e.Id}")));
    }

    [Test]
    public void Search_FiltersByNickname()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=exercises-search]").Input("gravitron");

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-218" }));
    }

    [Test]
    public void Pick_StrengthAddsAtTheTopWithOneEmptySetAndClosesTheList()
    {
        var cut = RenderPage();
        Add(cut, 132);

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Barbell Back Squat", "Dumbbell Curl" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-1-set-][data-testid$=-reps]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value"), Is.Null.Or.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-overlay]"), Is.Empty);
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
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("10");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
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
    public void PickAgain_Strength_EmptyLastSet_IsBlockedAndFocusesTheFieldAtFault()
    {
        var cut = RenderPage();
        Add(cut, 132);
        // bUnit drops the reference id from the markup once the input re-renders, so read it first.
        var reps = cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("blazor:elementreference");
        Add(cut, 6);

        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error] p").Select(p => p.TextContent.Trim()), Is.EqualTo(new[] { "Enter reps", "Enter weight" }));
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(reps)));
    }

    [Test]
    public void PickAgain_Cardio_MovesItToTheTopAndFocusesItsEntryWithoutAdding()
    {
        var cut = RenderPage();
        Add(cut, 305);
        var hours = ReferenceId(cut, "exercise-1-duration-h");
        Add(cut, 132);

        Add(cut, 305);

        Assert.That(Names(cut), Is.EqualTo(new[] { "Rowing Machine", "Dumbbell Curl" }));
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(hours)));
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
    public void AddSet_ValidSet_AddsASet()
    {
        var cut = RenderPage();
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("10");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error]"), Is.Empty);
        });
    }

    [Test]
    public void AddSet_EmptySet_IsBlockedAndShowsWhy()
    {
        var cut = RenderPage();
        Add(cut, 132);

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error] p").Select(p => p.TextContent.Trim()), Is.EqualTo(new[] { "Enter reps", "Enter weight" }));
        });
    }

    [Test]
    public void AddSet_AfterTheFix_ErrorsClearAndTheSetIsAdded()
    {
        var cut = RenderPage();
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.255");
        cut.Find("[data-testid=exercise-1-add-set]").Click();

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("10");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.25");
        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2-reps]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void AddSet_Carry_BlockedWithTwoErrors()
    {
        var cut = RenderPage();
        Add(cut, 187);
        cut.Find("[data-testid=exercise-1-set-1-duration-ss]").Input("75");

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-2]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error] p").Select(p => p.TextContent.Trim()),
                Is.EqualTo(new[] { "Enter distance", "Duration minutes and seconds must be 00–59" }));
        });
    }

    [Test]
    public void PickAgain_Cardio_IsNotChecked()
    {
        var cut = RenderPage();
        Add(cut, 305);

        Add(cut, 305);

        Assert.That(cut.FindAll("[data-testid=exercise-1-entry-error] p"), Is.Empty);
    }

    [Test]
    public void Save_NoExercises_ShowsTheWorkoutErrorAndFocusesAddExercise()
    {
        var cut = RenderPage();
        var addExercise = ReferenceId(cut, "add-exercise");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Add at least one exercise"));
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(addExercise)));
    }

    [Test]
    public void Pick_ClearsTheWorkoutError()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=save-workout]").Click();

        Add(cut, 6);

        Assert.That(cut.FindAll("[data-testid=workout-error]"), Is.Empty);
    }

    [Test]
    public async Task Pick_Strength_ShowsNothingWhileLoadingThenLastAndPb()
    {
        var weights = UseWeights();
        var cut = RenderPage();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(weights.Loads, Is.EqualTo(new[] { 6 }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-weights]"), Is.Empty);
        });
        weights.Complete(new WeightHistoryResult(62.5m, 70m, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=exercise-1-weights]").TextContent, Is.EqualTo("Last 62.5kg · PB 70kg")));
    }

    [TestCase(264, 0, 0, null)]
    [TestCase(264, 0, 10, "Last — · PB 10kg")]
    [TestCase(258, 0, 0, null)]
    [TestCase(187, 0, 0, null)]
    [TestCase(6, 0, 0, "Last 0kg · PB 0kg")]
    [TestCase(218, 0, 0, "Last 0kg · PB 0kg")]
    public async Task Pick_ZeroWeights_ShowAsNoneOnBodyweightHoldAndCarry(int exerciseId, int lastKg, int bestKg, string? line)
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, exerciseId);

        weights.Complete(new WeightHistoryResult(lastKg, bestKg, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=exercise-1-weights]").Select(w => w.TextContent), Is.EqualTo(line is null ? [] : new[] { line })));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Pick_NoWeightsOrFailedLoad_ShowsNoLineAndLogsNothing(bool failed)
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, 6);

        weights.Complete(new WeightHistoryResult(null, null, failed));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-weights]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-reps]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public void Pick_Cardio_LoadsNoWeights()
    {
        var weights = UseWeights();
        var cut = RenderPage();

        Add(cut, 300);

        Assert.That(weights.Loads, Is.Empty);
    }

    [Test]
    public async Task PickAgain_DoesNotReloadAndKeepsTheLine()
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, 6);
        weights.Complete(new WeightHistoryResult(60m, 70m, Failed: false));
        await WaitForRendererAsync();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(weights.Loads, Is.EqualTo(new[] { 6 }));
            Assert.That(cut.Find("[data-testid=exercise-1-weights]").TextContent, Is.EqualTo("Last 60kg · PB 70kg"));
        });
    }

    [Test]
    public async Task PickAgain_AfterRemoving_LoadsAgain()
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, 6);
        weights.Complete(new WeightHistoryResult(60m, 70m, Failed: false));
        await WaitForRendererAsync();
        cut.Find("[data-testid=exercise-1-remove]").Click();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(weights.Loads, Is.EqualTo(new[] { 6, 6 }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-weights]"), Is.Empty);
        });
    }

    [Test]
    public async Task LoadFinishingAfterRemove_ShowsNothing()
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, 6);
        cut.Find("[data-testid=exercise-1-remove]").Click();

        weights.Complete(new WeightHistoryResult(60m, 70m, Failed: false));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.Empty);
            Assert.That(cut.FindAll("[data-testid$=-weights]"), Is.Empty);
        });
    }

    [Test]
    public async Task LoadFinishingAfterRemoveAndAddAgain_ShowsOnlyTheNewLoad()
    {
        var weights = UseWeights();
        var cut = RenderPage();
        Add(cut, 6);
        cut.Find("[data-testid=exercise-1-remove]").Click();
        Add(cut, 6);

        weights.Complete(new WeightHistoryResult(50m, 55m, Failed: false), index: 0);
        await WaitForRendererAsync();
        Assert.That(cut.FindAll("[data-testid=exercise-1-weights]"), Is.Empty);

        weights.Complete(new WeightHistoryResult(60m, 70m, Failed: false), index: 1);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=exercise-1-weights]").TextContent, Is.EqualTo("Last 60kg · PB 70kg")));
    }

    [Test]
    public void Save_ExerciseWithNoSets_ShowsWhyAndFocusesItsAddSet()
    {
        var cut = RenderPage();
        Add(cut, 6);
        var addSet = ReferenceId(cut, "exercise-1-add-set");
        cut.Find("[data-testid=exercise-1-set-1-remove]").Click();

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(cut.Find("[data-testid=exercise-1-error]").TextContent, Is.EqualTo("Add at least one set"));
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(addSet)));
    }

    [Test]
    public void Save_ChecksEverySetAndEntryAndFocusesTheFirstError()
    {
        var cut = RenderPage();
        Add(cut, 6);
        Add(cut, 311);
        cut.Find("[data-testid=exercise-1-distance]").Input("5.123");
        var hours = ReferenceId(cut, "exercise-1-duration-h");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-1-entry-error] p").Select(p => p.TextContent.Trim()),
                Is.EqualTo(new[] { "Enter duration", "Distance can have up to 2 decimal places" }));
            Assert.That(cut.FindAll("[data-testid=exercise-2-set-1-error] p").Select(p => p.TextContent.Trim()), Is.EqualTo(new[] { "Enter reps", "Enter weight" }));
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(hours)));
    }

    [Test]
    public void CardioEntry_BeforeTheFirstSave_IsNotChecked()
    {
        var cut = RenderPage();
        Add(cut, 300);

        cut.Find("[data-testid=exercise-1-incline]").Input("1.255");

        Assert.That(cut.FindAll("[data-testid=exercise-1-entry-error] p"), Is.Empty);
    }

    [Test]
    public void Save_CardioEntryWithErrors_IsReCheckedAsYouType()
    {
        var cut = RenderPage();
        Add(cut, 300);
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=exercise-1-duration-mm]").Input("20");
        cut.Find("[data-testid=exercise-1-distance]").Input("3");
        var clearedOnValid = cut.FindAll("[data-testid=exercise-1-entry-error] p").Count;
        foreach (var typed in new[] { "1", "1.", "1.2", "1.25", "1.255" })
        {
            cut.Find("[data-testid=exercise-1-incline]").Input(typed);
        }

        Assert.Multiple(() =>
        {
            Assert.That(clearedOnValid, Is.Zero);
            Assert.That(cut.FindAll("[data-testid=exercise-1-entry-error] p").Select(p => p.TextContent.Trim()),
                Is.EqualTo(new[] { "Incline can have up to 2 decimal places" }));
        });
    }

    [Test]
    public void Save_Valid_SavesTheReadRecordsAndRaisesOnSaved()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("12.5");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
            Assert.That(workouts.Saves[0].Draft, Is.SameAs(workouts.Drafts.Single()));
            var exercise = (StrengthExercise)workouts.Saves[0].Exercises.Single();
            Assert.That(exercise.ExerciseId, Is.EqualTo(132));
            Assert.That(exercise.Sets, Is.EqualTo(new[] { new StrengthSet(8, 12.5m, null, null) }));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public void Save_Invalid_DoesNotSave()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        Add(cut, 132);

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(saved, Is.Zero);
        });
    }

    [Test]
    public void Save_WriteFails_ShowsSaveFailedFocusesSaveAndKeepsTheWorkout()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        var save = ReferenceId(cut, "save-workout");
        workouts.SaveNew = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't save the workout. Try again"));
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl" }));
            Assert.That(cut.Find("[data-testid=exercise-1-set-1-reps]").GetAttribute("value"), Is.EqualTo("8"));
            Assert.That(saved, Is.Zero);
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(save)));
    }

    [Test]
    public void Save_NoWorkoutIdWithAnotherCode_ShowsSaveFailed()
    {
        var cut = RenderPage();
        workouts.SaveNew = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.NoExercises], [])));
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't save the workout. Try again"));
    }

    [Test]
    public async Task Save_WhileSaving_IsIgnored()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        var pending = new TaskCompletionSource<SaveResult>();
        workouts.SaveNew = () => pending.Task;
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();
        await cut.InvokeAsync(() => pending.SetResult(new SaveResult(1, new WorkoutErrors([], []))));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(saved, Is.EqualTo(1)));
        Assert.That(workouts.Saves, Has.Count.EqualTo(1));
    }

    [Test]
    public void Save_AfterASuccessfulSave_IsIgnored()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public void Save_AfterAFailure_SavesAgainAndClearsTheError()
    {
        var saved = 0;
        var cut = RenderPage(onSaved: () => saved++);
        workouts.SaveNew = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        Add(cut, 132);
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-weight]").Input("20");
        cut.Find("[data-testid=save-workout]").Click();
        workouts.SaveNew = null;

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=workout-error]"), Is.Empty);
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public void Remove_RemovesOnlyThatExerciseAndKeepsTheOrder()
    {
        var cut = RenderPage();
        Add(cut, 132);
        Add(cut, 6);
        Add(cut, 305);

        cut.Find("[data-testid=exercise-2-remove]").Click();

        Assert.That(Names(cut), Is.EqualTo(new[] { "Rowing Machine", "Dumbbell Curl" }));
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
    public async Task AndroidBack_ClosesTheListAndStays()
    {
        var backs = 0;
        var cut = RenderPage(() => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-overlay]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public void ListBackArrow_ClosesTheListAndStays()
    {
        var backs = 0;
        var cut = RenderPage(() => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=exercises-back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-overlay]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public void Reopen_ClearsTheSearchAndTheChip()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-search]").Input("zumba");
        cut.Find("[data-testid=exercises-filter-cardio]").Change(true);
        cut.Find("[data-testid=exercises-back]").Click();

        cut.Find("[data-testid=add-exercise]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-filter-all]").HasAttribute("checked"), Is.True);
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Has.Count.EqualTo(Catalog.All.Count));
        });
    }

    [Test]
    public async Task TimerTick_DoesNotRunTheSearchAgain()
    {
        var catalog = new CountingCatalog(Catalog);
        Context.Services.AddSingleton<IExerciseCatalog>(catalog);
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-search]").Input("curl");
        var searches = catalog.Searches;

        clock.Advance(TimeSpan.FromSeconds(3));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Subtitle(cut), Is.EqualTo("Elapsed 0:00:03")));
        Assert.That(catalog.Searches, Is.EqualTo(searches));
    }

    [Test]
    public void LongSearch_IsCutToTheMaxLength()
    {
        var cut = RenderPage();
        cut.Find("[data-testid=add-exercise]").Click();

        cut.Find("[data-testid=exercises-search]").Input("pec" + new string('x', 100_000));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Has.Length.EqualTo(ExerciseCatalog.MaxQueryLength));
            Assert.That(cut.FindAll("[data-testid=exercises-none]"), Has.Count.EqualTo(1));
        });
    }

    /// <summary>The element's reference id. bUnit drops it from the markup once the element re-renders, so read it first.</summary>
    private static string? ReferenceId(IRenderedComponent<NewWorkoutPage> cut, string testId) =>
        cut.Find($"[data-testid={testId}]").GetAttribute("blazor:elementreference");

    private static string Subtitle(IRenderedComponent<NewWorkoutPage> cut) => cut.Find("[data-testid=screen-subtitle]").TextContent;

    private static IEnumerable<string> Names(IRenderedComponent<NewWorkoutPage> cut) => cut.FindAll(".card .name").Select(n => n.TextContent);

    /// <summary>The latest focus call; the list search and added exercises each take focus.</summary>
    private JSRuntimeInvocation LastFocus() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"][^1];

    private IRenderedComponent<NewWorkoutPage> RenderPage(Action? onDiscarded = null, Action? onSaved = null)
    {
        workouts = UseWorkouts(clock);
        return Context.Render<NewWorkoutPage>(p => p
            .Add(x => x.OnDiscarded, onDiscarded ?? (() => { }))
            .Add(x => x.OnSaved, onSaved ?? (() => { })));
    }

    /// <summary>Opens the add-exercise list, searches for the exercise by name and picks it.</summary>
    private static void Add(IRenderedComponent<NewWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=exercises-row-{exerciseId}]").Click();
    }

    /// <summary>Registers a workout service whose drafts start at <paramref name="draftClock"/>'s current time.</summary>
    private CountingWorkoutService UseWorkouts(TimeProvider draftClock)
    {
        var workouts = new CountingWorkoutService(new SampleWorkoutService(draftClock, Catalog));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        return workouts;
    }

    private PendingWeightHistory UseWeights()
    {
        var weights = new PendingWeightHistory();
        Context.Services.AddSingleton<IWeightHistoryService>(weights);
        return weights;
    }

    /// <summary>Records the exercise id of each load; each load waits until the test completes it.</summary>
    private sealed class PendingWeightHistory : IWeightHistoryService
    {
        private readonly List<TaskCompletionSource<WeightHistoryResult>> pending = [];

        public List<int> Loads { get; } = [];

        public Task<WeightHistoryResult> GetAsync(int exerciseId)
        {
            Loads.Add(exerciseId);
            var load = new TaskCompletionSource<WeightHistoryResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            pending.Add(load);
            return load.Task;
        }

        /// <summary>Completes a load: the latest, or the one at <paramref name="index"/> in call order.</summary>
        public void Complete(WeightHistoryResult result, int? index = null) => pending[index ?? pending.Count - 1].SetResult(result);
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

        public bool IsAssisted(int exerciseId) => inner.IsAssisted(exerciseId);

        public bool HasSearchWords(string? query) => inner.HasSearchWords(query);

        public IReadOnlyList<ExerciseDefinition> Search(string? query)
        {
            Searches++;
            return inner.Search(query);
        }
    }

    /// <summary>
    /// Counts <see cref="StartNew"/> calls and keeps the drafts, and records <see cref="SaveNewAsync"/> calls, which return
    /// <see cref="SaveNew"/>'s result when set; the rest is passed to <paramref name="inner"/>.
    /// </summary>
    private sealed class CountingWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        public int Started => Drafts.Count;

        public List<WorkoutDraft> Drafts { get; } = [];

        public List<(WorkoutDraft Draft, IReadOnlyList<WorkoutExercise> Exercises)> Saves { get; } = [];

        /// <summary>When set, <see cref="SaveNewAsync"/> returns its result instead of the inner service's.</summary>
        public Func<Task<SaveResult>>? SaveNew { get; set; }

        public WorkoutDraft StartNew()
        {
            var draft = inner.StartNew();
            Drafts.Add(draft);
            return draft;
        }

        /// <summary>When set, <see cref="Elapsed"/> returns it instead of the inner service's time.</summary>
        public TimeSpan? ElapsedTime { get; set; }

        public TimeSpan Elapsed(WorkoutDraft draft) => ElapsedTime ?? inner.Elapsed(draft);

        public WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, IReadOnlyList<WorkoutExercise> exercises)
        {
            Saves.Add((draft, exercises));
            return SaveNew?.Invoke() ?? inner.SaveNewAsync(draft, exercises);
        }

        public Task<WorkoutResult> GetAsync(int workoutId) => inner.GetAsync(workoutId);

        public Task<SaveResult> SaveAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveAsync(workoutId, exercises);

        public Task<WorkoutDelete> DeleteAsync(int workoutId) => inner.DeleteAsync(workoutId);
    }
}
