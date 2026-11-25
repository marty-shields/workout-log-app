using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.ThisWeek;

namespace WorkoutLog.UI.Tests.Models.ThisWeek;

public class UnfinishedItemTests
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1");

    /// <summary>Wed 18 Nov 2026, 10:00 UTC; 11:00 in UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void From_ShowsTheStartOnThePhonesClockAndTheCounts()
    {
        var item = UnfinishedItem.From(new UnfinishedSummary(new DateTimeOffset(2026, 11, 17, 17, 12, 0, TimeSpan.Zero), 4, 12), Zone, Now);

        Assert.That(item, Is.EqualTo(new UnfinishedItem("Started yesterday, 6:12 pm", 4, 12)));
    }

    [Test]
    public void From_StartedAfterMidnightLocally_IsTheNextDayInThePhonesZone()
    {
        var item = UnfinishedItem.From(new UnfinishedSummary(new DateTimeOffset(2026, 11, 17, 23, 30, 0, TimeSpan.Zero), 1, 1), Zone, Now);

        Assert.That(item.Started, Is.EqualTo("Started today, 12:30 am"));
    }

    [Test]
    public void Lost_NamesTheExercisesAndSetsLost() =>
        Assert.That(new UnfinishedItem("", 4, 12).Lost, Is.EqualTo("4 exercises, 12 sets will be lost"));

    [Test]
    public void Lost_OneOfEach_IsSingular() =>
        Assert.That(new UnfinishedItem("", 1, 1).Lost, Is.EqualTo("1 exercise, 1 set will be lost"));
}
