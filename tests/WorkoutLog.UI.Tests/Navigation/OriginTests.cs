using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class OriginTests
{
    [Test]
    public void Route_FromThisWeek_IsThisWeek() =>
        Assert.That(Origin.Route(Origin.ThisWeek), Is.EqualTo(string.Empty));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("workouts")]
    [TestCase("dashboard")]
    [TestCase("This-week")]
    [TestCase("this-week ")]
    public void Route_AnythingElse_IsThisWeek(string? from) =>
        Assert.That(Origin.Route(from), Is.EqualTo(string.Empty));

    [TestCase(PeriodKind.Week, "2026-11-09", "trends:week:2026-11-09", "trends?period=week&date=2026-11-09")]
    [TestCase(PeriodKind.Month, "2026-11-01", "trends:month:2026-11-01", "trends?period=month&date=2026-11-01")]
    [TestCase(PeriodKind.Year, "2026-01-01", "trends:year:2026-01-01", "trends?period=year&date=2026-01-01")]
    public void Trends_IsReadBackAsThatPeriodsRoute(PeriodKind kind, string start, string expectedFrom, string expectedRoute)
    {
        var from = Origin.Trends(Period.Containing(kind, TrendsRoute.ParseDate(start)!.Value));

        Assert.Multiple(() =>
        {
            Assert.That(from, Is.EqualTo(expectedFrom));
            Assert.That(Origin.Route(from), Is.EqualTo(expectedRoute));
        });
    }

    [Test]
    public void Route_ATrendsValueWithOnlyADate_IsTheWeekHoldingIt() =>
        Assert.That(Origin.Route("trends:2026-11-11"), Is.EqualTo("trends?period=week&date=2026-11-09"));

    [TestCase("trends:")]
    [TestCase("trends:soon")]
    [TestCase("trends:2026-13-01")]
    [TestCase("trends")]
    [TestCase("Trends:2026-11-09")]
    [TestCase("trends:decade:2026-11-09")]
    [TestCase("trends:month:")]
    [TestCase("trends:month:soon")]
    [TestCase("trends:week:x:2026-11-09")]
    public void Route_ATrendsValueThatIsNotADate_IsThisWeek(string from) =>
        Assert.That(Origin.Route(from), Is.EqualTo(string.Empty));

    [Test]
    public void Workouts_WithNoOrigin_IsReadBackAsThoseDatesRoute()
    {
        var from = Origin.Workouts(new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8)), null);

        Assert.Multiple(() =>
        {
            Assert.That(from, Is.EqualTo("workouts:2026-11-02:2026-11-08"));
            Assert.That(Origin.Route(from), Is.EqualTo("workouts?first=2026-11-02&last=2026-11-08"));
        });
    }

    [Test]
    public void Workouts_KeepsWhereWorkoutsWasOpenedFromSoBackFromItStillReachesIt()
    {
        var trends = Origin.Trends(Period.Containing(PeriodKind.Month, new DateOnly(2026, 11, 18)));
        var from = Origin.Workouts(new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8)), trends);

        Assert.Multiple(() =>
        {
            Assert.That(from, Is.EqualTo("workouts:2026-11-02:2026-11-08:trends:month:2026-11-01"));
            Assert.That(Origin.Route(from), Is.EqualTo("workouts?first=2026-11-02&last=2026-11-08&from=trends%3Amonth%3A2026-11-01"));
            Assert.That(Origin.Route(Origin.Workouts(new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8)), Origin.ThisWeek)),
                Is.EqualTo("workouts?first=2026-11-02&last=2026-11-08&from=this-week"));
        });
    }

    [TestCase("workouts:")]
    [TestCase("workouts:2026-11-02")]
    [TestCase("workouts:2026-11-02:soon")]
    [TestCase("workouts:2026-11-08:2026-11-02")]
    [TestCase("Workouts:2026-11-02:2026-11-08")]
    public void Route_AWorkoutsValueThatIsNotTwoDatesInOrder_IsThisWeek(string from) =>
        Assert.That(Origin.Route(from), Is.EqualTo(string.Empty));

    [Test]
    public void Link_AddsTheOriginEscapedAsTheFromQuery() =>
        Assert.That(Origin.Link("workout/7", "trends:week:2026-11-09"), Is.EqualTo("workout/7?from=trends%3Aweek%3A2026-11-09"));

    [Test]
    public void Link_WithNoOrigin_IsTheRoute() =>
        Assert.That(Origin.Link("settings", null), Is.EqualTo("settings"));
}
