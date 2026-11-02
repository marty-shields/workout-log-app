using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
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
        new CardioExercise(300, new CardioEntry(new TimeSpan(0, 20, 0), 3.2m, null, null, 1.5m, null, null)),
    ]);

    private static readonly WorkoutResult Loaded = new(Saved, WorkoutLoad.Loaded);

    /// <summary><see cref="Saved"/> with a workout note, and notes on the squat and the treadmill.</summary>
    private static readonly Workout SavedWithNotes = Saved with
    {
        Note = "Slept badly",
        Exercises = [Saved.Exercises[0] with { Note = "Belt on" }, Saved.Exercises[1], Saved.Exercises[2] with { Note = "Windy" }],
    };

    private FakeWorkoutService workouts = null!;
    private FakeWeightHistoryService history = null!;
    private FakeLogger<EditWorkoutPage> logger = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider();
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        workouts = new FakeWorkoutService(new SampleWorkoutService(clock, Catalog));
        Context.Services.AddSingleton<IWorkoutService>(workouts);
        history = new FakeWeightHistoryService();
        Context.Services.AddSingleton<IWeightHistoryService>(history);
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
    public async Task Loaded_ShowsTheStartDateAndTotalTimeInThePhonesTimeZone()
    {
        var cut = await RenderLoadedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Wed 21 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Total 1:04:37"));
            Assert.That(cut.FindAll("[data-testid=delete-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheSavedExercisesNewestFirstWithTheirValues()
    {
        var cut = await RenderOpenedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Treadmill", "Farmer's Walk", "Barbell Back Squat" }));
            Assert.That(Value(cut, "exercise-3-set-1-reps"), Is.EqualTo("8"));
            Assert.That(Value(cut, "exercise-3-set-2-weight"), Is.EqualTo("62.5"));
            Assert.That(Value(cut, "exercise-1-duration-mm"), Is.EqualTo("20"));
            Assert.That(Value(cut, "exercise-1-distance"), Is.EqualTo("3.2"));
            Assert.That(cut.Find("[data-testid=exercise-1-worked-out]").TextContent, Is.EqualTo("Speed 9.6 km/h"));
        });
    }

    [Test]
    public async Task Loaded_EveryExerciseStartsCollapsed()
    {
        var cut = await RenderLoadedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Treadmill", "Farmer's Walk", "Barbell Back Squat" }));
            Assert.That(cut.FindAll("[data-testid$=-edit]"), Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid$=-remove]"), Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid$=-add-set]"), Is.Empty);
        });
    }

    [Test]
    public async Task Edit_ReopensTheExerciseWithItsValues()
    {
        var cut = await RenderLoadedAsync();

        cut.Find("[data-testid=exercise-3-edit]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(Value(cut, "exercise-3-set-1-reps"), Is.EqualTo("8"));
            Assert.That(Value(cut, "exercise-3-set-2-weight"), Is.EqualTo("62.5"));
            Assert.That(cut.FindAll("[data-testid=exercise-2-edit]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Edit_ManyCardsCanBeOpenAtOnce()
    {
        var cut = await RenderLoadedAsync();

        cut.Find("[data-testid=exercise-1-edit]").Click();
        cut.Find("[data-testid=exercise-3-edit]").Click();

        Assert.That(cut.FindAll("[data-testid$=-done]").Select(d => d.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-done", "exercise-3-done" }));
    }

    [Test]
    public async Task EditThenDone_WithNoChange_IsNotAChange()
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);
        cut.Find("[data-testid=exercise-3-edit]").Click();
        cut.Find("[data-testid=exercise-3-done]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(backs, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Done_AfterATypedChange_StillAsksToDiscard()
    {
        var cut = await RenderLoadedAsync();
        cut.Find("[data-testid=exercise-3-edit]").Click();
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-done]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Discard changes?" }));
    }

    [Test]
    public async Task Done_InvalidEditedExercise_StaysOpenWithItsError()
    {
        var cut = await RenderLoadedAsync();
        cut.Find("[data-testid=exercise-3-edit]").Click();
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("");

        cut.Find("[data-testid=exercise-3-done]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-3-set-1-error]").TextContent, Does.Contain("Enter reps"));
            Assert.That(cut.FindAll("[data-testid=exercise-3-edit]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-2-edit]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Save_InvalidWhileOtherCardsAreCollapsed_FocusesTheOpenCardsError()
    {
        var cut = await RenderLoadedAsync();
        cut.Find("[data-testid=exercise-3-edit]").Click();
        var reps = cut.Find("[data-testid=exercise-3-set-1-reps]").GetAttribute("blazor:elementreference");
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves, Is.Empty);
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(reps)));
    }

    [Test]
    public async Task Remove_ACollapsedExercise_AsksByNameAndRemovesOnConfirm()
    {
        var cut = await RenderLoadedAsync();

        cut.Find("[data-testid=exercise-2-remove]").Click();

        Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Delete Farmer's Walk?" }));
        Assert.That(Names(cut), Has.Count.EqualTo(3));
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        Assert.That(Names(cut), Is.EqualTo(new[] { "Treadmill", "Barbell Back Squat" }));
    }

    [Test]
    public async Task Remove_Cancel_KeepsTheExerciseAndIsNotAChange()
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);
        cut.Find("[data-testid=exercise-2-remove]").Click();

        cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(backs, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task PickAgain_ACollapsedExercise_ReopensItOnTop()
    {
        var cut = await RenderLoadedAsync();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Barbell Back Squat", "Treadmill", "Farmer's Walk" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-edit]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-3-reps]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_CarrySetsStartClosed()
    {
        var cut = await RenderOpenedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-2-set-1-open]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-2-set-2-open]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".panel"), Is.Empty);
        });
    }

    [Test]
    public async Task NotFound_ShowsTheMessage()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(null, WorkoutLoad.NotFound));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-not-found]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=workout-load-retry]"), Is.Empty);
    }

    [Test]
    public async Task LoadFails_ShowsTheErrorThenTryAgainReloads()
    {
        var cut = RenderPage();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workout-load-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Loads, Is.EqualTo(new[] { Id, Id }));
            Assert.That(cut.FindAll("[data-testid=workout-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Is.Empty);
        });

        workouts.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Names(cut), Has.Count.EqualTo(3)));
    }

    [Test]
    public async Task TryAgain_FailsAgain_ShowsTheErrorAndFocusesTryAgainAgain()
    {
        var cut = RenderPage();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workout-load-retry]").Click();
        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
        Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1));
        Focuses()[^1].Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=workout-load-retry]"));
    }

    [Test]
    public async Task LoadFails_LogsNothingInTheUi()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(null, WorkoutLoad.Failed));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task UnknownExercise_LogsAnErrorAndShowsTheLoadFailure()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(Saved with { Exercises = [Saved.Exercises[0], new StrengthExercise(9999, [])] }, WorkoutLoad.Loaded));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout 7 failed: exercise 9999 is unknown or the wrong type"),
        }));
    }

    [Test]
    public async Task WrongType_LogsAnErrorAndShowsTheLoadFailure()
    {
        var cut = RenderPage();

        workouts.Complete(new WorkoutResult(Saved with { Exercises = [new CardioExercise(6, new CardioEntry(TimeSpan.FromMinutes(1), null, null, null, null, null, null))] }, WorkoutLoad.Loaded));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Has.Count.EqualTo(1)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (UiEvent.EditWorkoutOpenFailed, LogLevel.Error, "Opening workout 7 failed: exercise 6 is unknown or the wrong type"),
        }));
    }

    [Test]
    public async Task Loaded_LogsNothingInTheUi()
    {
        await RenderLoadedAsync();

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public async Task Back_NoChanges_RaisesOnBackWithNoPrompt()
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Back_AfterAnEdit_ShowsDiscardChangesAndFocusesCancel()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=confirm-dialog] h2").TextContent, Is.EqualTo("Discard changes?"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").TextContent, Is.EqualTo("Discard"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-cancel]").TextContent, Is.EqualTo("Cancel"));
            Assert.That(backs, Is.Zero);
        });
        var cancel = cut.Find("[data-testid=confirm-dialog-cancel]").GetAttribute("blazor:elementreference");
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(cancel)));
    }

    [TestCase("value typed")]
    [TestCase("set added")]
    [TestCase("set removed")]
    [TestCase("exercise added")]
    [TestCase("exercise removed")]
    [TestCase("strength exercise picked again")]
    [TestCase("cardio exercise on top picked again")]
    [TestCase("value typed back")]
    public async Task AndroidBack_Changed_Asks(string change)
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        switch (change)
        {
            case "value typed":
                cut.Find("[data-testid=exercise-1-distance]").Input("4");
                break;
            case "set added":
                cut.Find("[data-testid=exercise-3-add-set]").Click();
                break;
            case "set removed":
                cut.Find("[data-testid=exercise-3-set-2-remove]").Click();
                break;
            case "exercise added":
                Add(cut, 132);
                break;
            case "exercise removed":
                Remove(cut, 2);
                break;
            case "strength exercise picked again":
                Add(cut, 6);
                break;
            case "cardio exercise on top picked again":
                Add(cut, 300);
                break;
            case "value typed back":
                cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
                cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
                cut.Find("[data-testid=exercise-3-set-1-reps]").Input("8");
                cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
                break;
        }

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
            Assert.That(backs, Is.Zero);
        });
    }

    [TestCase("add-exercise list opened and closed")]
    [TestCase("save failed")]
    public async Task Back_NotChanged_RaisesOnBack(string action)
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);
        switch (action)
        {
            case "add-exercise list opened and closed":
                cut.Find("[data-testid=add-exercise]").Click();
                cut.Find("[data-testid=exercises-search]").Input("curl");
                await cut.InvokeAsync(BackButton.HandleAsync);
                break;
            case "save failed":
                workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
                cut.Find("[data-testid=save-workout]").Click();
                break;
        }

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [Test]
    public async Task Cancel_ClosesThePromptKeepsTheEditsAndMovesNoFocus()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        var cancel = cut.Find("[data-testid=confirm-dialog-cancel]").GetAttribute("blazor:elementreference");
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(cancel)));
        var focusCalls = Focuses().Count;

        cut.Find("[data-testid=confirm-dialog-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(Focuses(), Has.Count.EqualTo(focusCalls));
            Assert.That(Value(cut, "exercise-3-set-1-reps"), Is.EqualTo("9"));
            Assert.That(backs, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Discard_LogsTheIdAndRaisesOnBack()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.EditsDiscarded, LogLevel.Information, $"Edits to workout {Id} discarded") }));
            Assert.That(workouts.Saves, Is.Empty);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Back_AfterASuccessfulSave_IsIgnored(bool edited)
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        if (edited)
        {
            cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
            cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        }

        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task Back_AfterASaveFails_WithChanges_Asks()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
            Assert.That(backs, Is.Zero);
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
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workout-loading]"), Is.Empty));

        cut.Find("[data-testid=back]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(2));
    }

    [Test]
    public async Task Delete_ShowsDeleteThisWorkoutAndFocusesCancel()
    {
        var cut = await RenderLoadedAsync();

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=confirm-dialog] h2").TextContent, Is.EqualTo("Delete this workout?"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-confirm]").TextContent, Is.EqualTo("Delete"));
            Assert.That(cut.Find("[data-testid=confirm-dialog-cancel]").TextContent, Is.EqualTo("Cancel"));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog] .message"), Is.Empty);
            Assert.That(workouts.Deletes, Is.Empty);
        });
        var cancel = cut.Find("[data-testid=confirm-dialog-cancel]").GetAttribute("blazor:elementreference");
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(cancel)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Delete_CancelOrAndroidBack_ClosesThePromptAndKeepsTheWorkout(bool androidBack)
    {
        var backs = 0;
        var deleted = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++, onDeleted: () => deleted++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=delete-workout]").Click();

        if (androidBack)
        {
            await cut.InvokeAsync(BackButton.HandleAsync);
        }
        else
        {
            cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        }

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(Value(cut, "exercise-3-set-1-reps"), Is.EqualTo("9"));
            Assert.That(workouts.Deletes, Is.Empty);
            Assert.That(backs, Is.Zero);
            Assert.That(deleted, Is.Zero);
        });
    }

    [Test]
    public async Task Delete_Confirmed_DeletesTheLoadedIdAndRaisesOnDeletedOnly()
    {
        var backs = 0;
        var saved = 0;
        var deleted = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++, onSaved: () => saved++, onDeleted: () => deleted++);
        cut.Find("[data-testid=delete-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Deletes, Is.EqualTo(new[] { Id }));
            Assert.That(deleted, Is.EqualTo(1));
            Assert.That(backs, Is.Zero);
            Assert.That(saved, Is.Zero);
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Delete_AfterTheIdParameterChanges_DeletesTheLoadedId()
    {
        var cut = await RenderLoadedAsync();
        cut.Render(p => p.Add(x => x.WorkoutId, Id + 1));
        cut.Find("[data-testid=delete-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.That(workouts.Deletes, Is.EqualTo(new[] { Id }));
    }

    [Test]
    public async Task Delete_WithChanges_AsksTheSameAndDeletesWithoutDiscardingOrSaving()
    {
        var deleted = 0;
        var cut = await RenderOpenedAsync(onDeleted: () => deleted++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=delete-workout]").Click();
        var title = cut.Find("[data-testid=confirm-dialog] h2").TextContent;

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("Delete this workout?"));
            Assert.That(workouts.Deletes, Is.EqualTo(new[] { Id }));
            Assert.That(deleted, Is.EqualTo(1));
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Delete_Fails_ShowsDeleteFailedFocusesDeleteAndKeepsTheEdits()
    {
        var deleted = 0;
        var cut = await RenderOpenedAsync(onDeleted: () => deleted++);
        var delete = cut.Find("[data-testid=delete-workout]").GetAttribute("blazor:elementreference");
        workouts.Delete = () => Task.FromResult(WorkoutDelete.Failed);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=delete-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't delete the workout. Try again"));
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(Value(cut, "exercise-3-set-1-reps"), Is.EqualTo("9"));
            Assert.That(deleted, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(delete)));
    }

    [Test]
    public async Task Delete_AfterItFails_BackAndDeleteWorkAgain()
    {
        var backs = 0;
        var deleted = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++, onDeleted: () => deleted++);
        workouts.Delete = () => Task.FromResult(WorkoutDelete.Failed);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);
        var discardTitle = cut.Find("[data-testid=confirm-dialog] h2").TextContent;
        cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        workouts.Delete = null;
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(discardTitle, Is.EqualTo("Discard changes?"));
            Assert.That(workouts.Deletes, Is.EqualTo(new[] { Id, Id }));
            Assert.That(deleted, Is.EqualTo(1));
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task Delete_AfterItFails_SaveWorks()
    {
        var saved = 0;
        var cut = await RenderLoadedAsync(onSaved: () => saved++);
        workouts.Delete = () => Task.FromResult(WorkoutDelete.Failed);
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ConfirmDelete_Twice_DeletesOnce()
    {
        var deleted = 0;
        var cut = await RenderLoadedAsync(onDeleted: () => deleted++);
        var pending = new TaskCompletionSource<WorkoutDelete>();
        workouts.Delete = () => pending.Task;
        cut.Find("[data-testid=delete-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        await cut.InvokeAsync(() => pending.SetResult(WorkoutDelete.Deleted));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(deleted, Is.EqualTo(1)));
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Deletes, Has.Count.EqualTo(1));
            Assert.That(deleted, Is.EqualTo(1));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task WhileDeletingOrDeleted_BackSaveAndDeleteAreIgnored(bool finished)
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        if (!finished)
        {
            workouts.Delete = () => new TaskCompletionSource<WorkoutDelete>().Task;
        }

        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=save-workout]").Click();
        var afterBackAndSave = DialogTitles(cut);
        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(afterBackAndSave, Is.EqualTo(new[] { "Delete this workout?" }));
            Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Delete this workout?" }));
            Assert.That(backs, Is.Zero);
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(workouts.Deletes, Has.Count.EqualTo(1));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CancelOrAndroidBack_WhileDeletingOrDeleted_KeepsThePrompt(bool finished)
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);
        if (!finished)
        {
            workouts.Delete = () => new TaskCompletionSource<WorkoutDelete>().Task;
        }

        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.Find("[data-testid=confirm-dialog-cancel]").Click();
        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Delete this workout?" }));
            Assert.That(backs, Is.Zero);
            Assert.That(workouts.Deletes, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Cancel_AfterADeleteFails_ClosesThePrompt()
    {
        workouts.Delete = () => Task.FromResult(WorkoutDelete.Failed);
        var cut = await RenderLoadedAsync();
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-cancel]").Click();

        Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Delete_WhileSavingOrSaved_IsIgnored(bool finished)
    {
        var cut = await RenderLoadedAsync();
        if (!finished)
        {
            workouts.Save = () => new TaskCompletionSource<SaveResult>().Task;
        }

        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(workouts.Deletes, Is.Empty);
        });
    }

    [Test]
    public async Task Delete_AfterADiscard_IsIgnored()
    {
        var cut = await RenderOpenedAsync();
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Discard changes?" }));
            Assert.That(workouts.Deletes, Is.Empty);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ConfirmDelete_WhileSavingOrSaved_ClosesThePromptAndDoesNothing(bool finished)
    {
        var deleted = 0;
        var cut = await RenderLoadedAsync(onDeleted: () => deleted++);
        if (!finished)
        {
            workouts.Save = () => new TaskCompletionSource<SaveResult>().Task;
        }

        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
            Assert.That(workouts.Deletes, Is.Empty);
            Assert.That(deleted, Is.Zero);
        });
    }

    [Test]
    public async Task ConfirmDelete_AfterADiscard_DoesNothing()
    {
        var deleted = 0;
        var cut = await RenderOpenedAsync(onDeleted: () => deleted++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=delete-workout]").Click();

        cut.FindAll("[data-testid=confirm-dialog-confirm]")[0].Click();
        cut.FindAll("[data-testid=confirm-dialog-confirm]")[1].Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Deletes, Is.Empty);
            Assert.That(deleted, Is.Zero);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Discard_WhileDeletingOrDeleted_ClosesThePromptAndDoesNothing(bool finished)
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        if (!finished)
        {
            workouts.Delete = () => new TaskCompletionSource<WorkoutDelete>().Task;
        }

        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");
        cut.Find("[data-testid=exercise-3-set-1-weight]").Input("20");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=delete-workout]").Click();
        cut.FindAll("[data-testid=confirm-dialog-confirm]")[1].Click();

        cut.FindAll("[data-testid=confirm-dialog-confirm]")[0].Click();

        Assert.Multiple(() =>
        {
            Assert.That(DialogTitles(cut), Is.EqualTo(new[] { "Delete this workout?" }));
            Assert.That(backs, Is.Zero);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Deleted_HandlerFails_ShowsTheErrorScreen()
    {
        var cut = Context.Render<AppErrorBoundary>(p => p
            .Add(x => x.OnRestart, () => { })
            .AddChildContent<EditWorkoutPage>(page => page
                .Add(x => x.WorkoutId, Id)
                .Add(x => x.OnDeleted, () => throw new InvalidOperationException("Navigation failed"))));
        workouts.Complete(Loaded);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=save-workout]"), Has.Count.EqualTo(1)));
        cut.Find("[data-testid=delete-workout]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=error-message]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task Save_Unchanged_SavesTheSameRecordsUnderTheIdAndRaisesOnSaved()
    {
        var saved = 0;
        var cut = await RenderLoadedAsync(onSaved: () => saved++);

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves.Single().WorkoutId, Is.EqualTo(Id));
            Assert.That(workouts.Saves.Single().Exercises, Is.EqualTo(Saved.Exercises).Using<WorkoutExercise>(SameExercise));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Save_AfterTheIdParameterChanges_SavesUnderTheLoadedId()
    {
        var cut = await RenderLoadedAsync();
        cut.Render(p => p.Add(x => x.WorkoutId, Id + 1));

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves.Single().WorkoutId, Is.EqualTo(Id));
    }

    [Test]
    public async Task Save_Edited_SavesTheChanges()
    {
        var cut = await RenderOpenedAsync();
        cut.Find("[data-testid=exercise-3-set-2-weight]").Input("65");
        cut.Find("[data-testid=exercise-3-add-set]").Click();
        cut.Find("[data-testid=exercise-3-set-3-reps]").Input("5");
        cut.Find("[data-testid=exercise-3-set-3-weight]").Input("20");
        Remove(cut, 2);

        cut.Find("[data-testid=save-workout]").Click();

        var exercises = workouts.Saves.Single().Exercises;
        Assert.Multiple(() =>
        {
            Assert.That(exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { 6, 300 }));
            Assert.That(((StrengthExercise)exercises[0]).Sets, Is.EqualTo(new[]
            {
                new StrengthSet(8, 60m, null, null), new StrengthSet(6, 65m, null, null), new StrengthSet(5, 20m, null, null),
            }));
        });
    }

    [Test]
    public async Task Save_AfterAPickAgain_SavesThePickedExerciseLast()
    {
        var cut = await RenderLoadedAsync();
        Add(cut, 6);
        cut.WaitForElement("[data-testid=exercise-1-set-3-reps]").Input("5");
        cut.Find("[data-testid=exercise-1-set-3-weight]").Input("20");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves.Single().Exercises.Select(e => e.ExerciseId), Is.EqualTo(new[] { 187, 300, 6 }));
    }

    [Test]
    public async Task Save_Invalid_ShowsTheErrorsAndDoesNotSave()
    {
        var saved = 0;
        var cut = await RenderOpenedAsync(onSaved: () => saved++);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("");

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-3-set-1-error]").TextContent, Does.Contain("Enter reps"));
            Assert.That(workouts.Saves, Is.Empty);
            Assert.That(saved, Is.Zero);
        });
    }

    [Test]
    public async Task Save_WriteFails_ShowsSaveFailedFocusesSaveAndKeepsTheWorkout()
    {
        var saved = 0;
        var cut = await RenderLoadedAsync(onSaved: () => saved++);
        var save = cut.Find("[data-testid=save-workout]").GetAttribute("blazor:elementreference");
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't save the workout. Try again"));
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(saved, Is.Zero);
        });
        cut.WaitForAssertion(() => Assert.That(((ElementReference)LastFocus().Arguments[0]!).Id, Is.EqualTo(save)));
    }

    [Test]
    public async Task Save_WhileSaving_IsIgnored()
    {
        var saved = 0;
        var cut = await RenderLoadedAsync(onSaved: () => saved++);
        var pending = new TaskCompletionSource<SaveResult>();
        workouts.Save = () => pending.Task;

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();
        await cut.InvokeAsync(() => pending.SetResult(new SaveResult(Id, new WorkoutErrors([], []))));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(saved, Is.EqualTo(1)));
        Assert.That(workouts.Saves, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Save_AfterASuccessfulSave_IsIgnored()
    {
        var saved = 0;
        var cut = await RenderLoadedAsync(onSaved: () => saved++);

        cut.Find("[data-testid=save-workout]").Click();
        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.Saves, Has.Count.EqualTo(1));
            Assert.That(saved, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Pick_AddsAtTheTopWithOneEmptySet()
    {
        var cut = await RenderLoadedAsync();

        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Dumbbell Curl", "Treadmill", "Farmer's Walk", "Barbell Back Squat" }));
            Assert.That(Value(cut, "exercise-1-set-1-reps"), Is.Null.Or.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-overlay]"), Is.Empty);
        });
    }

    [Test]
    public async Task PickAgain_Strength_MovesItsCardToTheTopAndAddsASet()
    {
        var cut = await RenderLoadedAsync();

        Add(cut, 6);

        Assert.Multiple(() =>
        {
            Assert.That(Names(cut), Is.EqualTo(new[] { "Barbell Back Squat", "Treadmill", "Farmer's Walk" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-3]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task LoadedAndPicked_ShowNoLastAndPbLine()
    {
        var cut = await RenderLoadedAsync();

        Add(cut, 132);
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid$=-weights]"), Is.Empty);
    }

    [Test]
    public async Task Pick_ClearsTheWorkoutError()
    {
        var cut = await RenderLoadedAsync();
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        cut.Find("[data-testid=save-workout]").Click();

        Add(cut, 132);

        Assert.That(cut.FindAll("[data-testid=workout-error]"), Is.Empty);
    }

    [Test]
    public async Task AndroidBack_ClosesTheListAndStays()
    {
        var backs = 0;
        var cut = await RenderLoadedAsync(onBack: () => backs++);
        cut.Find("[data-testid=add-exercise]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-overlay]"), Is.Empty);
            Assert.That(Names(cut), Has.Count.EqualTo(3));
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task Loaded_ShowsTheWorkoutNoteAndTheExerciseNotes()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-note-open] .text").TextContent, Is.EqualTo("Slept badly"));
            Assert.That(cut.Find("[data-testid=exercise-1-note-open] .text").TextContent, Is.EqualTo("Windy"));
            Assert.That(cut.FindAll("[data-testid=exercise-2-note-open]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-3-note-open] .text").TextContent, Is.EqualTo("Belt on"));
        });
    }

    [Test]
    public async Task Loaded_WithNoNotes_ShowsAddNoteOnTheWorkoutAndEachOpenedExercise()
    {
        var cut = await RenderOpenedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-note-add]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid$=-add-note]"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public async Task Loaded_ReadsEachExercisesLastNoteBeforeTheWorkoutsStart()
    {
        history.Results[6] = new WeightHistoryResult(70m, 80m, "Belt on from set 3", Failed: false);
        history.Results[300] = new WeightHistoryResult(null, null, "Windy", Failed: false);

        var cut = await RenderOpenedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(history.Calls, Is.EquivalentTo(new (int, DateTimeOffset?)[] { (6, Start), (187, Start), (300, Start) }));
            Assert.That(cut.Find("[data-testid=exercise-1-last-note] .text").TextContent, Is.EqualTo("Windy"));
            Assert.That(cut.FindAll("[data-testid=exercise-2-last-note]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-3-last-note] .text").TextContent, Is.EqualTo("Belt on from set 3"));
            Assert.That(cut.FindAll("[data-testid$=-weights]"), Is.Empty);
        });
    }

    [Test]
    public async Task Loaded_FailedHistoryRead_ShowsNoLastNoteAndNoError()
    {
        history.Results[6] = new WeightHistoryResult(null, null, null, Failed: true);

        var cut = await RenderOpenedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid$=-last-note]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workout-load-error]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.Empty);
        });
    }

    [Test]
    public async Task Pick_ReadsTheLastNoteBeforeTheWorkoutsStartOnceForAnAddedExercise()
    {
        history.Results[132] = new WeightHistoryResult(null, null, "Slow tempo", Failed: false);
        var cut = await RenderLoadedAsync();
        history.Calls.Clear();

        Add(cut, 132);
        Add(cut, 132);

        Assert.Multiple(() =>
        {
            Assert.That(history.Calls, Is.EqualTo(new (int, DateTimeOffset?)[] { (132, Start) }));
            Assert.That(cut.Find("[data-testid=exercise-1-last-note] .text").TextContent, Is.EqualTo("Slow tempo"));
        });
    }

    [Test]
    public async Task Pick_AnExerciseAlreadyInTheWorkout_ReadsNothing()
    {
        var cut = await RenderLoadedAsync();
        history.Calls.Clear();

        Add(cut, 6);

        Assert.That(history.Calls, Is.Empty);
    }

    [Test]
    public async Task EditingNotes_SavesTheNotesTyped()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);

        TypeNote(cut, null, "Slept well");
        TypeNote(cut, 2, "Grip gave out");
        cut.Find("[data-testid=save-workout]").Click();

        var save = workouts.Saves.Single();
        Assert.Multiple(() =>
        {
            Assert.That(save.Note, Is.EqualTo("Slept well"));
            Assert.That(save.Exercises.Select(e => e.Note), Is.EqualTo(new[] { "Belt on", "Grip gave out", "Windy" }));
        });
    }

    [Test]
    public async Task ClearingNotes_SavesNone()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);

        TypeNote(cut, null, "");
        TypeNote(cut, 1, "");
        cut.Find("[data-testid=save-workout]").Click();

        var save = workouts.Saves.Single();
        Assert.Multiple(() =>
        {
            Assert.That(save.Note, Is.Null);
            Assert.That(save.Exercises.Select(e => e.Note), Is.EqualTo(new[] { "Belt on", null, null }));
        });
    }

    [Test]
    public async Task Save_ChangingAValueOnly_KeepsEveryNote()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);
        cut.Find("[data-testid=exercise-3-set-1-reps]").Input("9");

        cut.Find("[data-testid=save-workout]").Click();

        var save = workouts.Saves.Single();
        Assert.Multiple(() =>
        {
            Assert.That(save.Note, Is.EqualTo("Slept badly"));
            Assert.That(save.Exercises.Select(e => e.Note), Is.EqualTo(new[] { "Belt on", null, "Windy" }));
        });
    }

    [Test]
    public async Task Save_Unchanged_KeepsEveryNoteWithoutOpeningAnExercise()
    {
        var cut = await RenderLoadedAsync(saved: SavedWithNotes);

        cut.Find("[data-testid=save-workout]").Click();

        var save = workouts.Saves.Single();
        Assert.Multiple(() =>
        {
            Assert.That(save.Note, Is.EqualTo("Slept badly"));
            Assert.That(save.Exercises.Select(e => e.Note), Is.EqualTo(new[] { "Belt on", null, "Windy" }));
        });
    }

    [Test]
    public async Task Save_WorkoutNoteTooLong_ShowsTheNoteErrorAndDoesNotSaveUntilItIsShortened()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);
        TypeNote(cut, null, SampleData.NoteTooLong);

        cut.Find("[data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-note-error]").TextContent, Is.EqualTo("Note must be 500 characters or less"));
            Assert.That(workouts.Saves, Is.Empty);
        });

        Retype(cut, null, "Short");

        Assert.That(cut.FindAll("[data-testid=workout-note-error]"), Is.Empty);
        cut.Find("[data-testid=save-workout]").Click();
        Assert.That(workouts.Saves.Single().Note, Is.EqualTo("Short"));
    }

    [Test]
    public async Task Save_ExerciseNoteTooLong_ReopensTheCardShowsTheNoteErrorAndDoesNotSave()
    {
        var cut = await RenderLoadedAsync(saved: SavedWithNotes);
        cut.Find("[data-testid=exercise-3-edit]").Click();
        TypeNote(cut, 3, SampleData.NoteTooLong);
        cut.Find("[data-testid=exercise-3-done]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-3-note-error]").TextContent, Is.EqualTo("Note must be 500 characters or less"));
            Assert.That(cut.FindAll("[data-testid=exercise-3-edit]"), Is.Empty);
        });

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves, Is.Empty);
    }

    [Test]
    public async Task Save_Fails_WithNotes_ShowsTheSaveErrorKeepsTheNotesAndSavesThemOnRetry()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);
        workouts.Save = () => Task.FromResult(new SaveResult(null, new WorkoutErrors([WorkoutError.SaveFailed], [])));
        cut.Find("[data-testid=save-workout]").Click();
        workouts.Save = null;

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Couldn't save the workout. Try again"));
            Assert.That(cut.Find("[data-testid=workout-note-open] .text").TextContent, Is.EqualTo("Slept badly"));
        });

        cut.Find("[data-testid=save-workout]").Click();

        Assert.That(workouts.Saves.Last().Note, Is.EqualTo("Slept badly"));
    }

    [TestCase("workout note")]
    [TestCase("exercise note")]
    public async Task Back_AfterANoteEdit_Asks(string note)
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++, saved: SavedWithNotes);

        TypeNote(cut, note == "workout note" ? null : 2, "Changed");
        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Has.Count.EqualTo(1));
            Assert.That(backs, Is.Zero);
        });
    }

    [Test]
    public async Task Back_AfterOpeningAddNoteWithoutTyping_RaisesOnBack()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++);
        cut.Find("[data-testid=workout-note-add]").Click();
        cut.Find("[data-testid=workout-note-input]").Blur();
        cut.Find("[data-testid=exercise-2-add-note]").Click();
        cut.Find("[data-testid=exercise-2-note-input]").Blur();

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [Test]
    public async Task Back_AfterOpeningASavedNoteWithoutTyping_RaisesOnBack()
    {
        var backs = 0;
        var cut = await RenderOpenedAsync(onBack: () => backs++, saved: SavedWithNotes);
        cut.Find("[data-testid=workout-note-open]").Click();
        cut.Find("[data-testid=workout-note-input]").Blur();

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(backs, Is.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=confirm-dialog]"), Is.Empty);
        });
    }

    [Test]
    public async Task Discard_AfterANoteEdit_LogsTheIdAndNoNoteText()
    {
        var cut = await RenderOpenedAsync(saved: SavedWithNotes);
        TypeNote(cut, null, "Changed note text");
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=confirm-dialog-confirm]").Click();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.EditsDiscarded, LogLevel.Information, $"Edits to workout {Id} discarded") }));
    }

    private static IReadOnlyList<string> Names(IRenderedComponent<EditWorkoutPage> cut) =>
        cut.FindAll(".card .name").Select(n => n.TextContent).ToList();

    /// <summary>
    /// Types the text into the note of the workout (<paramref name="number"/> null) or the numbered exercise and leaves the field. Opens the
    /// field by Add note, or by the one-line note when it has text.
    /// </summary>
    private static void TypeNote(IRenderedComponent<EditWorkoutPage> cut, int? number, string text)
    {
        var prefix = number is { } n ? $"exercise-{n}-note" : "workout-note";
        OpenNote(cut, prefix, number is { } m ? $"exercise-{m}-add-note" : "workout-note-add");
        var input = cut.Find($"[data-testid={prefix}-input]");
        input.Input(text);
        input.Blur();
    }

    /// <summary>Types over the note that shows an error, opening its field first when it is a one-line summary.</summary>
    private static void Retype(IRenderedComponent<EditWorkoutPage> cut, int? number, string text)
    {
        var prefix = number is { } n ? $"exercise-{n}-note" : "workout-note";
        OpenNote(cut, prefix, null);
        cut.Find($"[data-testid={prefix}-input]").Input(text);
    }

    private static void OpenNote(IRenderedComponent<EditWorkoutPage> cut, string prefix, string? addButton)
    {
        if (cut.FindAll($"[data-testid={prefix}-input]").Count > 0)
        {
            return;
        }

        var open = cut.FindAll($"[data-testid={prefix}-open]");
        (open.Count > 0 ? open[0] : cut.Find($"[data-testid={addButton}]")).Click();
    }

    private static string? Value(IRenderedComponent<EditWorkoutPage> cut, string testId) =>
        cut.Find($"[data-testid={testId}]").GetAttribute("value");

    /// <summary>Same type, exercise id and sets or entry; the records hold their sets in a list, which compares by reference.</summary>
    private static bool SameExercise(WorkoutExercise x, WorkoutExercise y) => (x, y) switch
    {
        (StrengthExercise a, StrengthExercise b) => a.ExerciseId == b.ExerciseId && a.Sets.SequenceEqual(b.Sets),
        (CardioExercise a, CardioExercise b) => a == b,
        _ => false,
    };

    /// <summary>The latest focus call; the list search and added exercises each take focus.</summary>
    private JSRuntimeInvocation LastFocus() => Focuses()[^1];

    private IReadOnlyList<JSRuntimeInvocation> Focuses() => Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    private static IReadOnlyList<string> DialogTitles(IRenderedComponent<EditWorkoutPage> cut) =>
        cut.FindAll("[data-testid=confirm-dialog] h2").Select(h => h.TextContent).ToList();

    private IRenderedComponent<EditWorkoutPage> RenderPage(Action? onBack = null, Action? onSaved = null, Action? onDeleted = null) =>
        Context.Render<EditWorkoutPage>(p => p
            .Add(x => x.WorkoutId, Id)
            .Add(x => x.OnBack, onBack ?? (() => { }))
            .Add(x => x.OnSaved, onSaved ?? (() => { }))
            .Add(x => x.OnDeleted, onDeleted ?? (() => { })));

    private async Task<IRenderedComponent<EditWorkoutPage>> RenderLoadedAsync(Action? onBack = null, Action? onSaved = null, Action? onDeleted = null, Workout? saved = null)
    {
        var cut = RenderPage(onBack, onSaved, onDeleted);
        workouts.Complete(saved is null ? Loaded : new WorkoutResult(saved, WorkoutLoad.Loaded));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=save-workout]"), Has.Count.EqualTo(1)));
        return cut;
    }

    /// <summary>Loads the workout and opens every exercise, as the pencil does.</summary>
    private async Task<IRenderedComponent<EditWorkoutPage>> RenderOpenedAsync(Action? onBack = null, Action? onSaved = null, Action? onDeleted = null, Workout? saved = null)
    {
        var cut = await RenderLoadedAsync(onBack, onSaved, onDeleted, saved);
        foreach (var edit in cut.FindAll("[data-testid$=-edit]"))
        {
            edit.Click();
        }

        return cut;
    }

    /// <summary>Taps the exercise's trash and confirms the delete.</summary>
    private static void Remove(IRenderedComponent<EditWorkoutPage> cut, int number)
    {
        cut.Find($"[data-testid=exercise-{number}-remove]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
    }

    /// <summary>Opens the add-exercise list, searches for the exercise by name and picks it.</summary>
    private static void Add(IRenderedComponent<EditWorkoutPage> cut, int exerciseId)
    {
        cut.Find("[data-testid=add-exercise]").Click();
        cut.Find("[data-testid=exercises-search]").Input(Catalog.All.Single(e => e.Id == exerciseId).Name);
        cut.Find($"[data-testid=exercises-row-{exerciseId}]").Click();
    }

    /// <summary>Checks with the inner service. Each load waits until the test completes it; saves and deletes are recorded.</summary>
    private sealed class FakeWorkoutService(IWorkoutService inner) : IWorkoutService
    {
        private TaskCompletionSource<WorkoutResult> pending = new();

        /// <summary>The id asked for by each load.</summary>
        public List<int> Loads { get; } = [];

        public List<(int WorkoutId, string? Note, IReadOnlyList<WorkoutExercise> Exercises)> Saves { get; } = [];

        /// <summary>When set, <see cref="SaveAsync"/> returns its result instead of the inner service's.</summary>
        public Func<Task<SaveResult>>? Save { get; set; }

        /// <summary>The id asked for by each delete.</summary>
        public List<int> Deletes { get; } = [];

        /// <summary>When set, <see cref="DeleteAsync"/> returns its result instead of the inner service's.</summary>
        public Func<Task<WorkoutDelete>>? Delete { get; set; }

        /// <summary>Completes the latest load.</summary>
        public void Complete(WorkoutResult result) => pending.SetResult(result);

        public WorkoutDraft StartNew() => inner.StartNew();

        public TimeSpan Elapsed(WorkoutDraft draft) => inner.Elapsed(draft);

        public WorkoutErrors Validate(string? note, IReadOnlyList<WorkoutExercise> exercises) => inner.Validate(note, exercises);

        public ExerciseErrors ValidateSet(int exerciseId, StrengthSet set) => inner.ValidateSet(exerciseId, set);

        public ExerciseErrors ValidateEntry(int exerciseId, CardioEntry entry) => inner.ValidateEntry(exerciseId, entry);

        public Task<SaveResult> SaveNewAsync(WorkoutDraft draft, string? note, IReadOnlyList<WorkoutExercise> exercises) => inner.SaveNewAsync(draft, note, exercises);

        public Task<WorkoutResult> GetAsync(int workoutId)
        {
            Loads.Add(workoutId);
            pending = new TaskCompletionSource<WorkoutResult>();
            return pending.Task;
        }

        public Task<SaveResult> SaveAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises)
        {
            Saves.Add((workoutId, note, exercises));
            return Save?.Invoke() ?? inner.SaveAsync(workoutId, note, exercises);
        }

        public Task<WorkoutDelete> DeleteAsync(int workoutId)
        {
            Deletes.Add(workoutId);
            return Delete?.Invoke() ?? inner.DeleteAsync(workoutId);
        }
    }
}
