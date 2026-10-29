using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class WorkedOutTests
{
    private static readonly ExerciseCatalog Catalog = new();

    [TestCase("Outdoor Run", true)]
    [TestCase("Outdoor Walk", true)]
    [TestCase("Treadmill", false)]
    [TestCase("Rowing Machine", false)]
    public void HasPace_ExercisesWithAPaceUnit(string name, bool expected) =>
        Assert.That(WorkedOut.HasPace(Find(name)), Is.EqualTo(expected));

    [TestCase("Treadmill", true)]
    [TestCase("Outdoor Cycle", true)]
    [TestCase("Outdoor Run", false)]
    [TestCase("Exercise Bike", false)]
    public void HasSpeed_ExercisesWithKmPerHour(string name, bool expected) =>
        Assert.That(WorkedOut.HasSpeed(Find(name)), Is.EqualTo(expected));

    [Test]
    public void Pace_IsDurationPerKm() =>
        Assert.That(WorkedOut.Pace(TimeSpan.FromMinutes(30), 5m), Is.EqualTo(TimeSpan.FromMinutes(6)));

    [Test]
    public void Pace_IsUnrounded() =>
        Assert.That(WorkedOut.Pace(TimeSpan.FromMinutes(30), 5.2m), Is.EqualTo(TimeSpan.FromTicks(3_461_538_461)));

    [Test]
    public void Speed_IsKmPerHour() =>
        Assert.That(WorkedOut.Speed(TimeSpan.FromMinutes(30), 5.2m), Is.EqualTo(10.4m));

    [Test]
    public void Speed_IsUnrounded() =>
        Assert.That(WorkedOut.Speed(TimeSpan.FromMinutes(7), 1m), Is.EqualTo(60m / 7m));

    [TestCase(null, 5.0)]
    [TestCase(1800, null)]
    [TestCase(0, 5.0)]
    [TestCase(1800, 0.0)]
    public void PaceAndSpeed_WithoutBothValuesOver0_AreNull(int? seconds, double? km)
    {
        var duration = seconds is { } s ? TimeSpan.FromSeconds(s) : (TimeSpan?)null;
        var distance = (decimal?)km;

        Assert.Multiple(() =>
        {
            Assert.That(WorkedOut.Pace(duration, distance), Is.Null);
            Assert.That(WorkedOut.Speed(duration, distance), Is.Null);
        });
    }

    [TestCase(3_461_538_461, 346)]
    [TestCase(3_465_000_000, 347)]
    [TestCase(3_464_999_999, 346)]
    public void RoundPace_IsToTheSecondHalvesAwayFromZero(long ticks, int seconds) =>
        Assert.That(WorkedOut.RoundPace(TimeSpan.FromTicks(ticks)), Is.EqualTo(TimeSpan.FromSeconds(seconds)));

    [TestCase(10.4, 10.4)]
    [TestCase(8.575, 8.58)]
    [TestCase(8.574999, 8.57)]
    public void RoundSpeed_IsTo2PlacesHalvesAwayFromZero(decimal speed, decimal expected) =>
        Assert.That(WorkedOut.RoundSpeed(speed), Is.EqualTo(expected));

    private static ExerciseDefinition Find(string name) => Catalog.All.Single(e => e.Name == name);
}
