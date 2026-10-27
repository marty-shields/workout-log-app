using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class SetSummaryTests : ComponentTestBase
{
    private static readonly FieldSpec[] Carry =
    [
        new("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
        new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        new("duration", Metric.Duration, "Duration", FieldKind.Duration),
    ];

    private static EntryState Entry(params (string Key, string? Value)[] values)
    {
        var entry = new EntryState();
        foreach (var (key, value) in values)
        {
            entry.Values[key] = value;
        }

        return entry;
    }

    [Test]
    public async Task Focus_FocusesTheSummaryLine()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, Entry())
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-open]"));
    }

    [Test]
    public void FilledSet_ShowsValuesWithUnitsAndDuration()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, Entry(("distance", "40"), ("weight", "32"), ("duration", "0:00:45"))));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".number").TextContent, Is.EqualTo("1"));
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("40m · 32kg · 0:00:45"));
        });
    }

    [Test]
    public void Errors_OutlineTheSummary()
    {
        var entry = Entry(("weight", "32"));
        entry.Errors["distance"] = "Enter distance";

        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set"));

        Assert.That(cut.Find("[data-testid=set-open]").ClassList, Does.Contain("invalid"));
    }

    [Test]
    public void NoErrors_LeaveTheSummaryUnmarked()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, Entry(("distance", "40")))
            .Add(x => x.TestId, "set"));

        Assert.That(cut.Find("[data-testid=set-open]").ClassList, Does.Not.Contain("invalid"));
    }

    [Test]
    public void EmptyFields_AreLeftOut()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, Entry(("distance", "30"), ("weight", null), ("duration", "::"))));

        Assert.That(cut.Find(".text").TextContent, Is.EqualTo("30m"));
    }

    [Test]
    public void FieldWithoutUnit_UsesLabel()
    {
        FieldSpec[] fields = [new("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true)];
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, fields)
            .Add(x => x.Entry, Entry(("reps", "8"))));

        Assert.That(cut.Find(".text").TextContent, Is.EqualTo("8 reps"));
    }

    [Test]
    public void NothingEntered_ShowsEmpty()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Empty"));
    }

    [Test]
    public void Errors_ShowUnderSummaryInFieldOrder()
    {
        var entry = new EntryState { Errors = { ["duration"] = "Duration error", ["distance"] = "Enter distance" } };
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        var lines = cut.FindAll("[data-testid=set-1-error] .error").Select(e => e.TextContent);

        Assert.That(lines, Is.EqualTo(new[] { "Enter distance", "Duration error" }));
    }

    [Test]
    public void Button_ReadsAsSetNumberAndValues()
    {
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, Entry(("distance", "40")))
            .Add(x => x.TestId, "set-2"));

        Assert.That(TestText.Normalise(cut.Find("[data-testid=set-2-open]").TextContent), Is.EqualTo("Set 2 40m"));
    }

    [Test]
    public void Tap_RaisesOnOpen()
    {
        var opened = false;
        var cut = Context.Render<SetSummary>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.OnOpen, () => opened = true)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-open]").Click();

        Assert.That(opened, Is.True);
    }
}
