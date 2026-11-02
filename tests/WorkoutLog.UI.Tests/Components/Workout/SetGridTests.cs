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
            Assert.That(SetGrid.Columns([Reps]), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) var(--touch-target)"));
            Assert.That(SetGrid.Columns([Weight, Reps]), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) minmax(0, 1fr) var(--touch-target)"));
            Assert.That(SetGrid.Columns([Reps, Time]), Is.EqualTo("var(--set-number-width) minmax(0, 1fr) minmax(0, 2fr) var(--touch-target)"));
        });
    }
}
