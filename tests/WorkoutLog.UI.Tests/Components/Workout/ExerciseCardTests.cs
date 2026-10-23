using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class ExerciseCardTests : ComponentTestBase
{
    /// <summary>Reps are required and must be digits only; marks the set for re-checking when it fails, as <see cref="SetChecker"/> does.</summary>
    private static bool WholeReps(ExerciseState exercise, EntryState entry)
    {
        entry.Errors.Clear();
        var reps = entry.Values.GetValueOrDefault("reps");
        if (string.IsNullOrEmpty(reps))
        {
            entry.Errors["reps"] = "Enter reps";
        }
        else if (!reps.All(char.IsAsciiDigit))
        {
            entry.Errors["reps"] = "Reps must be a number";
        }

        entry.CheckOnChange |= entry.Errors.Count > 0;
        return entry.Errors.Count == 0;
    }

    /// <summary>As <see cref="WholeReps"/>, but leaves <see cref="EntryState.CheckOnChange"/> to the card.</summary>
    private static bool PlainWholeReps(ExerciseState exercise, EntryState entry)
    {
        var flag = entry.CheckOnChange;
        var valid = WholeReps(exercise, entry);
        entry.CheckOnChange = flag;
        return valid;
    }

    private static ExerciseState Strength(int sets)
    {
        var exercise = new ExerciseState(132, "Dumbbell Curl", ExerciseKind.Strength,
        [
            new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        ]);
        for (var i = 0; i < sets; i++)
        {
            exercise.Entries.Add(new EntryState());
        }

        return exercise;
    }

    private static ExerciseState Carry(int sets)
    {
        var exercise = new ExerciseState(187, "Farmer's Walk", ExerciseKind.Strength,
        [
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
            new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration),
        ]);
        for (var i = 0; i < sets; i++)
        {
            var entry = new EntryState();
            entry.Values["distance"] = "40";
            exercise.Entries.Add(entry);
        }

        return exercise;
    }

    private static ExerciseState Cardio()
    {
        var exercise = new ExerciseState(300, "Treadmill", ExerciseKind.Cardio,
        [
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true),
            new FieldSpec("distance", Metric.Distance, "Distance", FieldKind.Decimal, Unit: "km"),
        ]);
        exercise.Entries.Add(new EntryState());
        return exercise;
    }

    [Test]
    public void Strength_ShowsOneRowPerSet()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(2))
            .Add(x => x.TestId, "exercise-1"));

        Assert.That(cut.FindAll(".set-row"), Has.Count.EqualTo(2));
    }

    [Test]
    public void Strength_HeaderShowsLabelsWithUnitsAndRequired()
    {
        var cut = Context.Render<ExerciseCard>(p => p.Add(x => x.Exercise, Strength(1)));

        var labels = cut.FindAll(".set-header span").Select(s => s.TextContent);

        Assert.That(labels, Is.EqualTo(new[] { "Set", "Reps *", "Weight (kg)", "" }));
    }

    [Test]
    public void Strength_ColumnsAreNumberFieldsAndRemove()
    {
        var exercise = new ExerciseState(258, "Plank", ExerciseKind.Strength,
        [
            new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole),
            new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration),
        ]);
        exercise.Entries.Add(new EntryState());
        var cut = Context.Render<ExerciseCard>(p => p.Add(x => x.Exercise, exercise));

        Assert.That(cut.Find(".set-header").GetAttribute("style"),
            Is.EqualTo("grid-template-columns: var(--set-number-width) minmax(0, 1fr) minmax(0, 2fr) var(--touch-target)"));
    }

    [Test]
    public void Remove_NamesTheExercise()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(1))
            .Add(x => x.TestId, "exercise-1"));

        Assert.That(TestText.Normalise(cut.Find("[data-testid=exercise-1-remove]").TextContent), Is.EqualTo("Remove Dumbbell Curl"));
    }

    [Test]
    public void RemoveSet_NamesTheExercise()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(1))
            .Add(x => x.TestId, "exercise-1"));

        Assert.That(cut.Find("[data-testid=exercise-1-set-1-remove]").GetAttribute("aria-label"), Is.EqualTo("Remove Dumbbell Curl set 1"));
    }

    [Test]
    public void Carry_NoHeaderLastSetOpenOthersSummaries()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(3))
            .Add(x => x.TestId, "exercise-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".set-header"), Is.Empty);
            Assert.That(cut.FindAll(".summary"), Has.Count.EqualTo(2));
            Assert.That(cut.Find(".panel").GetAttribute("data-testid"), Is.EqualTo("exercise-1-set-3"));
        });
    }

    [Test]
    public void Carry_TapSummary_OpensThatSetAndClosesOpenOne()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(3))
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-1" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-3-open]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Carry_AddSet_OpensNewSet()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(2))
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-3" }));
    }

    [Test]
    public void Carry_RemoveOpenSet_LeavesAllCollapsed()
    {
        var exercise = Carry(2);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-2-remove]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries, Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".panel"), Is.Empty);
            Assert.That(cut.FindAll(".summary"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Carry_NewExerciseInstance_OpensItsLastSet()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(2))
            .Add(x => x.TestId, "exercise-1"));

        cut.Render(p => p.Add(x => x.Exercise, Carry(3)));

        Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-3" }));
    }

    [TestCase(false, 1)]
    [TestCase(true, 2)]
    public void AddSet_ValidatesLastRow(bool valid, int sets)
    {
        var exercise = Strength(1);
        (ExerciseState Exercise, EntryState Entry)? checkedSet = null;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (e, s) => { checkedSet = (e, s); return valid; })
            .Add(x => x.TestId, "exercise-1"));
        var last = exercise.Entries[0];

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(checkedSet, Is.EqualTo((exercise, last)));
            Assert.That(exercise.Entries, Has.Count.EqualTo(sets));
        });
    }

    [Test]
    public void AddSet_Blocked_FocusesTheFirstFieldAtFault()
    {
        var exercise = Strength(1);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, s) => { s.Errors["weight"] = "Weight must be a number"; return false; })
            .Add(x => x.TestId, "exercise-1"));

        var weight = ReferenceId(cut, "exercise-1-set-1-weight");
        cut.Find("[data-testid=exercise-1-add-set]").Click();

        cut.WaitForAssertion(() => Assert.That(FocusedId(), Is.EqualTo(weight)));
    }

    [TestCase("exercise-1-add-set")]
    [TestCase("exercise-1-set-1-open")]
    public void Blocked_FocusFails_LogsAWarningAndKeepsTheCard(string tap)
    {
        var logger = new FakeLogger<ExerciseCard>();
        Context.Services.AddSingleton<ILogger<ExerciseCard>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(2))
            .Add(x => x.ValidateSet, (_, s) => { s.Errors["weight"] = "Weight must be a number"; return false; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find($"[data-testid={tap}]").Click();

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.BlockedSetFocusFailed, LogLevel.Warning, "Focusing a blocked set failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=exercise-1]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void AddSet_Allowed_LeavesFocus()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(1))
            .Add(x => x.ValidateSet, (_, _) => true)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(Context.JSInterop.Invocations, Is.Empty);
    }

    [Test]
    public void Carry_OpenOtherSet_Blocked_FocusesTheFirstFieldAtFaultInTheOpenSet()
    {
        var exercise = Carry(2);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, s) => { s.Errors["weight"] = "Weight must be a number"; return false; })
            .Add(x => x.TestId, "exercise-1"));

        var weight = ReferenceId(cut, "exercise-1-set-2-weight");
        cut.Find("[data-testid=exercise-1-set-1-open]").Click();

        cut.WaitForAssertion(() => Assert.That(FocusedId(), Is.EqualTo(weight)));
    }

    [Test]
    public void AddSet_NoSets_IsNotBlocked()
    {
        var exercise = Strength(0);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, _) => false)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.That(exercise.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Carry_AddSet_BlockedWhileOpenSetInvalid()
    {
        var exercise = Carry(2);
        EntryState? checkedEntry = null;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, s) => { checkedEntry = s; return false; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(checkedEntry, Is.SameAs(exercise.Entries[1]));
            Assert.That(exercise.Entries, Has.Count.EqualTo(2));
            Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-2" }));
        });
    }

    [TestCase(false, "exercise-1-set-2")]
    [TestCase(true, "exercise-1-set-1")]
    public void Carry_OpenOtherSet_ValidatesOpenSet(bool valid, string expectedOpen)
    {
        var exercise = Carry(2);
        EntryState? checkedEntry = null;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, s) => { checkedEntry = s; return valid; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(checkedEntry, Is.SameAs(exercise.Entries[1]));
            Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { expectedOpen }));
        });
    }

    [Test]
    public void Carry_NoOpenSet_OpeningIsNotBlocked()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(2))
            .Add(x => x.OpenLastSet, false)
            .Add(x => x.ValidateSet, (_, _) => false)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-open]").Click();

        Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-1" }));
    }

    [Test]
    public void Carry_OpenLastSetFalse_AllCollapsed()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Carry(2))
            .Add(x => x.OpenLastSet, false));

        Assert.That(cut.FindAll(".panel"), Is.Empty);
    }

    [Test]
    public void Carry_OpenLastSetFalse_EmptyLastSetStillOpens()
    {
        var exercise = Carry(1);
        exercise.Entries.Add(new EntryState());
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.OpenLastSet, false)
            .Add(x => x.TestId, "exercise-1"));

        Assert.That(cut.FindAll(".panel").Select(p => p.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-1-set-2" }));
    }

    [Test]
    public void AddSet_AddsEmptySetAndRaisesOnChanged()
    {
        var exercise = Strength(1);
        var changed = false;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.OnChanged, () => changed = true)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries, Has.Count.EqualTo(2));
            Assert.That(cut.FindAll(".set-row"), Has.Count.EqualTo(2));
            Assert.That(changed, Is.True);
        });
    }

    [Test]
    public void RemoveSet_RemovesThatSet()
    {
        var exercise = Strength(2);
        var second = exercise.Entries[1];
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-remove]").Click();

        Assert.That(exercise.Entries, Is.EqualTo(new[] { second }));
    }

    [Test]
    public void Remove_RaisesOnRemove()
    {
        var removed = false;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(1))
            .Add(x => x.OnRemove, () => removed = true)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-remove]").Click();

        Assert.That(removed, Is.True);
    }

    [Test]
    public void Cardio_ShowsLabelledFieldsWithoutSetControls()
    {
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Cardio())
            .Add(x => x.TestId, "exercise-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".cardio .label").Select(l => TestText.Normalise(l.TextContent)),
                Is.EqualTo(new[] { "Duration *", "Distance (km)" }));
            Assert.That(cut.FindAll("[data-testid=exercise-1-add-set]"), Is.Empty);
            Assert.That(cut.FindAll(".set-row"), Is.Empty);
        });
    }

    [Test]
    public void CardioEntryErrors_ShowOneLineEachInFieldOrderAndMarkFields()
    {
        var exercise = Cardio();
        exercise.Entries[0].Errors["distance"] = "Distance error";
        exercise.Entries[0].Errors["duration"] = "Enter duration";
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.TestId, "exercise-1"));

        var lines = cut.FindAll("[data-testid=exercise-1-entry-error] .error").Select(e => e.TextContent);

        Assert.Multiple(() =>
        {
            Assert.That(lines, Is.EqualTo(new[] { "Enter duration", "Distance error" }));
            Assert.That(cut.Find("[data-testid=exercise-1-distance]").GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid^=exercise-1-duration-]").Select(i => i.GetAttribute("aria-invalid")),
                Is.All.EqualTo("true"));
        });
    }

    [Test]
    public void ExerciseError_IsShown()
    {
        var exercise = Strength(0);
        exercise.Error = "Add at least one set";
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.TestId, "exercise-1"));

        Assert.That(cut.Find("[data-testid=exercise-1-error]").TextContent, Is.EqualTo("Add at least one set"));
    }

    [Test]
    public void AddSet_ClearsExerciseError()
    {
        var exercise = Strength(0);
        exercise.Error = "Add at least one set";
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-add-set]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Error, Is.Null);
            Assert.That(cut.FindAll("[data-testid=exercise-1-error]"), Is.Empty);
        });
    }

    [Test]
    public void EditingSetWithErrors_FixedErrorClears()
    {
        var exercise = Strength(1);
        RequireReps(exercise, exercise.Entries[0]);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, RequireReps)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries[0].Errors, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-1-set-1-error]"), Is.Empty);
        });
    }

    [Test]
    public void EditingSetWithErrors_NewErrorAppears()
    {
        var exercise = Strength(1);
        exercise.Entries[0].Values["reps"] = "8";
        exercise.Entries[0].Errors["weight"] = "Weight error";
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, RequireReps)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("");

        Assert.That(cut.Find("[data-testid=exercise-1-set-1-error]").TextContent.Trim(), Is.EqualTo("Enter reps"));
    }

    [Test]
    public void EditingCardioEntryWithErrors_FixedErrorClears()
    {
        var exercise = Cardio();
        exercise.Entries[0].Errors["duration"] = "Enter duration";
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, entry) => { entry.Errors.Clear(); return true; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-duration-mm]").Input("20");

        Assert.That(cut.FindAll("[data-testid=exercise-1-entry-error]"), Is.Empty);
    }

    [Test]
    public void TypingThroughAValidValue_KeepsCheckingAndShowsTheNewError()
    {
        var exercise = Strength(1);
        WholeReps(exercise, exercise.Entries[0]);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, WholeReps)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8.");

        Assert.That(cut.Find("[data-testid=exercise-1-set-1-error]").TextContent.Trim(), Is.EqualTo("Reps must be a number"));
    }

    [Test]
    public void TypingThroughAValidValue_PlainValidator_KeepsChecking()
    {
        var exercise = Strength(1);
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, PlainWholeReps)
            .Add(x => x.TestId, "exercise-1"));
        cut.Find("[data-testid=exercise-1-add-set]").Click();

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8.");

        Assert.That(cut.Find("[data-testid=exercise-1-set-1-error]").TextContent.Trim(), Is.EqualTo("Reps must be a number"));
    }

    [Test]
    public void AddSet_Allowed_StopsCheckingTheSetOnChange()
    {
        var exercise = Strength(1);
        WholeReps(exercise, exercise.Entries[0]);
        var checks = 0;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (e, s) => { checks++; return WholeReps(e, s); })
            .Add(x => x.TestId, "exercise-1"));
        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8");
        cut.Find("[data-testid=exercise-1-add-set]").Click();
        checks = 0;

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("8.");

        Assert.Multiple(() =>
        {
            Assert.That(exercise.Entries[0].CheckOnChange, Is.False);
            Assert.That(checks, Is.Zero);
        });
    }

    [Test]
    public void Carry_OpenOtherSet_Allowed_StopsCheckingTheOpenSetOnChange()
    {
        var exercise = Carry(2);
        exercise.Entries[1].CheckOnChange = true;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, exercise)
            .Add(x => x.ValidateSet, (_, _) => true)
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-open]").Click();

        Assert.That(exercise.Entries[1].CheckOnChange, Is.False);
    }

    [Test]
    public void EditingSetWithoutErrors_IsNotChecked()
    {
        var checks = 0;
        var cut = Context.Render<ExerciseCard>(p => p
            .Add(x => x.Exercise, Strength(1))
            .Add(x => x.ValidateSet, (_, _) => { checks++; return false; })
            .Add(x => x.TestId, "exercise-1"));

        cut.Find("[data-testid=exercise-1-set-1-reps]").Input("0");

        Assert.That(checks, Is.Zero);
    }

    private static bool RequireReps(ExerciseState exercise, EntryState entry)
    {
        entry.Errors.Clear();
        if (string.IsNullOrEmpty(entry.Values.GetValueOrDefault("reps")))
        {
            entry.Errors["reps"] = "Enter reps";
        }

        return entry.Errors.Count == 0;
    }

    /// <summary>Element reference id of the input with <paramref name="testId"/>. bUnit drops it from the markup once the input re-renders, so read it first.</summary>
    private static string? ReferenceId(IRenderedComponent<ExerciseCard> cut, string testId) =>
        cut.Find($"[data-testid={testId}]").GetAttribute("blazor:elementreference");

    /// <summary>Element reference id passed to the last focus call.</summary>
    private string FocusedId() => ((ElementReference)Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0]!).Id;
}
