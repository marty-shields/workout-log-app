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
    public void Collapses_NeedsMoreThanTwoFieldsAndADuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.Collapses([Reps, Weight]), Is.False);
            Assert.That(SetGrid.Collapses([Reps, Time]), Is.False);
            Assert.That(SetGrid.Collapses([Reps, Weight, Reps]), Is.False);
            Assert.That(SetGrid.Collapses([Reps, Weight, Time]), Is.True);
            Assert.That(SetGrid.Collapses([Time, Time, Time, Time]), Is.True);
        });
    }

    [Test]
    public void Columns_AreTheNumberOneColumnPerFieldAndTheRemoveButton()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.Columns([Reps], false), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) var(--touch-target)"));
            Assert.That(SetGrid.Columns([Weight, Reps], false), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
            Assert.That(SetGrid.Columns([Reps, Time], false), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) minmax(0, 2fr) var(--touch-target)"));
        });
    }

    [Test]
    public void Columns_WithLast_AddALastColumnAfterTheNumber()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.Columns([Weight, Reps], true), Is.EqualTo("var(--set-number-width) var(--set-last-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
        });
    }

    [Test]
    public void LastBelow_IsTrueForAnyDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastBelow([Time]), Is.True);
            Assert.That(SetGrid.LastBelow([Time, Weight]), Is.True);
            Assert.That(SetGrid.LastBelow([Reps, Weight]), Is.False);
        });
    }

    [Test]
    public void LastColumn_NeedsLastTimeAndNoDuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.LastColumn([Reps, Weight], true), Is.True);
            Assert.That(SetGrid.LastColumn([Reps, Weight], false), Is.False);
            Assert.That(SetGrid.LastColumn([Time, Weight], true), Is.False);
        });
    }

    [Test]
    public void Columns_WithLast_LeaveOutTheLastColumnForADuration()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SetGrid.Columns([Time, Weight], true), Is.EqualTo(SetGrid.Columns([Time, Weight], false)));
            Assert.That(SetGrid.Columns([Time, Weight], true), Is.EqualTo("var(--set-number-width) minmax(0, 2fr) minmax(0, 1fr) var(--touch-target)"));
        });
    }
}
