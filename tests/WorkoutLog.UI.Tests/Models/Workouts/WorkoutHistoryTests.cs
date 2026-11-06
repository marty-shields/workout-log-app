using WorkoutLog.Core.Domain;
using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Tests.Models.Workouts;

public class WorkoutHistoryTests
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly WorkoutHistory History = Histories.Of(
    [
        new(4, new DateTime(2026, 11, 6, 7, 0, 0)),
        new(3, new DateTime(2026, 8, 6, 23, 59, 0)),
        new(2, new DateTime(2026, 8, 5, 18, 0, 0)),
        new(1, new DateTime(2026, 8, 5, 7, 0, 0)),
    ], Today);

    [Test]
    public void Earliest_IsTheFirstWorkoutsDay() => Assert.That(History.Earliest, Is.EqualTo(new DateOnly(2026, 8, 5)));

    [Test]
    public void NoWorkouts_IsEmptyWithTodayAsEarliest()
    {
        var history = new WorkoutHistory(new Dictionary<DateOnly, int>(), Today);

        Assert.Multiple(() =>
        {
            Assert.That(history.IsEmpty, Is.True);
            Assert.That(history.Earliest, Is.EqualTo(Today));
        });
    }

    [Test]
    public void CountIn_Dates_CountsTheWorkoutsOnEveryDayIncludingBothEnds() =>
        Assert.That(History.CountIn(new DateSpan(new(2026, 8, 6), Today)), Is.EqualTo(2));

    [Test]
    public void CountIn_CountsWorkoutsNotDays() =>
        Assert.That(new[] { History.CountIn(2026, 8), History.CountIn(2026, 9), History.CountIn(2026, 11) }, Is.EqualTo(new[] { 3, 0, 1 }));

    [Test]
    public void HasWorkoutOn_IsTrueOnlyOnDaysWithOne() =>
        Assert.That(new[] { History.HasWorkoutOn(new(2026, 8, 5)), History.HasWorkoutOn(new(2026, 8, 7)) }, Is.EqualTo(new[] { true, false }));

    [TestCase(HistoryRange.ThreeMonths, 2026, 8, 6)]
    [TestCase(HistoryRange.Year, 2025, 11, 6)]
    [TestCase(HistoryRange.All, 2026, 8, 5)]
    public void Span_RunsFromTheRangesStartOrTheFirstWorkoutToToday(HistoryRange range, int year, int month, int day) =>
        Assert.That(History.Span(range), Is.EqualTo(new DateSpan(new(year, month, day), Today)));

    [Test]
    public void RangeOf_TheSpanOfARange_IsThatRange() =>
        Assert.That(Enum.GetValues<HistoryRange>().Select(r => History.RangeOf(History.Span(r))), Is.EqualTo(Enum.GetValues<HistoryRange>().Cast<HistoryRange?>()));

    [Test]
    public void RangeOf_OtherDates_IsNull() =>
        Assert.That(new[]
        {
            History.RangeOf(new DateSpan(new(2026, 8, 7), Today)),
            History.RangeOf(new DateSpan(new(2026, 8, 6), new(2026, 11, 5))),
        }, Is.All.Null);

    [Test]
    public void AStartAfterToday_IsSavedButOutsideAll()
    {
        var history = new WorkoutHistory(new Dictionary<DateOnly, int> { [Today] = 1, [Today.AddDays(1)] = 1 }, Today);

        Assert.Multiple(() =>
        {
            Assert.That(history.IsEmpty, Is.False);
            Assert.That(history.CountIn(history.Span(HistoryRange.All)), Is.EqualTo(1));
        });
    }
}
