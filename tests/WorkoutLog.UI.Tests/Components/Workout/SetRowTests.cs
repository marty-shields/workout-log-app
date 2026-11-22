using Shields.UI.Components.Actions;
using Shields.UI.Components.Inputs;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class SetRowTests : ComponentTestBase
{
    private static readonly FieldSpec[] Fields =
    [
        new("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
        new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
    ];

    private static readonly FieldSpec[] HoldFields =
    [
        new("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true),
        new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg", HideZero: true),
    ];

    [Test]
    public void Render_ShowsNumberAndOneInputPerField()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-2"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".index").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=set-2-reps]"), Is.Not.Null);
            Assert.That(cut.Find("[data-testid=set-2-weight]"), Is.Not.Null);
        });
    }

    [Test]
    public void Last_Unset_LeavesTheColumnOut()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".aside"), Is.Empty);
            Assert.That(cut.Find(SetMarkup.OpenRow).GetAttribute("style"),
                Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
        });
    }

    [TestCase("60×8")]
    [TestCase("—")]
    public void Last_ShowsItAfterTheNumberInItsOwnColumn(string last)
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.Last, last)
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".index + .aside"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=set-1-last]").TextContent, Is.EqualTo($"Last time {last}"));
            Assert.That(cut.Find(".aside .visually-hidden").TextContent, Is.EqualTo("Last time "));
            Assert.That(cut.Find(SetMarkup.OpenRow).GetAttribute("style"),
                Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) var(--entry-grid-aside-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
        });
    }

    [Test]
    public void Last_WideAside_WidensTheColumn()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.Last, "57.25kg×12")
            .Add(x => x.AsideWidth, EntryGridAsideWidth.Wide)
            .Add(x => x.TestId, "set-1"));

        Assert.That(cut.Find(SetMarkup.OpenRow).GetAttribute("style"),
            Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) var(--entry-grid-aside-width-wide) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
    }

    [TestCase("1:30", "Last time 1:30")]
    [TestCase("12.5kg · 1:30", "Last time 12.5kg · 1:30")]
    public void Last_OnADurationRow_ShowsALineUnderTheRowAndNoColumn(string last, string text)
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, HoldFields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.Last, last)
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".aside"), Is.Empty);
            Assert.That(cut.FindAll($"{SetMarkup.OpenRow} + .indent [data-testid=set-1-last]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=set-1-last]").TextContent, Is.EqualTo(text));
            Assert.That(cut.Find(SetMarkup.OpenRow).GetAttribute("style"),
                Is.EqualTo("grid-template-columns: var(--entry-grid-index-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
        });
    }

    [Test]
    public void Last_WhenTheCardSaysBelow_ShowsALineUnderTheRowAndNoColumnOnARowWithoutADuration()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.Last, "60×8")
            .Add(x => x.LastBelow, true)
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".aside"), Is.Empty);
            Assert.That(cut.Find($"{SetMarkup.OpenRow} + .indent [data-testid=set-1-last]").TextContent, Is.EqualTo("Last time 60×8"));
        });
    }

    [Test]
    public void Last_OnADurationRow_Unset_ShowsNoLine()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 3)
            .Add(x => x.Fields, HoldFields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-3"));

        Assert.That(cut.FindAll("[data-testid$=-last]"), Is.Empty);
    }

    [Test]
    public void Last_IsNotAValueOfTheSet()
    {
        var entry = new EntryState();
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.Last, "60×8")
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values, Is.Empty);
            Assert.That(cut.Find("[data-testid=set-1-reps]").GetAttribute("value"), Is.Null.Or.Empty);
            Assert.That(cut.Find("[data-testid=set-1-weight]").GetAttribute("value"), Is.Null.Or.Empty);
        });
    }

    [TestCase(null, "Remove set 2")]
    [TestCase("Squat", "Remove Squat set 2")]
    public void Remove_NamesTheSet(string? exerciseName, string expected)
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.ExerciseName, exerciseName)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-2"));

        Assert.That(cut.Find("[data-testid=set-2-remove]").GetAttribute("aria-label"), Is.EqualTo(expected));
    }

    [Test]
    public void Remove_IsASmallMutedCross()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-2"));

        var remove = cut.FindComponent<IconButton>().Instance;

        Assert.That((remove.TestId, remove.Icon, remove.Muted, remove.Size), Is.EqualTo(("set-2-remove", IconName.Close, true, IconSize.Small)));
    }

    [Test]
    public void Remove_RaisesOnRemove()
    {
        var removed = false;
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.OnRemove, () => removed = true)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-remove]").Click();

        Assert.That(removed, Is.True);
    }

    [Test]
    public void EntryError_IsShown()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState { Errors = { ["reps"] = "Enter reps" } })
            .Add(x => x.TestId, "set-1"));

        Assert.That(TestText.Normalise(cut.Find("[data-testid=set-1-error]").TextContent.Trim()), Is.EqualTo("Enter reps"));
    }

    [Test]
    public void SeveralErrors_ShowOneLineEachInFieldOrder()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState { Errors = { ["weight"] = "Weight error", ["reps"] = "Reps error" } })
            .Add(x => x.TestId, "set-1"));

        var lines = cut.FindAll("[data-testid=set-1-error] li").Select(e => e.TextContent);

        Assert.That(lines, Is.EqualTo(new[] { "Reps error", "Weight error" }));
    }

    [Test]
    public void EntryError_MarksOnlyFieldsAtFault()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState { Errors = { ["reps"] = "Enter reps" } })
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=set-1-reps]").GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=set-1-weight]").HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public void NoErrors_ShowsNoErrorBlock()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        Assert.That(cut.FindAll("[data-testid=set-1-error]"), Is.Empty);
    }

    [Test]
    public async Task Focus_NoErrors_FocusesTheFirstField()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-reps]"));
    }

    [Test]
    public async Task Focus_DurationFirst_FocusesTheDurationsFirstBox()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, HoldFields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-duration-h]"));
    }

    [Test]
    public async Task Focus_ErrorOnTheDuration_FocusesTheDurationsFirstBox()
    {
        var entry = new EntryState();
        entry.Errors["duration"] = "Enter a duration";
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, [HoldFields[1], HoldFields[0]])
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-duration-h]"));
    }

    [Test]
    public async Task Focus_WithErrors_FocusesTheFirstFieldAtFault()
    {
        var entry = new EntryState();
        entry.Errors["weight"] = "Weight must be a number";
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-weight]"));
    }

    [Test]
    public void IsRecord_ShowsTheBadgeUnderTheRowInLineWithTheFields()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.IsRecord, true)
            .Add(x => x.TestId, "set-2"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=set-2-record]").TextContent.Trim(), Is.EqualTo("Record"));
            Assert.That(cut.Find("[data-testid=set-2-record]").ParentElement!.ClassList, Does.Contain("indent"));
        });
    }

    [Test]
    public void NotRecord_ShowsNoBadge()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-2"));

        Assert.That(cut.FindAll("[data-testid=set-2-record]"), Is.Empty);
    }

    [Test]
    public void NumberBoxes_AreCompactAndHaveABoxTestId()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=set-1-weight-box]").ClassList, Does.Contain("input-box-compact"));
            Assert.That(cut.Find("[data-testid=set-1-reps-box]").ClassList, Does.Contain("input-box-compact"));
        });
    }

    [Test]
    public void Input_WritesEntryAndRaisesOnChanged()
    {
        var entry = new EntryState();
        var changed = 0;
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.OnChanged, () => changed++)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-reps]").Input("10");

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values["reps"], Is.EqualTo("10"));
            Assert.That(changed, Is.EqualTo(1));
        });
    }

    [Test]
    public void ClearedInput_SetsEntryToNull()
    {
        var entry = new EntryState { Values = { ["reps"] = "5" } };
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-reps]").Input("");

        Assert.That(entry.Values["reps"], Is.Null);
    }

    [TestCase("1.")]
    [TestCase("1,5")]
    [TestCase("abc")]
    [TestCase("-")]
    [TestCase(" 5")]
    [TestCase(".5")]
    [TestCase("007")]
    public void TypedText_StaysInTheBoxAndTheEntry(string typed)
    {
        var entry = new EntryState();
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-weight]").Input(typed);

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values["weight"], Is.EqualTo(typed));
            Assert.That(cut.Find("[data-testid=set-1-weight]").GetAttribute("value"), Is.EqualTo(typed));
        });
    }

    [TestCase("1.")]
    [TestCase("1,5")]
    [TestCase("abc")]
    [TestCase("-")]
    [TestCase(" 5")]
    [TestCase(".5")]
    [TestCase("007")]
    public void TypedText_SurvivesARedrawFromTheEntry(string typed)
    {
        var entry = new EntryState();
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));
        cut.Find("[data-testid=set-1-weight]").Input(typed);

        entry.Errors["weight"] = "Weight must be a number";
        cut.Render();

        Assert.That(cut.Find("[data-testid=set-1-weight]").GetAttribute("value"), Is.EqualTo(typed));
    }

    [TestCase("22,75")]
    [TestCase("abc")]
    [TestCase("007")]
    public void ReopenedSetWithRejectedText_ShowsItAsTypedAndStaysInvalid(string rejected)
    {
        var entry = new EntryState { Values = { ["weight"] = rejected }, Errors = { ["weight"] = "Weight must be a number" } };
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        var input = cut.Find("[data-testid=set-1-weight]");

        Assert.Multiple(() =>
        {
            Assert.That(input.GetAttribute("value"), Is.EqualTo(rejected));
            Assert.That(input.GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(entry.Values["weight"], Is.EqualTo(rejected));
        });
    }

    [Test]
    public void EntryValue_ShowsInTheBox()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState { Values = { ["weight"] = "62.5" } })
            .Add(x => x.TestId, "set-1"));

        Assert.That(cut.Find("[data-testid=set-1-weight]").GetAttribute("value"), Is.EqualTo("62.5"));
    }

    [Test]
    public void Enter_InTheLastField_RaisesOnEnterLast()
    {
        var entered = false;
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.OnEnterLast, () => entered = true)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-weight]").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        Assert.That(entered, Is.True);
    }

    [Test]
    public void DurationField_WritesTheJoinedTextToTheEntry()
    {
        var entry = new EntryState();
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, HoldFields)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-duration-mm]").Input("45");

        Assert.That(entry.Values["duration"], Is.EqualTo(":45:"));
    }

    [Test]
    public void DurationField_IsTheKitsCompactOne_SoThreeBoxesFitAnEqualColumn()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, HoldFields)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        Assert.That(cut.Find($"{SetMarkup.OpenRow} .parts").ClassList, Does.Contain("compact"));
    }
}
