using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class SetPanelTests : ComponentTestBase
{
    private static readonly FieldSpec[] Carry =
    [
        new("distance", Metric.Distance, "Distance", FieldKind.Decimal, Required: true, Unit: "m"),
        new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
        new("duration", Metric.Duration, "Duration", FieldKind.Duration),
    ];

    [Test]
    public void Render_ShowsTitleAndLabelsEveryField()
    {
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState()));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("Set 2"));
            Assert.That(cut.FindAll(".label").Select(l => TestText.Normalise(l.TextContent)),
                Is.EqualTo(new[] { "Distance (m) *", "Weight (kg)", "Duration" }));
        });
    }

    [Test]
    public void Duration_TakesFullWidth()
    {
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        Assert.That(cut.Find(".wide").QuerySelector("[data-testid=set-1-duration-h]"), Is.Not.Null);
    }

    [Test]
    public void Input_WritesEntryAndRaisesOnChanged()
    {
        var entry = new EntryState();
        var changed = false;
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, entry)
            .Add(x => x.OnChanged, () => changed = true)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-distance]").Input("40");

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values["distance"], Is.EqualTo("40"));
            Assert.That(changed, Is.True);
        });
    }

    [TestCase(null, "Remove set 2")]
    [TestCase("Farmer's Walk", "Remove Farmer's Walk set 2")]
    public void Remove_NamesTheSet(string? exerciseName, string expected)
    {
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 2)
            .Add(x => x.ExerciseName, exerciseName)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-2"));

        Assert.That(cut.Find("[data-testid=set-2-remove]").GetAttribute("aria-label"), Is.EqualTo(expected));
    }

    [Test]
    public void Remove_RaisesOnRemove()
    {
        var removed = false;
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.OnRemove, () => removed = true)
            .Add(x => x.TestId, "set-1"));

        cut.Find("[data-testid=set-1-remove]").Click();

        Assert.That(removed, Is.True);
    }

    [Test]
    public void Errors_ShowLinesAndMarkFields()
    {
        var entry = new EntryState { Errors = { ["distance"] = "Enter distance", ["duration"] = "Duration error" } };
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=set-1-error] .error").Select(e => e.TextContent),
                Is.EqualTo(new[] { "Enter distance", "Duration error" }));
            Assert.That(cut.Find("[data-testid=set-1-distance]").GetAttribute("aria-invalid"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=set-1-weight]").HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public async Task Focus_NoErrors_FocusesTheFirstField()
    {
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-distance]"));
    }

    [Test]
    public async Task Focus_WithErrors_FocusesTheFirstFieldAtFault()
    {
        var entry = new EntryState();
        entry.Errors["weight"] = "Weight must be a number";
        var cut = Context.Render<SetPanel>(p => p
            .Add(x => x.Number, 1)
            .Add(x => x.Fields, Carry)
            .Add(x => x.Entry, entry)
            .Add(x => x.TestId, "set-1"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=set-1-weight]"));
    }
}
