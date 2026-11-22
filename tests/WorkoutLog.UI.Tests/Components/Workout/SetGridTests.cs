using Shields.UI.Components.Inputs;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class SetGridTests
{
    private static readonly FieldSpec Reps = new("reps", Metric.Reps, "Reps", FieldKind.Whole);
    private static readonly FieldSpec Weight = new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal);
    private static readonly FieldSpec Time = new("time", Metric.Duration, "Time", FieldKind.Duration);

    [Test]
    public void OpensAsPanel_NeedsMoreThanTwoFieldsAndADuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.OpensAsPanel([Reps, Weight]), Is.False);
            Assert.That(SetGrid.OpensAsPanel([Reps, Time]), Is.False);
            Assert.That(SetGrid.OpensAsPanel([Reps, Weight, Reps]), Is.False);
            Assert.That(SetGrid.OpensAsPanel([Reps, Weight, Time]), Is.True);
            Assert.That(SetGrid.OpensAsPanel([Time, Time, Time, Time]), Is.True);
        });
    }

    [Test]
    public void ShortHeaders_AreForALastColumnOrADuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.ShortHeaders([Weight, Reps], false), Is.False);
            Assert.That(SetGrid.ShortHeaders([Weight, Reps], true), Is.True);
            Assert.That(SetGrid.ShortHeaders([Time, Weight], false), Is.True);
        });
    }

    [Test]
    public void LastBelow_IsTrueForAnyDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastBelow([Time], new LastTime(["1:30"])), Is.True);
            Assert.That(SetGrid.LastBelow([Time, Weight], null), Is.True);
            Assert.That(SetGrid.LastBelow([Reps, Weight], new LastTime(["60×8"])), Is.False);
            Assert.That(SetGrid.LastBelow([Reps, Weight], null), Is.False);
        });
    }

    [TestCase("—", false)]
    [TestCase("60×8", false)]
    [TestCase("62.5×6", false)]
    [TestCase("140×10", false)]
    [TestCase("25kg×10", false)]
    [TestCase("62.5kg×12", false)]
    [TestCase("57.25kg×12", false)]
    [TestCase("157.5kg×12", false)]
    [TestCase("1000kg×100", true)]
    [TestCase("102.25kg×12", true)]
    [TestCase("157.25kg×10", true)]
    public void TooWideForColumn_IsTrueAbove9AndAHalfDigitWidths_SoThe96pxAsideNeverOverflows(string last, bool tooWide) =>
        Assert.That(SetGrid.TooWideForColumn(last), Is.EqualTo(tooWide));

    [Test]
    public void AsideWidth_IsWideOnlyWhenATextIsWiderThanTheNormalColumn()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.AsideWidth(null), Is.EqualTo(EntryGridAsideWidth.Normal));
            Assert.That(SetGrid.AsideWidth(new LastTime(["60×8", "62.5×6", "—"])), Is.EqualTo(EntryGridAsideWidth.Normal));
            Assert.That(SetGrid.AsideWidth(new LastTime(["60×8", "140×10"])), Is.EqualTo(EntryGridAsideWidth.Wide));
            Assert.That(SetGrid.AsideWidth(new LastTime(["25kg×10"])), Is.EqualTo(EntryGridAsideWidth.Wide));
            Assert.That(SetGrid.AsideWidth(new LastTime(["57.25kg×12"])), Is.EqualTo(EntryGridAsideWidth.Wide));
        });
    }

    [Test]
    public void LastBelow_OneTooWideSetPutsEverySetBelow()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastBelow([Reps, Weight], new LastTime(["60×8", "102.25kg×12"])), Is.True);
            Assert.That(SetGrid.LastBelow([Reps, Weight], new LastTime(["60×8", "62.5×6"])), Is.False);
        });
    }

    [Test]
    public void LastColumn_NeedsLastTimeShortTextsAndNoDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastColumn([Reps, Weight], new LastTime(["60×8"])), Is.True);
            Assert.That(SetGrid.LastColumn([Reps, Weight], null), Is.False);
            Assert.That(SetGrid.LastColumn([Time, Weight], new LastTime(["1:30"])), Is.False);
            Assert.That(SetGrid.LastColumn([Reps, Weight], new LastTime(["60×8", "102.25kg×12"])), Is.False);
        });
    }
}
