using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class EntryFieldTests : ComponentTestBase
{
    [Test]
    public void DurationField_RendersThreeBoxes()
    {
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("duration", Metric.Duration, "Duration", FieldKind.Duration, Required: true))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.FindAll("input"), Has.Count.EqualTo(3));
    }


    [TestCase(FieldKind.Whole, "numeric")]
    [TestCase(FieldKind.Decimal, "decimal")]
    public void NumberKinds_SetInputMode(FieldKind kind, string inputMode)
    {
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("value", Metric.WeightKg, "Value", kind))
            .Add(x => x.Entry, new EntryState()));

        Assert.That(cut.Find("input").GetAttribute("inputmode"), Is.EqualTo(inputMode));
    }

    [Test]
    public void Label_IncludesUnit()
    {
        var cut = Context.Render<EntryField>(p => p
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

        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true))
            .Add(x => x.Entry, entry));

        Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.EqualTo(hasError));
    }

    [Test]
    public void EntryErrorForOtherField_DoesNotMarkInput()
    {
        var entry = new EntryState { Errors = { ["weight"] = "Weight error" } };
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true))
            .Add(x => x.Entry, entry));

        Assert.That(cut.Find("input").HasAttribute("aria-invalid"), Is.False);
    }

    [Test]
    public void Input_WritesEntryAndRaisesOnChanged()
    {
        var entry = new EntryState();
        var changed = false;
        var cut = Context.Render<EntryField>(p => p
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

    [TestCase("1.", "1.")]
    [TestCase("1,5", "1,5")]
    [TestCase("abc", "abc")]
    public void TypedText_StaysInTheBoxAndTheEntry(string typed, string expected)
    {
        var entry = new EntryState();
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal))
            .Add(x => x.Entry, entry));

        cut.Find("input").Input(typed);

        Assert.Multiple(() =>
        {
            Assert.That(entry.Values["weight"], Is.EqualTo(typed));
            Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo(expected));
        });
    }

    [Test]
    public void ClearedInput_SetsEntryToNull()
    {
        var entry = new EntryState { Values = { ["reps"] = "5" } };
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole))
            .Add(x => x.Entry, entry));

        cut.Find("input").Input("");

        Assert.That(entry.Values["reps"], Is.Null);
    }

    [Test]
    public void EntryValue_ShowsInTheBox()
    {
        var entry = new EntryState { Values = { ["weight"] = "62.5" } };
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("weight", Metric.WeightKg, "Weight", FieldKind.Decimal))
            .Add(x => x.Entry, entry));

        Assert.That(cut.Find("input").GetAttribute("value"), Is.EqualTo("62.5"));
    }

    [Test]
    public void Enter_RaisesOnEnter()
    {
        var entered = false;
        var cut = Context.Render<EntryField>(p => p
            .Add(x => x.Field, new FieldSpec("reps", Metric.Reps, "Reps", FieldKind.Whole))
            .Add(x => x.Entry, new EntryState())
            .Add(x => x.OnEnter, () => entered = true));

        cut.Find("input").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        Assert.That(entered, Is.True);
    }
}
