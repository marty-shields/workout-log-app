using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Fields;

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
            Assert.That(SetGrid.LastBelow([Time], "1:30"), Is.True);
            Assert.That(SetGrid.LastBelow([Time, Weight], null), Is.True);
            Assert.That(SetGrid.LastBelow([Reps, Weight], "60×8"), Is.False);
        });
    }

    [TestCase("—", false)]
    [TestCase("60×8", false)]
    [TestCase("62.5×6", false)]
    [TestCase("25kg×10", true)]
    [TestCase("62.5kg×12", true)]
    [TestCase("57.25kg×12", true)]
    public void LastBelow_IsTrueForATextTooLongForTheKitsAsideColumn(string last, bool below) =>
        Assert.That(SetGrid.LastBelow([Reps, Weight], last), Is.EqualTo(below));

    [Test]
    public void LastColumn_NeedsLastTimeAShortTextAndNoDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastColumn([Reps, Weight], true, "60×8"), Is.True);
            Assert.That(SetGrid.LastColumn([Reps, Weight], false, null), Is.False);
            Assert.That(SetGrid.LastColumn([Time, Weight], true, "1:30"), Is.False);
            Assert.That(SetGrid.LastColumn([Reps, Weight], true, "57.25kg×12"), Is.False);
        });
    }
}
