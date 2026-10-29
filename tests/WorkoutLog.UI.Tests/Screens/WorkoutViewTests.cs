using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutViewTests : ComponentTestBase
{
    private static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    private static ExerciseState Curl()
    {
        var exercise = new ExerciseState(132, "Dumbbell Curl", ExerciseKind.Strength,
            [new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true)]);
        exercise.Entries.Add(new EntryState());
        return exercise;
    }

    private static ExerciseState Carry()
    {
        var exercise = new ExerciseState(187, "Farmer's Walk", ExerciseKind.Strength,
        [
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration),
        ]);
        var entry = new EntryState();
        entry.Values["distance"] = "40";
        exercise.Entries.Add(entry);
        return exercise;
    }

    [TestCase(WorkoutMode.New, true)]
    [TestCase(WorkoutMode.Edit, false)]
    public void CarrySets_OpenLastSetOnlyForNewWorkout(WorkoutMode mode, bool open)
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, mode)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Carry()]));

        Assert.That(cut.FindAll(".panel").Count, Is.EqualTo(open ? 1 : 0));
    }

    [TestCase(9, false)]
    [TestCase(10, true)]
    public void AddExercise_IsOffWithANoteAtTenExercises(int count, bool full)
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [.. Enumerable.Range(0, count).Select(_ => Curl())]));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=add-exercise]").HasAttribute("disabled"), Is.EqualTo(full));
            Assert.That(cut.FindAll("[data-testid=exercises-full]").Select(n => n.TextContent), Is.EqualTo(full ? new[] { "At most 10 exercises" } : []));
        });
    }

    [Test]
    public void ValidateSet_IsPassedToExerciseCards()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.ValidateSet, (_, _) => false));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(curl.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void New_ShowsDateAndElapsedTime()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(0, 12, 34))
            .Add(x => x.Exercises, []));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Tue 20 Oct 2026"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 0:12:34"));
            Assert.That(cut.FindAll("[data-testid=delete-workout]"), Is.Empty);
        });
    }

    [Test]
    public void Edit_ShowsTotalTimeAndDelete()
    {
        var deleted = false;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(1, 2, 3))
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.OnDelete, () => deleted = true));

        cut.Find("[data-testid=delete-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Total 1:02:03"));
            Assert.That(cut.Find("[data-testid=delete-workout]").GetAttribute("aria-label"), Is.EqualTo("Delete"));
            Assert.That(deleted, Is.True);
        });
    }

    [Test]
    public void Edit_DeleteIsATrash()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(1, 2, 3))
            .Add(x => x.Exercises, []));

        var delete = cut.FindComponents<IconButton>().Single(b => b.Instance.TestId == "delete-workout").Instance;

        Assert.That((delete.Icon, delete.Muted, delete.Size), Is.EqualTo((IconName.Delete, false, IconSize.Default)));
    }

    [Test]
    public void ElapsedOverADay_ShowsTotalHours()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Time, new TimeSpan(1, 2, 3, 4))
            .Add(x => x.Exercises, []));

        Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Elapsed 26:03:04"));
    }

    [Test]
    public void NoExercises_ShowsEmptyState()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, []));

        Assert.That(cut.Find("[data-testid=no-exercises]").TextContent, Does.Contain("No exercises yet"));
    }

    [Test]
    public void Exercises_RenderInOrder()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl(), new ExerciseState(258, "Plank", ExerciseKind.Strength, [])]));

        Assert.That(cut.FindAll(".name").Select(n => n.TextContent), Is.EqualTo(new[] { "Dumbbell Curl", "Plank" }));
    }

    [Test]
    public void Error_IsShown()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Error, "Add at least one exercise"));

        Assert.That(cut.Find("[data-testid=workout-error]").TextContent, Is.EqualTo("Add at least one exercise"));
    }

    [Test]
    public void BottomBar_RaisesAddExerciseAndSave()
    {
        var added = false;
        var saved = false;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.OnAddExercise, () => added = true)
            .Add(x => x.OnSave, () => saved = true));

        cut.Find("[data-testid=bottom-bar] [data-testid=add-exercise]").Click();
        cut.Find("[data-testid=bottom-bar] [data-testid=save-workout]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.True);
            Assert.That(saved, Is.True);
        });
    }

    [Test]
    public void RemoveExercise_RaisesWithThatExercise()
    {
        var curl = Curl();
        ExerciseState? removed = null;
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.OnRemoveExercise, (ExerciseState e) => removed = e));

        cut.Find("[data-testid=exercise-1-remove]").Click();

        Assert.That(removed, Is.SameAs(curl));
    }

    [TestCase(132, "exercise-2-set-1-reps")]
    [TestCase(187, "exercise-2-set-1-distance")]
    [TestCase(300, "exercise-2-duration-h")]
    public void Focus_FocusesItsFirstField(int exerciseId, string testId)
    {
        var added = ExerciseFields.New(Catalog.All.Single(e => e.Id == exerciseId));

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl(), added])
            .Add(x => x.Focus, new ExerciseFocus(added)));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find($"[data-testid={testId}]"));
    }

    [Test]
    public void Focus_StrengthWithSeveralSets_FocusesTheLastSet()
    {
        var curl = Curl();
        curl.Entries.Add(new EntryState());

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.Focus, new ExerciseFocus(curl)));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-reps]"));
    }

    [Test]
    public void Focus_FocusesOnlyOnce()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.Focus, new ExerciseFocus(curl)));

        cut.Render(p => p.Add(x => x.Time, TimeSpan.FromSeconds(1)));

        Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 1);
    }

    [Test]
    public void Focus_NewRequestForTheSameExercise_FocusesAgain()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.Focus, new ExerciseFocus(curl)));

        cut.Render(p => p.Add(x => x.Focus, new ExerciseFocus(curl)));

        Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2);
    }

    [Test]
    public void FocusWithAddSet_AddsASetAndFocusesIt()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.Focus, new ExerciseFocus(curl, addSet: true)));

        cut.WaitForAssertion(() => Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-reps]")));
        Assert.That(curl.Entries, Has.Count.EqualTo(2));
    }

    [Test]
    public void FocusWithAddSet_BlockedByTheLastSet_FocusesThatSet()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.ValidateSet, (_, _) => false)
            .Add(x => x.Focus, new ExerciseFocus(curl, addSet: true)));

        cut.WaitForAssertion(() => Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-1-reps]")));
        Assert.That(curl.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void FocusWithAddSet_Carry_OpensAndFocusesTheNewSet()
    {
        var carry = Carry();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [carry])
            .Add(x => x.Focus, new ExerciseFocus(carry, addSet: true)));

        cut.WaitForAssertion(() => Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-distance]")));
    }

    [Test]
    public void FocusWithAddSet_CarryBlockedByTheOpenSet_FocusesItsFirstFieldAtFault()
    {
        var carry = Carry();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [carry]));
        // bUnit drops the reference id from the markup once the input re-renders, so read it first.
        var weight = cut.Find("[data-testid=exercise-1-set-1-weight]").GetAttribute("blazor:elementreference");

        cut.Render(p => p
            .Add(x => x.ValidateSet, (_, s) => { s.Errors["weight"] = "Weight must be a number"; return false; })
            .Add(x => x.Focus, new ExerciseFocus(carry, addSet: true)));

        cut.WaitForAssertion(() => Assert.That(
            ((ElementReference)Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]!).Id, Is.EqualTo(weight)));
        Assert.That(carry.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void FocusWithAddSet_Cardio_FocusesTheEntryWithoutAdding()
    {
        var rower = ExerciseFields.New(Catalog.All.Single(e => e.Id == 305));
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [rower])
            .Add(x => x.Focus, new ExerciseFocus(rower, addSet: true)));

        cut.WaitForAssertion(() => Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-duration-h]")));
        Assert.That(rower.Entries, Has.Count.EqualTo(1));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void FocusFails_LogsAWarningAndKeepsTheWorkout(bool addSet)
    {
        var logger = new FakeLogger<WorkoutView>();
        Context.Services.AddSingleton<ILogger<WorkoutView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));
        var curl = Curl();

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.Focus, new ExerciseFocus(curl, addSet)));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ExerciseFocusFailed, LogLevel.Warning, "Focusing an exercise failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=exercise-1]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Focus_RemovedExercise_FocusesNothing()
    {
        var curl = Curl();
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl]));

        cut.Render(p => p
            .Add(x => x.Exercises, [])
            .Add(x => x.Focus, new ExerciseFocus(curl)));

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void NoFocus_FocusesNothing()
    {
        Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()]));

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void ErrorFocus_WorkoutError_FocusesAddExercise()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Error, "Add at least one exercise")
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=add-exercise]"));
    }

    [Test]
    public void ErrorFocus_Save_FocusesSave()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.Error, "Couldn't save the workout. Try again")
            .Add(x => x.ErrorFocus, new ErrorFocus(ErrorFocusTarget.Save)));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=save-workout]"));
    }

    [Test]
    public void ErrorFocus_Delete_FocusesDelete()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.Error, "Couldn't delete the workout. Try again")
            .Add(x => x.ErrorFocus, new ErrorFocus(ErrorFocusTarget.Delete)));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=delete-workout]"));
    }

    [Test]
    public void ErrorFocus_ExerciseWithNoSets_FocusesItsAddSet()
    {
        var valid = Curl();
        var empty = Curl();
        empty.Entries.Clear();
        empty.Error = "Add at least one set";
        empty.ErrorCode = ExerciseError.NoSets;

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [valid, empty])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-2-add-set]"));
    }

    [Test]
    public void ErrorFocus_ExerciseErrorWithSets_FocusesItsRemove()
    {
        var unknown = Curl();
        unknown.Error = "Exercise not found";

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [unknown])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-remove]"));
    }

    [Test]
    public void ErrorFocus_UnknownStrengthExerciseWithNoSets_FocusesItsRemove()
    {
        var unknown = Curl();
        unknown.Entries.Clear();
        unknown.Error = "Exercise not found";
        unknown.ErrorCode = ExerciseError.UnknownExercise;

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [unknown])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-remove]"));
    }

    [Test]
    public void ErrorFocus_CardioExerciseError_FocusesItsRemove()
    {
        var treadmill = ExerciseFields.New(Catalog.All.Single(e => e.Id == 300));
        treadmill.Error = "Exercise type doesn't match";

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [treadmill])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-remove]"));
    }

    [Test]
    public void ErrorFocus_SetRow_FocusesTheFirstSetWithErrors()
    {
        var curl = Curl();
        curl.Entries.Add(new EntryState { Errors = { ["reps"] = "Enter reps" } });
        curl.Entries.Add(new EntryState { Errors = { ["reps"] = "Enter reps" } });

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [curl])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-reps]"));
    }

    [Test]
    public void ErrorFocus_OpenCarrySet_FocusesItsFirstFieldAtFault()
    {
        var carry = Carry();
        carry.Entries[0].Errors["weight"] = "Weight must be a number";

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.New)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [carry])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-1-weight]"));
    }

    [Test]
    public void ErrorFocus_ClosedCarrySet_FocusesItsSummaryAndLeavesItClosed()
    {
        var carry = Carry();
        carry.Entries.Add(new EntryState { Values = { ["distance"] = "40" } });
        carry.Entries[1].Errors["weight"] = "Weight must be a number";

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [carry])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Assert.Multiple(() =>
        {
            Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-1-set-2-open]"));
            Assert.That(cut.FindAll(".panel"), Is.Empty);
        });
    }

    [Test]
    public void ErrorFocus_CardioEntry_FocusesItsFirstFieldAtFault()
    {
        var treadmill = ExerciseFields.New(Catalog.All.Single(e => e.Id == 300));
        treadmill.Entries[0].Errors["incline"] = "Incline can have up to 2 decimal places";
        treadmill.Entries[0].Errors["calories"] = "Calories must be a number";

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl(), treadmill])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=exercise-2-incline]"));
    }

    [Test]
    public void ErrorFocus_FocusesOnlyOnce()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Error, "Add at least one exercise")
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        cut.Render(p => p.Add(x => x.Time, TimeSpan.FromSeconds(1)));

        Context.JSInterop.VerifyFocusAsyncInvoke(calledTimes: 1);
    }

    [Test]
    public void ErrorFocus_NoErrors_FocusesNothing()
    {
        Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void ErrorFocusFails_LogsAWarningAndKeepsTheWorkout()
    {
        var logger = new FakeLogger<WorkoutView>();
        Context.Services.AddSingleton<ILogger<WorkoutView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Error, "Add at least one exercise")
            .Add(x => x.ErrorFocus, new ErrorFocus()));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ErrorFocusFailed, LogLevel.Warning, "Focusing the first error failed") })));
        Assert.That(cut.FindAll("[data-testid=workout-error]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void SaveFocusFails_LogsAWarningAndKeepsTheWorkout()
    {
        var logger = new FakeLogger<WorkoutView>();
        Context.Services.AddSingleton<ILogger<WorkoutView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.Error, "Couldn't save the workout. Try again")
            .Add(x => x.ErrorFocus, new ErrorFocus(ErrorFocusTarget.Save)));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ErrorFocusFailed, LogLevel.Warning, "Focusing the first error failed") })));
        Assert.That(cut.FindAll("[data-testid=exercise-1]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void DeleteFocusFails_LogsAWarningAndKeepsTheWorkout()
    {
        var logger = new FakeLogger<WorkoutView>();
        Context.Services.AddSingleton<ILogger<WorkoutView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.Mode, WorkoutMode.Edit)
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [Curl()])
            .Add(x => x.Error, "Couldn't delete the workout. Try again")
            .Add(x => x.ErrorFocus, new ErrorFocus(ErrorFocusTarget.Delete)));

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ErrorFocusFailed, LogLevel.Warning, "Focusing the first error failed") })));
        Assert.That(cut.FindAll("[data-testid=exercise-1]"), Has.Count.EqualTo(1));
    }

    [Test]
    public void Overlay_IsRendered()
    {
        var cut = Context.Render<WorkoutView>(p => p
            .Add(x => x.StartedAt, Started)
            .Add(x => x.Exercises, [])
            .Add(x => x.Overlay, (RenderFragment)(b => b.AddMarkupContent(0, "<div data-testid=\"overlay\"></div>"))));

        Assert.That(cut.Find("[data-testid=overlay]"), Is.Not.Null);
    }
}
