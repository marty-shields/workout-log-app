using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class OriginTests
{
    [Test]
    public void Route_FromTheDashboard_IsTheDashboard() =>
        Assert.That(Origin.Route("dashboard", "workouts"), Is.EqualTo(string.Empty));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("workouts")]
    [TestCase("Dashboard")]
    [TestCase("dashboard ")]
    public void Route_AnythingElse_IsTheList(string? from) =>
        Assert.That(Origin.Route(from, "workouts"), Is.EqualTo("workouts"));

    [TestCase(PeriodKind.Week, "2026-11-09", "trends:week:2026-11-09", "trends?period=week&date=2026-11-09")]
    [TestCase(PeriodKind.Month, "2026-11-01", "trends:month:2026-11-01", "trends?period=month&date=2026-11-01")]
    [TestCase(PeriodKind.Year, "2026-01-01", "trends:year:2026-01-01", "trends?period=year&date=2026-01-01")]
    public void Trends_IsReadBackAsThatPeriodsRoute(PeriodKind kind, string start, string expectedFrom, string expectedRoute)
    {
        var from = Origin.Trends(Period.Containing(kind, TrendsRoute.ParseDate(start)!.Value));

        Assert.Multiple(() =>
        {
            Assert.That(from, Is.EqualTo(expectedFrom));
            Assert.That(Origin.Route(from, "exercises"), Is.EqualTo(expectedRoute));
        });
    }

    [Test]
    public void Route_ATrendsValueWithOnlyADate_IsTheWeekHoldingIt() =>
        Assert.That(Origin.Route("trends:2026-11-11", "exercises"), Is.EqualTo("trends?period=week&date=2026-11-09"));

    [TestCase("trends:")]
    [TestCase("trends:soon")]
    [TestCase("trends:2026-13-01")]
    [TestCase("trends")]
    [TestCase("Trends:2026-11-09")]
    [TestCase("trends:decade:2026-11-09")]
    [TestCase("trends:month:")]
    [TestCase("trends:month:soon")]
    [TestCase("trends:week:x:2026-11-09")]
    public void Route_ATrendsValueThatIsNotADate_IsTheList(string from) =>
        Assert.That(Origin.Route(from, "exercises"), Is.EqualTo("exercises"));
}
