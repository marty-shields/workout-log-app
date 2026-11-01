using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Actions;
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
            Assert.That(cut.Find(".number").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=set-2-reps]"), Is.Not.Null);
            Assert.That(cut.Find("[data-testid=set-2-weight]"), Is.Not.Null);
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

        Assert.That(TestText.Normalise(cut.Find("[data-testid=set-1-error]").TextContent), Is.EqualTo("Enter reps"));
    }

    [Test]
    public void SeveralErrors_ShowOneLineEachInFieldOrder()
    {
        var cut = Context.Render<SetRow>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Fields)
            .Add(x => x.Entry, new EntryState { Errors = { ["weight"] = "Weight error", ["reps"] = "Reps error" } })
            .Add(x => x.TestId, "set-1"));

        var lines = cut.FindAll("[data-testid=set-1-error] .error").Select(e => e.TextContent);

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
}
