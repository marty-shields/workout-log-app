using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class FieldInputTests : ComponentTestBase
{
    [Test]
    public void DurationField_RendersThreeBoxes()
    {
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.FindAll("input"), Has.Count.EqualTo(3));
    }

    [Test]
    public void PaceField_RendersTwoBoxes()
    {
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("rate", Metric.Rate, "Pace", FieldKind.Pace, Unit: "min/km"))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.FindAll("input"), Has.Count.EqualTo(2));
    }

    [TestCase(FieldKind.Whole, "numeric")]
    [TestCase(FieldKind.Decimal, "decimal")]
    public void NumberKinds_SetInputMode(FieldKind kind, string inputMode)
    {
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("value", Metric.WeightKg, "Value", kind))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.Find("input").GetAttribute("inputmode"), Is.EqualTo(inputMode));
    }

    [Test]
    public void Label_IncludesUnit()
    {
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(TestText.Normalise(cut.Find(".label").TextContent), Is.EqualTo("Weight (kg)"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EntryErrorForField_MarksInputInvalid(bool hasError)
    {
        var entry = new EntryState();
        if (hasError)
        {
            entry.Errors["reps"] = "Enter reps";
        }

        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true))
            .Add(x => x.Entry, entry));

        Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.EqualTo(hasError));
    }

    [Test]
    public void EntryErrorForOtherField_DoesNotMarkInput()
    {
        var entry = new EntryState { Errors = { ["weight"] = "Weight error" } };
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true))
            .Add(x => x.Entry, entry));

        Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.False);
    }

    [Test]
    public void Input_WritesEntryAndRaisesOnChanged()
    {
        var entry = new EntryState();
        var changed = false;
        var cut = Context.Render<FieldInput>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true))
            .Add(x => x.Entry, entry)
            .Add(x => x.OnChanged, () => changed = true));

        cut.Find("input").Input("10");

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values["reps"], Is.EqualTo("10"));
            Assert.That(changed, Is.True);
        });
    }
}
