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

    [Test]
    public void Trends_IsReadBackAsThatWeeksRoute()
    {
        var from = Origin.Trends(new DateOnly(2026, 11, 9));

        Assert.Multiple(() =>
        {
            Assert.That(from, Is.EqualTo("trends:2026-11-09"));
            Assert.That(Origin.Route(from, "exercises"), Is.EqualTo("trends?period=week&date=2026-11-09"));
        });
    }

    [TestCase("trends:")]
    [TestCase("trends:soon")]
    [TestCase("trends:2026-13-01")]
    [TestCase("trends")]
    [TestCase("Trends:2026-11-09")]
    public void Route_ATrendsValueThatIsNotADate_IsTheList(string from) =>
        Assert.That(Origin.Route(from, "exercises"), Is.EqualTo("exercises"));
}
