using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class CarryProgressItemsTests
{
    private const int FarmersWalk = 187;

    private static readonly ExerciseDefinition Carry = new ExerciseCatalog().All.Single(e => e.Id == FarmersWalk);

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly CarrySessionProgress Weighted =
        Session(51, Now.AddDays(-20), [Carried(28m, 40m, null), Carried(32m, 40m, 45), Carried(0m, 30m, null)], Best(32m, 40m), Best(32m, 40m));

    private static readonly CarrySessionProgress Unweighted = Session(50, Now.AddDays(-43), [Carried(null, 60m, null)], null, Best(null, 60m));

    [Test]
    public void Exercise_IsAMetresCarry() =>
        Assert.That((Carry.Name, Carry.Category, Carry.DistanceUnit), Is.EqualTo(("Farmer's Walk", ExerciseCategory.Carry, DistanceUnit.Metres)));

    [Test]
    public void Tabs_AreWeightThenDistance() =>
        Assert.That(CarryProgressItems.Tabs.Select(t => (t.Value, t.Label, t.TestId)),
            Is.EqualTo(new[] { ("weight", "Weight", "exercise-tab-weight"), ("distance", "Distance", "exercise-tab-distance") }));

    [Test]
    public void Distance_IsInTheExercisesUnit() =>
        Assert.That(CarryProgressItems.Distance(Carry)(40.5m), Is.EqualTo("40.5m"));

    [Test]
    public void Points_Weight_AreTheHeaviestLabelledWithTheirDistanceAndSkipUnweightedSessions() =>
        Assert.That(CarryProgressItems.Points(Progress(Unweighted, Weighted), CarryProgressItems.WeightTab, Carry, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("8 Oct", 32m, "40m") }));

    [Test]
    public void Points_Distance_AreTheLongestLabelledWithTheirWeightWhenWeighted() =>
        Assert.That(CarryProgressItems.Points(Progress(Unweighted, Weighted), CarryProgressItems.DistanceTab, Carry, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("15 Sep", 60m), new ChartPoint("8 Oct", 40m, "32kg") }));

    [Test]
    public void NoDataMessage_NoSessions_IsNull() =>
        Assert.That(CarryProgressItems.NoDataMessage(Progress(), CarryProgressItems.WeightTab, ChartRange.ThreeMonths), Is.Null);

    [Test]
    public void NoDataMessage_WeightTabWithAWeightedSession_IsNull() =>
        Assert.That(CarryProgressItems.NoDataMessage(Progress(Unweighted, Weighted), CarryProgressItems.WeightTab, ChartRange.ThreeMonths), Is.Null);

    [TestCase(ChartRange.ThreeMonths, "No sets in the last 3 months")]
    [TestCase(ChartRange.All, "No sets yet")]
    public void NoDataMessage_WeightTabWithNoWeightedSession_IsNoSets(ChartRange range, string expected) =>
        Assert.That(CarryProgressItems.NoDataMessage(Progress(Unweighted), CarryProgressItems.WeightTab, range), Is.EqualTo(expected));

    [Test]
    public void NoDataMessage_DistanceTab_IsNull() =>
        Assert.That(CarryProgressItems.NoDataMessage(Progress(Unweighted), CarryProgressItems.DistanceTab, ChartRange.ThreeMonths), Is.Null);

    [Test]
    public void Records_ShowTheHeaviestWeightAndLongestDistanceWithTheirDates() =>
        Assert.That(CarryProgressItems.Records(
                new CarryProgress([], new CarryRecords(
                    new ProgressRecord<ExerciseBest>(Best(32m, 40m), Weighted.Session.StartedAt),
                    new ProgressRecord<ExerciseBest>(Best(24m, 60m), Unweighted.Session.StartedAt))),
                Carry, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Heaviest weight", "32kg · 40m", "8 Oct"), new RecordRow("Longest distance", "60m · 24kg", "15 Sep") }));

    [Test]
    public void Records_UnweightedLongestDistance_ShowsTheDistanceOnly() =>
        Assert.That(CarryProgressItems.Records(new CarryProgress([], new CarryRecords(null, new ProgressRecord<ExerciseBest>(Best(null, 60m), Unweighted.Session.StartedAt))),
                Carry, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Heaviest weight", null, null), new RecordRow("Longest distance", "60m", "15 Sep") }));

    [Test]
    public void Sessions_AreNewestFirstWithTheHeaviestOrLongestAndEverySetWithoutZeroWeights() =>
        Assert.That(CarryProgressItems.Sessions(Progress(Unweighted, Weighted), Carry, TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new ExerciseSession(51, "Thu 8 Oct", "32kg · 40m", "28kg 40m · 32kg 40m 0:45 · 30m"),
            new ExerciseSession(50, "Tue 15 Sep", "60m", "60m"),
        }));

    private static StrengthSet Carried(decimal? weightKg, decimal? metres, int? seconds) =>
        new(null, weightKg, seconds is { } s ? TimeSpan.FromSeconds(s) : null, metres);

    private static ExerciseBest Best(decimal? kg, decimal metres) => new(kg, null, null, metres);

    private static CarrySessionProgress Session(int id, DateTimeOffset startedAt, StrengthSet[] sets, ExerciseBest? heaviest, ExerciseBest? longest) =>
        new(new StrengthSession(id, startedAt, sets), heaviest, longest);

    private static CarryProgress Progress(params CarrySessionProgress[] sessions) => new(sessions, new CarryRecords(null, null));
}
