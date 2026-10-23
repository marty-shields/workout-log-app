using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Stories;

public class SampleDataTests
{
    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    private const int Squat = 6;
    private const int FarmersWalk = 187;
    private const int Treadmill = 300;
    private const int OutdoorRun = 311;

    [Test]
    public void ValidateSet_NegativeWeight_IsAnError()
    {
        Assert.That(Errors(Squat, "8", "-2.5"), Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Weight must be 0 or more" }));
    }

    [Test]
    public void ValidateSet_ZeroReps_IsAnError()
    {
        Assert.That(Errors(Squat, "0", "60"), Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be more than 0" }));
    }

    [Test]
    public void ValidateSet_ZeroWeight_IsValid()
    {
        Assert.That(Errors(Squat, "8", "0"), Is.Empty);
    }

    [TestCase("0")]
    [TestCase("-5")]
    public void ValidateSet_RequiredDistanceNotMoreThanZero_IsAnError(string distance)
    {
        Assert.That(Errors(FarmersWalk, distance, "32", null),
            Is.EqualTo(new Dictionary<string, string> { ["distance"] = "Distance must be more than 0" }));
    }

    [Test]
    public void ValidateSet_CardioOptionalZeroAndNegative_OnlyNegativeIsAnError()
    {
        Assert.That(Errors(Treadmill, "0:20:00", "0", "-1", "0", null),
            Is.EqualTo(new Dictionary<string, string> { ["incline"] = "Incline must be 0 or more" }));
    }

    [Test]
    public void ValidateSet_NotANumber_IsAnError()
    {
        Assert.That(Errors(Squat, "8", "1.2.3"), Is.EqualTo(new Dictionary<string, string> { ["weight"] = "Weight must be a number" }));
    }

    [Test]
    public void ValidateSet_DecimalInAWholeField_IsAnError()
    {
        Assert.That(Errors(Squat, "8.5", "60"), Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be a whole number" }));
    }

    [Test]
    public void ValidateSet_AboveTheLimit_IsAnError()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Errors(Squat, "1001", "1000"), Is.EqualTo(new Dictionary<string, string> { ["reps"] = "Reps must be 1,000 or less" }));
            Assert.That(Errors(FarmersWalk, "100001", "32", null),
                Is.EqualTo(new Dictionary<string, string> { ["distance"] = "Distance must be 100,000 or less" }));
            Assert.That(Errors(Treadmill, "24:00:00", "1001", "101", "10001", "101"), Is.EqualTo(new Dictionary<string, string>
            {
                ["duration"] = "Duration must be 23:59:59 or less",
                ["distance"] = "Distance must be 1,000 or less",
                ["incline"] = "Incline must be 100 or less",
                ["calories"] = "Calories must be 10,000 or less",
                ["rate"] = "Rate must be 100 or less",
            }));
            Assert.That(Errors(OutdoorRun, "0:30:00", null, null, "60:01"),
                Is.EqualTo(new Dictionary<string, string> { ["rate"] = "Pace must be 60:00 or less" }));
        });
    }

    [Test]
    public void ValidateSet_AtTheLimit_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Errors(Treadmill, "23:59:59", "1000", "100", "10000", "100"), Is.Empty);
            Assert.That(Errors(OutdoorRun, "0:30:00", null, null, "60:00"), Is.Empty);
        });
    }

    [Test]
    public void ValidateSet_OptionalDurationOfZero_IsValid()
    {
        Assert.That(Errors(FarmersWalk, "40", "32", "0:00:00"), Is.Empty);
    }

    [Test]
    public void ValidateSet_NegativeTimeParts_AreErrors()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Errors(FarmersWalk, "40", "32", "1:-5:00"),
                Is.EqualTo(new Dictionary<string, string> { ["duration"] = "Duration minutes and seconds must be 00–59" }));
            Assert.That(Errors(FarmersWalk, "40", "32", "-1:00:00"),
                Is.EqualTo(new Dictionary<string, string> { ["duration"] = "Duration must be 0 or more" }));
            Assert.That(Errors(OutdoorRun, "0:30:00", null, null, "5:-30"),
                Is.EqualTo(new Dictionary<string, string> { ["rate"] = "Pace seconds must be 00–59" }));
        });
    }

    private static Dictionary<string, string> Errors(int exerciseId, params string?[] values)
    {
        var exercise = SampleData.Exercise(Catalog, exerciseId, values);
        var entry = exercise.Entries.Single();
        SampleData.ValidateSet(exercise, entry);
        return entry.Errors;
    }
}
