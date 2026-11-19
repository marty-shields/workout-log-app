using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class TrendsRouteTests
{
    [TestCase(PeriodKind.Week, "trends?period=week&date=2026-11-09")]
    [TestCase(PeriodKind.Month, "trends?period=month&date=2026-11-01")]
    [TestCase(PeriodKind.Year, "trends?period=year&date=2026-01-01")]
    public void To_PutsTheKindAndTheFirstDayInTheQuery(PeriodKind kind, string route) =>
        Assert.That(TrendsRoute.To(Period.Containing(kind, new DateOnly(2026, 11, 11))), Is.EqualTo(route));

    [TestCase("week", PeriodKind.Week)]
    [TestCase("month", PeriodKind.Month)]
    [TestCase("year", PeriodKind.Year)]
    public void ParseKind_ReadsWhatKindNameWrites(string text, PeriodKind kind)
    {
        Assert.Multiple(() =>
        {
            Assert.That(TrendsRoute.ParseKind(text), Is.EqualTo(kind));
            Assert.That(TrendsRoute.KindName(kind), Is.EqualTo(text));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Week")]
    [TestCase("decade")]
    public void ParseKind_AnythingElse_IsNull(string? text) =>
        Assert.That(TrendsRoute.ParseKind(text), Is.Null);

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
