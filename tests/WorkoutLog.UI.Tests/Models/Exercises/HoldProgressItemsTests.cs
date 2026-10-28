using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class HoldProgressItemsTests
{
    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly HoldSessionProgress Weighted =
        Session(41, Now.AddDays(-13), [Hold(105, null), Hold(60, 10m), Hold(80, 0m)], 105, Best(10m, 60));

    private static readonly HoldSessionProgress Unweighted = Session(40, Now.AddDays(-20), [Hold(90, null)], 90, null);

    [Test]
    public void Tabs_AreDurationThenWeight() =>
        Assert.That(HoldProgressItems.Tabs.Select(t => (t.Value, t.Label, t.TestId)),
            Is.EqualTo(new[] { ("duration", "Duration", "exercise-tab-duration"), ("weight", "Weight", "exercise-tab-weight") }));

    [TestCase(105, "1:45")]
    [TestCase(45, "0:45")]
    [TestCase(3750, "1:02:30")]
    public void Duration_IsSecondsAsClockTime(int seconds, string expected) =>
        Assert.That(HoldProgressItems.Duration(seconds), Is.EqualTo(expected));

    [Test]
    public void Points_Duration_AreTheLongestHoldsInSeconds() =>
        Assert.That(HoldProgressItems.Points(Progress(Unweighted, Weighted), HoldProgressItems.DurationTab, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("8 Oct", 90m), new ChartPoint("15 Oct", 105m) }));

    [Test]
    public void Points_Weight_AreTheHeaviestLabelledWithTheirDuration_UnweightedHasNoValue() =>
        Assert.That(HoldProgressItems.Points(Progress(Unweighted, Weighted), HoldProgressItems.WeightTab, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("8 Oct", null), new ChartPoint("15 Oct", 10m, "1:00") }));





    [Test]
    public void Records_ShowTheLongestHoldAndHeaviestWeightWithTheirDates() =>
        Assert.That(HoldProgressItems.Records(
                new HoldProgress([], new HoldRecords(
                    new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(105), Weighted.Session.StartedAt),
                    new ProgressRecord<ExerciseBest>(Best(10m, 60), Weighted.Session.StartedAt))),
                TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Longest hold", "1:45", "15 Oct"), new RecordRow("Heaviest weight", "10kg · 1:00", "15 Oct") }));

    [Test]
    public void Records_None_ShowNoValue() =>
        Assert.That(HoldProgressItems.Records(Progress(), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Longest hold", null, null), new RecordRow("Heaviest weight", null, null) }));

    [Test]
    public void Sessions_AreNewestFirstWithTheLongestAndEverySetWithoutZeroWeights() =>
        Assert.That(HoldProgressItems.Sessions(Progress(Unweighted, Weighted), TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new ExerciseSession(41, "Thu 15 Oct", "1:45", "1:45 · 10kg 1:00 · 1:20"),
            new ExerciseSession(40, "Thu 8 Oct", "1:30", "1:30"),
        }));

    private static StrengthSet Hold(int seconds, decimal? weightKg) => new(null, weightKg, TimeSpan.FromSeconds(seconds), null);

    private static ExerciseBest Best(decimal kg, int seconds) => new(kg, null, TimeSpan.FromSeconds(seconds), null);

    private static HoldSessionProgress Session(int id, DateTimeOffset startedAt, StrengthSet[] sets, int? longestSeconds, ExerciseBest? heaviest) =>
        new(new StrengthSession(id, startedAt, sets), longestSeconds is { } s ? TimeSpan.FromSeconds(s) : null, heaviest);

    private static HoldProgress Progress(params HoldSessionProgress[] sessions) => new(sessions, new HoldRecords(null, null));
}
