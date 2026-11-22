using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class EntryStateTests
{
    private static readonly FieldSpec[] Fields =
    [
        new("reps", Metric.Reps, "Reps", FieldKind.Whole, Required: true),
        new("weight", Metric.WeightKg, "Weight", FieldKind.Decimal, Unit: "kg"),
    ];

    [Test]
    public void ErrorMessages_FollowTheOrderOfTheFields()
    {
        var entry = new EntryState { Errors = { ["weight"] = "Weight error", ["reps"] = "Enter reps" } };

        Assert.That(entry.ErrorMessages(Fields), Is.EqualTo(new[] { "Enter reps", "Weight error" }));
    }

    [Test]
    public void ErrorMessages_LeaveOutAFieldWithNoError()
    {
        var entry = new EntryState { Errors = { ["weight"] = "Weight error" } };

        Assert.That(entry.ErrorMessages(Fields), Is.EqualTo(new[] { "Weight error" }));
    }

    [Test]
    public void ErrorMessages_AreEmptyWithoutErrors()
    {
        Assert.That(new EntryState().ErrorMessages(Fields), Is.Empty);
    }
}
