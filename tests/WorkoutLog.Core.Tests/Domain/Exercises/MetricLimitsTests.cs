using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class MetricLimitsTests
{
    [TestCase(Metric.Reps, 1_000)]
    [TestCase(Metric.WeightKg, 1_000)]
    [TestCase(Metric.Duration, 86_399)]
    [TestCase(Metric.Floors, 1_000)]
    [TestCase(Metric.Level, 100)]
    [TestCase(Metric.Incline, 100)]
    [TestCase(Metric.Calories, 10_000)]
    public void Max_PerMetric(Metric metric, decimal max)
    {
        Assert.That(MetricLimits.Max(Exercise(null, null), metric), Is.EqualTo(max));
    }

    [TestCase(DistanceUnit.Km, 1_000)]
    [TestCase(DistanceUnit.Metres, 100_000)]
    [TestCase(DistanceUnit.Feet, 100_000)]
    public void Max_DistancePerUnit(DistanceUnit unit, decimal max)
    {
        Assert.That(MetricLimits.Max(Exercise(unit, null), Metric.Distance), Is.EqualTo(max));
    }

    [TestCase(RateUnit.KmPerHour, 100)]
    [TestCase(RateUnit.MinPerKm, 60)]
    [TestCase(RateUnit.Rpm, 300)]
    [TestCase(RateUnit.StrokesPerMin, 200)]
    [TestCase(RateUnit.StridesPerMin, 300)]
    [TestCase(RateUnit.StepsPerMin, 300)]
    [TestCase(RateUnit.FeetPerMin, 1_000)]
    public void Max_RatePerUnit(RateUnit unit, decimal max)
    {
        Assert.That(MetricLimits.Max(Exercise(null, unit), Metric.Rate), Is.EqualTo(max));
    }

    [Test]
    public void Max_DistanceOrRateWithNoUnit_Throws()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => MetricLimits.Max(Exercise(null, null), Metric.Distance), Throws.ArgumentException);
            Assert.That(() => MetricLimits.Max(Exercise(null, null), Metric.Rate), Throws.ArgumentException);
        });
    }

    [Test]
    public void Max_EveryMetricOfEveryExercise_HasALimit()
    {
        var catalog = new ExerciseCatalog();

        Assert.That(() => catalog.All.SelectMany(e => e.Metrics.Select(m => MetricLimits.Max(e, m.Metric))).ToList(), Throws.Nothing);
    }

    private static ExerciseDefinition Exercise(DistanceUnit? distanceUnit, RateUnit? rateUnit) =>
        new(1, "Exercise", ExerciseType.Cardio, [], null, null, null, [], distanceUnit, rateUnit);
}
