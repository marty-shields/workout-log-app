using Microsoft.AspNetCore.Components.Web;
using WorkoutLog.UI.Components.Inputs;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class DurationFieldTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsHoursMinutesSeconds()
    {
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.TestId, "duration"));

        var boxes = cut.FindAll("input").Select(i => i.GetAttribute("data-testid"));

        Assert.That(boxes, Is.EqualTo(new[] { "duration-h", "duration-mm", "duration-ss" }));
    }


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

    [Test]
    public void Invalid_MarksEveryBox()
    {
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Invalid, true));

        var boxes = cut.FindAll("input");

        Assert.Multiple(() =>
        {
            Assert.That(boxes.Select(i => i.ClassList.Contains("invalid")), Is.All.True);
            Assert.That(boxes.Select(i => i.GetAttribute("aria-invalid")), Is.All.EqualTo("true"));
        });
    }

    [TestCase(true, "true")]
    [TestCase(false, null)]
    public void Required_MarksEveryBox(bool required, string? expected)
    {
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Required, required));

        Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("aria-required")), Is.All.EqualTo(expected));
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

        await cut.Find(".field").FocusOutAsync(new FocusEventArgs());

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

        await cut.Find(".field").FocusOutAsync(new FocusEventArgs());

        Assert.That(raised, Is.False);
    }

    [Test]
    public async Task FocusMovesToAnotherBox_DoesNotFillIn()
    {
        string? raised = null;
        var cut = Context.Render<DurationField>(p => p
            .Add(x => x.Label, "Duration")
            .Add(x => x.Value, "1::")
            .Add(x => x.ValueChanged, (string? v) => raised = v));

        var leaving = cut.Find(".field").FocusOutAsync(new FocusEventArgs());
        await cut.Find(".field").FocusInAsync(new FocusEventArgs());
        await leaving;

        Assert.That(raised, Is.Null);
    }

    [Test]
    public void Boxes_UseNumericKeyboard()
    {
        var cut = Context.Render<DurationField>(p => p.Add(x => x.Label, "Duration"));

        Assert.That(cut.FindAll("input").Select(i => i.GetAttribute("inputmode")), Is.All.EqualTo("numeric"));
    }
}
