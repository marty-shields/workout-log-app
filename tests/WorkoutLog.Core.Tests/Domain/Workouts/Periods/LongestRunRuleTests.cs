using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Tests.Domain.Workouts.Consistency;
using static WorkoutLog.Core.Tests.Domain.Workouts.Periods.PeriodTestData;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

public class LongestRunRuleTests
{
    private static readonly DateTimeOffset Now = At("2026-10-28T12:00");

    private readonly LongestRunRule rule = new();

    private static DateTimeOffset[] Weeks(params string[] mondays) => mondays.Select(m => At(m + "T09:00")).ToArray();

    [Test]
    public void Longest_FindsTheLongestRunInTheYear()
    {
        var result = rule.Longest(Weeks("2026-01-05", "2026-01-12", "2026-03-02", "2026-03-09", "2026-03-16", "2026-04-20"), 2026, Now, TimeZoneInfo.Utc);

        Assert.That(result, Is.EqualTo(new LongestRun(3, false)));
    }

    [Test]
    public void Longest_ManyWorkoutsInAWeek_CountOneWeek() =>
        Assert.That(rule.Longest([At("2026-03-02T09:00"), At("2026-03-04T09:00"), At("2026-03-06T09:00")], 2026, Now, TimeZoneInfo.Utc), Is.EqualTo(new LongestRun(1, false)));

    [Test]
    public void Longest_EqualRuns_TakesTheLaterOne()
    {
        var result = rule.Longest(Weeks("2026-10-12", "2026-10-19", "2026-01-05", "2026-01-12"), 2026, Now, TimeZoneInfo.Utc);

        Assert.That(result, Is.EqualTo(new LongestRun(2, true)));
    }

    [TestCase("2026-10-19", "2026-10-26", true)]
    [TestCase("2026-10-12", "2026-10-19", true)]
    [TestCase("2026-10-05", "2026-10-12", false)]
    public void Longest_IsRunningWhenItReachesThisWeekOrLast(string first, string last, bool running) =>
        Assert.That(rule.Longest(Weeks(first, last), 2026, Now, TimeZoneInfo.Utc), Is.EqualTo(new LongestRun(2, running)));

    [Test]
    public void Longest_RunCrossingNewYear_IsCutAtTheYearEnds()
    {
        var starts = Weeks("2026-12-14", "2026-12-21", "2026-12-28", "2027-01-04", "2027-01-11");

        Assert.That((rule.Longest(starts, 2026, At("2027-02-01T12:00"), TimeZoneInfo.Utc), rule.Longest(starts, 2027, At("2027-02-01T12:00"), TimeZoneInfo.Utc)),
            Is.EqualTo((new LongestRun(3, false), new LongestRun(2, false))));
    }

    [Test]
    public void Longest_NoWorkoutsInTheYear_IsZero() =>
        Assert.That(rule.Longest(Weeks("2025-03-03"), 2026, Now, TimeZoneInfo.Utc), Is.EqualTo(new LongestRun(0, false)));

    [Test]
    public void Longest_NoWorkouts_IsZero() =>
        Assert.That(rule.Longest([], 2026, Now, TimeZoneInfo.Utc), Is.EqualTo(new LongestRun(0, false)));

    [Test]
    public void Longest_UsesTheLocalWeek()
    {
        // 14:30 UTC on Sunday 11 Oct is Monday 12 Oct at UTC+10, so the weeks of the 5th and 12th are both there.
        var starts = new[] { At("2026-10-06T09:00"), At("2026-10-11T14:30") };

        Assert.That(rule.Longest(starts, 2026, Now, ConsistencyZones.Plus10), Is.EqualTo(new LongestRun(2, false)));
    }
}
