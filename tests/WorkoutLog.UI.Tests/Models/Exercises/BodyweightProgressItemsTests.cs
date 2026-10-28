using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class BodyweightProgressItemsTests
{
    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly BodyweightSessionProgress Unweighted =
        Session(2, Now.AddDays(-15), [Set(25, null), Set(20, 0m), Set(18, null)], 25, 63, null, null);

    private static readonly BodyweightSessionProgress Mixed =
        Session(3, Now.AddDays(-8), [Set(20, null), Set(8, 16m), Set(10, 14m)], 20, 20, Top(16m, 8), 16m * (1 + 8 / 30m));

    [Test]
    public void Tabs_AreBestSetTotalAndTopSet() =>
        Assert.That(BodyweightProgressItems.Tabs(loaded: false).Select(t => (t.Value, t.Label, t.TestId)), Is.EqualTo(new[]
        {
            ("best-set", "Best set", "exercise-tab-best-set"), ("total", "Total", "exercise-tab-total"), ("top-set", "Top set", "exercise-tab-top-set"),
        }));

    [Test]
    public void Tabs_Loaded_AddE1rm() =>
        Assert.That(BodyweightProgressItems.Tabs(loaded: true).Select(t => t.Value), Is.EqualTo(new[] { "best-set", "total", "top-set", "e1rm" }));

    [TestCase("best-set", 25, "25")]
    [TestCase("total", 63, "63")]
    [TestCase("top-set", 16, "16kg")]
    [TestCase("e1rm", 20.2667, "20.3kg")]
    public void Format_FitsTheTab(string tab, decimal value, string expected) =>
        Assert.That(BodyweightProgressItems.Format(tab)(value), Is.EqualTo(expected));

    [Test]
    public void Points_BestSet_AreTheMostRepsUnlabelled() =>
        Assert.That(BodyweightProgressItems.Points(Progress(Unweighted, Mixed), "best-set", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("13 Oct", 25), new ChartPoint("20 Oct", 20) }));

    [Test]
    public void Points_Total_AreTheUnweightedRepsUnlabelled() =>
        Assert.That(BodyweightProgressItems.Points(Progress(Unweighted, Mixed), "total", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("13 Oct", 63), new ChartPoint("20 Oct", 20) }));

    [Test]
    public void Points_TopSet_SessionWithNoWeightedSet_HasNoValue() =>
        Assert.That(BodyweightProgressItems.Points(Progress(Unweighted, Mixed), "top-set", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("13 Oct", null), new ChartPoint("20 Oct", 16m, "×8") }));

    [Test]
    public void Points_E1rm_AreTheUnroundedEstimates() =>
        Assert.That(BodyweightProgressItems.Points(Progress(Unweighted, Mixed), "e1rm", TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("13 Oct", null), new ChartPoint("20 Oct", 16m * (1 + 8 / 30m)) }));





    [Test]
    public void Records_AreTheMostRepsAndHeaviestWeightWithTheirDates() =>
        Assert.That(BodyweightProgressItems.Records(Progress(Records()), loaded: false, TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new RecordRow("Most reps in a set", "25 reps", "13 Oct"),
            new RecordRow("Most reps in a session", "63 reps", "13 Oct"),
            new RecordRow("Heaviest weight", "16kg × 8", "20 Oct"),
        }));

    [Test]
    public void Records_Loaded_AddTheBestE1rm() =>
        Assert.That(BodyweightProgressItems.Records(Progress(Records()), loaded: true, TimeZoneInfo.Utc, Now)[^1],
            Is.EqualTo(new RecordRow("Best e1RM", "20.3kg", "20 Oct")));

    [Test]
    public void Records_None_HaveNoValues() =>
        Assert.That(BodyweightProgressItems.Records(Progress(new BodyweightRecords(null, null, null, null)), loaded: true, TimeZoneInfo.Utc, Now)
                .Select(r => (r.Value, r.Date)),
            Is.All.EqualTo(((string?)null, (string?)null)));

    [Test]
    public void Sessions_NewestFirstWithTheTopSetOrMostRepsAndEverySet() =>
        Assert.That(BodyweightProgressItems.Sessions(Progress(Unweighted, Mixed), TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new ExerciseSession(3, "Tue 20 Oct", "16kg × 8", "20 · 16×8 · 14×10"),
            new ExerciseSession(2, "Tue 13 Oct", "25 reps", "25 · 20 · 18"),
        }));

    [Test]
    public void Sessions_NoReps_ShowADash() =>
        Assert.That(BodyweightProgressItems.Sessions(Progress(Session(4, Now, [Set(null, null)], null, null, null, null)), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(4, "Wed 28 Oct", "—", "—")));

    private static BodyweightRecords Records() => new(
        new ProgressRecord<int>(25, Unweighted.Session.StartedAt),
        new ProgressRecord<int>(63, Unweighted.Session.StartedAt),
        new ProgressRecord<ExerciseBest>(Top(16m, 8), Mixed.Session.StartedAt),
        new ProgressRecord<decimal>(16m * (1 + 8 / 30m), Mixed.Session.StartedAt));

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);

    private static BodyweightSessionProgress Session(
        int id, DateTimeOffset startedAt, StrengthSet[] sets, int? bestSet, int? total, ExerciseBest? top, decimal? e1rm) =>
        new(new StrengthSession(id, startedAt, sets), bestSet, total, top, e1rm);

    private static BodyweightProgress Progress(params BodyweightSessionProgress[] sessions) => new(sessions, new BodyweightRecords(null, null, null, null));

    private static BodyweightProgress Progress(BodyweightRecords records) => new([], records);
}
