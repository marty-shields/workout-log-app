using Shields.UI.Components.Inputs;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Tests.Models.Fields;

public class FieldSpecTests
{
    [TestCase(FieldKind.Whole, ValueKind.Whole)]
    [TestCase(FieldKind.Decimal, ValueKind.Fractional)]
    [TestCase(FieldKind.Duration, ValueKind.Duration)]
    public void ValueKind_MatchesTheFieldKind(FieldKind kind, ValueKind expected) =>
        Assert.That(new FieldSpec("f", Metric.Reps, "F", kind).ValueKind, Is.EqualTo(expected));

    [Test]
    public void ValueKind_RejectsAKindItDoesNotKnow() =>
        Assert.That(() => new FieldSpec("f", Metric.Reps, "F", (FieldKind)99).ValueKind, Throws.TypeOf<ArgumentOutOfRangeException>());
}
