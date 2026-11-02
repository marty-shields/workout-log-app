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
}
