using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Periods;
using static WorkoutLog.Core.Tests.Domain.Workouts.Periods.PeriodTestData;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class VolumeRuleTests
{
    private readonly VolumeRule rule = new();

    [TestCase(ExerciseCategory.Lift, 5, "100", "500")]
    [TestCase(ExerciseCategory.Bodyweight, 10, "20", "200")]
    [TestCase(ExerciseCategory.Bodyweight, 10, "0", "0")]
    [TestCase(ExerciseCategory.Assisted, 10, "30", "0")]
    [TestCase(ExerciseCategory.Hold, 1, "20", "0")]
    [TestCase(ExerciseCategory.Carry, 1, "40", "0")]
    [TestCase(ExerciseCategory.Cardio, 1, "40", "0")]
    public void Of_FollowsTheRuleOfTheCategory(ExerciseCategory category, int reps, string kg, string expected) =>
        Assert.That(rule.Of(category, reps, decimal.Parse(kg, System.Globalization.CultureInfo.InvariantCulture)), Is.EqualTo(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture)));

    [TestCase(null, "100")]
    [TestCase(0, "100")]
    [TestCase(5, null)]
    [TestCase(5, "0")]
    [TestCase(null, null)]
    [TestCase(5, "-20")]
    [TestCase(-5, "100")]
    public void Of_ALiftMissingRepsOrWeight_AddsNothing(int? reps, string? kg) =>
        Assert.That(rule.Of(ExerciseCategory.Lift, reps, kg is null ? null : decimal.Parse(kg, System.Globalization.CultureInfo.InvariantCulture)), Is.Zero);

    [Test]
    public void Of_DecimalWeights_AreExact()
    {
        var sum = Enumerable.Repeat(rule.Of(ExerciseCategory.Lift, 3, 0.1m), 10).Sum();

        Assert.That(sum, Is.EqualTo(3m));
    }

    [Test]
    public void Of_DumbbellWeightIsNotDoubled() => Assert.That(rule.Of(ExerciseCategory.Lift, 10, 22.5m), Is.EqualTo(225m));

    [Test]
    public void Compare_WeekHoldingToday_CutsTheEarlierWeekAtTheSameWeekday()
    {
        var result = rule.Compare(
            [Sv("2026-10-20T08:00", 100m), Sv("2026-10-21T23:00", 200m), Sv("2026-10-22T01:00", 400m), Sv("2026-10-26T09:00", 800m), Sv("2026-10-28T12:00", 1600m), Sv("2026-10-30T09:00", 3200m)],
            Week(2026, 10, 28), At("2026-10-28T18:00"), TimeZoneInfo.Utc);

        Assert.That(result, Is.EqualTo(new PeriodVolume(Week(2026, 10, 28), 2400m, 300m)));
    }

    [Test]
    public void Compare_MonthHoldingToday_CutsTheEarlierMonthAtTheSameDay()
    {
        var result = rule.Compare(
            [Sv("2026-03-15T20:00", 100m), Sv("2026-03-16T08:00", 200m), Sv("2026-04-02T08:00", 400m), Sv("2026-04-15T10:00", 800m)],
            Month(2026, 4), At("2026-04-15T12:00"), TimeZoneInfo.Utc);

        Assert.That((result.Volume, result.Earlier), Is.EqualTo((1200m, 100m)));
    }

    [Test]
    public void Compare_PastYear_IsWholeAgainstWhole()
    {
        var result = rule.Compare(
            [Sv("2025-01-01T08:00", 100m), Sv("2025-12-31T08:00", 200m), Sv("2026-06-01T08:00", 400m), Sv("2027-01-01T08:00", 800m)],
            Year(2026), At("2027-03-01T12:00"), TimeZoneInfo.Utc);

        Assert.That(result, Is.EqualTo(new PeriodVolume(Year(2026), 400m, 300m)));
    }

    [Test]
    public void Compare_EmptyPeriod_IsZero() =>
        Assert.That(rule.Compare([], Month(2026, 10), At("2026-10-28T12:00"), TimeZoneInfo.Utc), Is.EqualTo(new PeriodVolume(Month(2026, 10), 0m, 0m)));

    [Test]
    public void Compare_PlacesASetByTheLocalDateOfItsStart()
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("plus10", TimeSpan.FromHours(10), "plus10", "plus10");

        var result = rule.Compare([Sv("2026-09-30T20:00", 100m)], Month(2026, 10), At("2026-10-28T12:00"), zone);

        Assert.That(result.Volume, Is.EqualTo(100m));
    }

    private static SetVolume Sv(string utc, decimal kg) => new(At(utc), kg);
}
