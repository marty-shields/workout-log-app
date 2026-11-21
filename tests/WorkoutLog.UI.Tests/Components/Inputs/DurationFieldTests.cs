using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Shields.UI.Components.Inputs;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class DurationFieldTests : ComponentTestBase
{

    [Test]
    public void Value_FillsBoxes()
    {
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "1:02:03"));

        var values = cut.FindAll("input").Select(i => i.GetAttribute("value"));

        Assert.That(values, Is.EqualTo(new[] { "1", "02", "03" }));
    }

    [Test]
    public void BoxInput_RaisesJoinedValue()
    {
        string? value = null;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "0::")
            .Add(x => x.ValueChanged, (string? v) => value = v)
            .Add(x => x.TestId, "duration"));

        cut.Find("[data-testid=duration-mm]").Input("45");

        Assert.That(value, Is.EqualTo("0:45:"));
    }

    [Test]
    public void AllBoxesEmpty_RaisesNull()
    {
        string? value = "0::";
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "0::")
            .Add(x => x.ValueChanged, (string? v) => value = v)
            .Add(x => x.TestId, "duration"));

        cut.Find("[data-testid=duration-h]").Input("");

        Assert.That(value, Is.Null);
    }

    [TestCase("1::", "1:00:00")]
    [TestCase("::22", "0:00:22")]
    [TestCase("1:5:3", "1:05:03")]
    public async Task FocusLeavesPartlyFilledField_FillsIn(string value, string expected)
    {
        string? raised = null;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        PassFocusMoveWait();
        await leaving;

        Assert.That(raised, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("0:20:00")]
    public async Task FocusLeavesEmptyOrComplete_RaisesNothing(string? value)
    {
        var raised = false;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, (string? _) => raised = true));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        PassFocusMoveWait();
        await leaving;

        Assert.That(raised, Is.False);
    }

    [Test]
    public async Task FocusEntersThenLeavesPartlyFilledField_FillsIn()
    {
        string? raised = null;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "1::")
            .Add(x => x.ValueChanged, (string? v) => raised = v));

        await cut.Find("[role=group]").FocusInAsync(new FocusEventArgs());
        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        PassFocusMoveWait();
        await leaving;

        Assert.That(raised, Is.EqualTo("1:00:00"));
    }

    [Test]
    public async Task FocusMovesToAnotherBox_DoesNotFillIn()
    {
        string? raised = null;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "1::")
            .Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find("[role=group]").FocusOutAsync(new FocusEventArgs());
        await cut.Find("[role=group]").FocusInAsync(new FocusEventArgs());
        PassFocusMoveWait();
        await leaving;

        Assert.That(raised, Is.Null);
    }

    // The kit keeps its wait private; its own tests advance by the same 10 ms.
    private static readonly TimeSpan FocusMoveWait = TimeSpan.FromMilliseconds(10);

    // The component waits on the context's FakeTimeProvider, which only moves here, so no test depends on real time.
    private void PassFocusMoveWait() => ((FakeTimeProvider)Context.Services.GetRequiredService<TimeProvider>()).Advance(FocusMoveWait);
}
