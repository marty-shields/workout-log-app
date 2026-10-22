using WorkoutLog.UI.Components.Inputs;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class NumberFieldTests : ComponentTestBase
{
    [TestCase(false, "numeric")]
    [TestCase(true, "decimal")]
    public void AllowDecimal_SetsInputMode(bool allowDecimal, string inputMode)
    {
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Weight (kg)")
            .Add(x => x.AllowDecimal, allowDecimal));

        Assert.That(cut.Find("input").GetAttribute("inputmode"), Is.EqualTo(inputMode));
    }

    [Test]
    public void Input_RaisesValueChanged()
    {
        string? value = null;
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Reps")
            .Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("input").Input("12");

        Assert.That(value, Is.EqualTo("12"));
    }

    [Test]
    public void EmptyInput_RaisesValueChangedWithNull()
    {
        string? value = "12";
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Reps")
            .Add(x => x.Value, "12")
            .Add(x => x.ValueChanged, (string? v) => value = v));

        cut.Find("input").Input("");

        Assert.That(value, Is.Null);
    }

    [Test]
    public void Required_MarksField()
    {
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Reps")
            .Add(x => x.Required, true));

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find(".label").TextContent), Is.EqualTo("Reps *"));
            Assert.That(cut.Find("input").GetAttribute("aria-required"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Invalid_MarksInput()
    {
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Reps")
            .Add(x => x.Invalid, true));

        var input = cut.Find("input");

        Assert.Multiple(() =>
        {
            Assert.That(input.ClassList, Does.Contain("invalid"));
            Assert.That(input.GetAttribute("aria-invalid"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Valid_DoesNotMarkInput()
    {
        var cut = Context.Render<NumberField>(p => p.Add(x => x.Label, "Reps"));

        var input = cut.Find("input");

        Assert.Multiple(() =>
        {
            Assert.That(input.ClassList, Does.Not.Contain("invalid"));
            Assert.That(input.HasAttribute("aria-invalid"), Is.False);
        });
    }

    [Test]
    public void ShowLabelFalse_HidesLabelVisually()
    {
        var cut = Context.Render<NumberField>(p => p
            .Add(x => x.Label, "Reps")
            .Add(x => x.ShowLabel, false));

        Assert.That(cut.Find(".label").ClassList, Does.Contain("visually-hidden"));
    }
}
