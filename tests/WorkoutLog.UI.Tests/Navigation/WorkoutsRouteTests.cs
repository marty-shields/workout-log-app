using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class WorkoutsRouteTests
{
    private static readonly DateSpan Week = new(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8));

    [Test]
    public void To_PutsTheDatesInTheQuery() =>
        Assert.That(WorkoutsRoute.To(Week), Is.EqualTo("workouts?first=2026-11-02&last=2026-11-08"));

    [Test]
    public void To_WithAnOrigin_AddsItEscaped() =>
        Assert.That(WorkoutsRoute.To(Week, "trends:week:2026-11-02"), Is.EqualTo("workouts?first=2026-11-02&last=2026-11-08&from=trends%3Aweek%3A2026-11-02"));

    [Test]
    public void ParseDates_ReadsWhatToWrote() =>
        Assert.That(WorkoutsRoute.ParseDates("2026-11-02", "2026-11-08"), Is.EqualTo(Week));

    [Test]
    public void ParseDates_OneDay_IsThatDay() =>
        Assert.That(WorkoutsRoute.ParseDates("2026-11-02", "2026-11-02"), Is.EqualTo(new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 2))));

    [TestCase(null, null)]
    [TestCase("2026-11-02", null)]
    [TestCase(null, "2026-11-08")]
    [TestCase("soon", "2026-11-08")]
    [TestCase("2026-11-02", "2026-13-08")]
    [TestCase("2026-11-08", "2026-11-02")]
    public void ParseDates_AMissingBadOrBackwardsPair_IsNull(string? first, string? last) =>
        Assert.That(WorkoutsRoute.ParseDates(first, last), Is.Null);
}
