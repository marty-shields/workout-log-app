using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class TrendsRouteTests
{
    [Test]
    public void Week_PutsTheKindAndTheDateInTheQuery() =>
        Assert.That(TrendsRoute.Week(new DateOnly(2026, 11, 9)), Is.EqualTo("trends?period=week&date=2026-11-09"));

    [Test]
    public void ParseDate_ReadsWhatFormatWrites() =>
        Assert.That(TrendsRoute.ParseDate(TrendsRoute.Format(new DateOnly(2026, 1, 5))), Is.EqualTo(new DateOnly(2026, 1, 5)));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("2026-1-5")]
    [TestCase("05/01/2026")]
    [TestCase("2026-02-30")]
    [TestCase("2026-11-09T10:00")]
    public void ParseDate_AnythingElse_IsNull(string? text) =>
        Assert.That(TrendsRoute.ParseDate(text), Is.Null);
}
