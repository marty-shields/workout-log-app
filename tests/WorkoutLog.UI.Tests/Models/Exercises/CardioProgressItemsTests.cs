using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class CardioProgressItemsTests
{
    private const int Treadmill = 300;
    private const int ExerciseBike = 301;
    private const int RowingMachine = 305;
    private const int StairClimber = 309;
    private const int VersaClimber = 310;
    private const int Run = 311;
    private const int JumpRope = 315;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    /// <summary>UTC+10, no daylight saving.</summary>
    private static readonly TimeZoneInfo Brisbane = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");

    private static readonly CardioRecords NoRecords = new(null, null, null, null, null);

    [Test]
    public void Exercises_AreTheOnesNamed() =>
        Assert.That(new[] { Treadmill, ExerciseBike, RowingMachine, StairClimber, VersaClimber, Run, JumpRope }.Select(id => Find(id).Name),
            Is.EqualTo(new[] { "Treadmill", "Exercise Bike", "Rowing Machine", "Stair Climber", "VersaClimber", "Outdoor Run", "Jump Rope" }));

    [Test]
    public void Subtitle_IsCardio() =>
        Assert.That(CardioProgressItems.Subtitle, Is.EqualTo("Cardio"));

    [TestCase(Run, new[] { "distance", "duration", "pace", "calories" })]
    [TestCase(StairClimber, new[] { "duration", "floors", "calories" })]
    [TestCase(Treadmill, new[] { "distance", "duration", "calories" })]
    [TestCase(JumpRope, new[] { "duration", "calories" })]
    public void Tabs_AreTheExercisesChartMetricsInOrder(int id, string[] keys) =>
        Assert.That(CardioProgressItems.Tabs(Find(id)).Select(t => t.Value), Is.EqualTo(keys));

    [Test]
    public void Tabs_HaveLabelsAndTestIds() =>
        Assert.That(CardioProgressItems.Tabs(Find(Run)).Select(t => (t.Label, t.TestId)),
            Is.EqualTo(new[]
            {
                ("Distance", "exercise-tab-distance"), ("Duration", "exercise-tab-duration"), ("Pace", "exercise-tab-pace"), ("Calories", "exercise-tab-calories"),
            }));

    [TestCase(Run, 5.2, "5.2km")]
    [TestCase(RowingMachine, 2000, "2000m")]
    [TestCase(VersaClimber, 500, "500ft")]
    [TestCase(Run, 5.256, "5.26km")]
    public void Format_Distance_IsInTheExercisesUnit(int id, decimal value, string text) =>
        Assert.That(CardioProgressItems.Format(Find(id), CardioProgressItems.DistanceTab)(value), Is.EqualTo(text));

    [TestCase("duration", 1800, "30:00")]
    [TestCase("duration", 3750, "1:02:30")]
    [TestCase("pace", 346, "5:46")]
    [TestCase("floors", 120, "120")]
    [TestCase("calories", 410, "410")]
    public void Format_OtherTabs_AreTimesOrNumbers(string tab, decimal value, string text) =>
        Assert.That(CardioProgressItems.Format(Find(Run), tab)(value), Is.EqualTo(text));

    [TestCase("distance", false, false, null)]
    [TestCase("duration", true, false, null)]
    [TestCase("pace", true, true, "Up = faster")]
    [TestCase("floors", false, false, null)]
    [TestCase("calories", false, false, null)]
    public void TimeScaleFlippedAndCaption_FollowTheTab(string tab, bool timeScale, bool flipped, string? caption) =>
        Assert.Multiple(() =>
        {
            Assert.That(CardioProgressItems.TimeScale(tab), Is.EqualTo(timeScale));
            Assert.That(CardioProgressItems.Flipped(tab), Is.EqualTo(flipped));
            Assert.That(CardioProgressItems.Caption(tab), Is.EqualTo(caption));
        });

    [TestCase("distance", new[] { 5.2 })]
    [TestCase("duration", new[] { 1500.0, 1800 })]
    [TestCase("pace", new[] { 346.0 })]
    [TestCase("floors", new[] { 42.0 })]
    [TestCase("calories", new[] { 300.0, 410 })]
    public void Points_AreTheSessionsWithAValueOldestFirst_TimesInSeconds(string tab, double[] values)
    {
        var progress = Progress(
            Session(1, Now.AddDays(-20), new CardioBest(null, TimeSpan.FromMinutes(25), null, 42, 300)),
            Session(2, Now.AddDays(-10), new CardioBest(5.2m, TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(346), null, 410)));

        Assert.That(CardioProgressItems.Points(progress, tab, TimeZoneInfo.Utc, Now).Select(p => p.Value), Is.EqualTo(values.Select(v => (decimal?)v)));
    }

    [Test]
    public void Points_DatesAreInThePhonesTimeZone() =>
        Assert.That(CardioProgressItems.Points(Progress(Session(1, new DateTimeOffset(2026, 10, 10, 20, 0, 0, TimeSpan.Zero), Best(5m))), "distance", Brisbane, Now)
            .Select(p => (p.Date, p.Label)), Is.EqualTo(new[] { ("11 Oct", (string?)null) }));

    [TestCase("distance", ChartRange.ThreeMonths, "No distance logged in the last 3 months")]
    [TestCase("duration", ChartRange.SixMonths, "No duration logged in the last 6 months")]
    [TestCase("pace", ChartRange.Year, "No pace logged in the last year")]
    [TestCase("floors", ChartRange.All, "No floors logged yet")]
    [TestCase("calories", ChartRange.All, "No calories logged yet")]
    public void NoDataMessage_SessionsWithNoValueForTheTab_NamesTheMetric(string tab, ChartRange range, string message) =>
        Assert.That(CardioProgressItems.NoDataMessage(Progress(Session(1, Now, new CardioBest(null, null, null, null, null))), tab, range), Is.EqualTo(message));

    [Test]
    public void NoDataMessage_AValueOrNoSessions_IsNull() =>
        Assert.Multiple(() =>
        {
            Assert.That(CardioProgressItems.NoDataMessage(Progress(Session(1, Now, Best(5m))), "distance", ChartRange.ThreeMonths), Is.Null);
            Assert.That(CardioProgressItems.NoDataMessage(Progress(), "distance", ChartRange.ThreeMonths), Is.Null);
        });

    [Test]
    public void Records_AreTheExercisesMetricsWithTheirDates()
    {
        var at = new DateTimeOffset(2026, 10, 10, 20, 0, 0, TimeSpan.Zero);
        var records = new CardioRecords(
            new ProgressRecord<decimal>(5.2m, at),
            new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(30), at),
            new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(330), at),
            new ProgressRecord<int>(99, at),
            new ProgressRecord<int>(410, at));

        Assert.That(CardioProgressItems.Records(Find(Run), new CardioProgress([], records), Brisbane, Now),
            Is.EqualTo(new[]
            {
                new RecordRow("Longest distance", "5.2km", "11 Oct"),
                new RecordRow("Longest duration", "30:00", "11 Oct"),
                new RecordRow("Fastest pace", "5:30/km", "11 Oct"),
                new RecordRow("Most calories", "410 calories", "11 Oct"),
            }));
    }

    [Test]
    public void Records_Floors_AreCountedAndDistanceLeftOut()
    {
        var at = Now.AddDays(-13);
        var records = new CardioRecords(null, new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(65), at), null, new ProgressRecord<int>(120, at), null);

        Assert.That(CardioProgressItems.Records(Find(StairClimber), new CardioProgress([], records), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[]
            {
                new RecordRow("Longest duration", "1:05:00", "15 Oct"),
                new RecordRow("Most floors", "120 floors", "15 Oct"),
                new RecordRow("Most calories", null, null),
            }));
    }

    [TestCase(RowingMachine, 2000, "2000m")]
    [TestCase(VersaClimber, 500, "500ft")]
    public void Records_Distance_IsInTheExercisesUnit(int id, decimal distance, string text) =>
        Assert.That(CardioProgressItems.Records(Find(id), new CardioProgress([], NoRecords with { LongestDistance = new(distance, Now) }), TimeZoneInfo.Utc, Now)[0],
            Is.EqualTo(new RecordRow("Longest distance", text, "28 Oct")));

    [Test]
    public void Sessions_NewestFirst_HeadlineIsTheDistanceAndDetailTheOtherValues()
    {
        var progress = Progress(
            Session(60, Now.AddDays(-24), [Entry(duration: TimeSpan.FromSeconds(1760), distance: 5m, pace: TimeSpan.FromSeconds(352))], Headline(5m, TimeSpan.FromSeconds(1760))),
            Session(61, Now.AddDays(-17), [Entry(duration: TimeSpan.FromMinutes(30), distance: 5.2m, pace: TimeSpan.FromSeconds(346), calories: 410)], Headline(5.2m, TimeSpan.FromMinutes(30))));

        Assert.That(CardioProgressItems.Sessions(Find(Run), progress, TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[]
            {
                new ExerciseSession(61, "Sun 11 Oct", "5.2km", "30:00 · 5:46/km · 410 calories"),
                new ExerciseSession(60, "Sun 4 Oct", "5km", "29:20 · 5:52/km"),
            }));
    }

    [Test]
    public void Sessions_NoDistanceLogged_HeadlineIsADash() =>
        Assert.That(CardioProgressItems.Sessions(Find(Run), Progress(Session(1, Now, [Entry(duration: TimeSpan.FromMinutes(30))], Headline(null, TimeSpan.FromMinutes(30)))), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(1, "Wed 28 Oct", "—", "30:00")));

    [Test]
    public void Sessions_LastYear_HaveTheYear() =>
        Assert.That(CardioProgressItems.Sessions(Find(JumpRope), Progress(Session(1, new DateTimeOffset(2025, 10, 15, 9, 0, 0, TimeSpan.Zero), [Entry(duration: TimeSpan.FromMinutes(10), calories: 120)], Headline(null, TimeSpan.FromMinutes(10)))), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(1, "Wed 15 Oct 2025", "10:00", "120 calories")));

    [TestCase(StairClimber, "25:00", "120 floors · 300 calories · Speed level 8 · 70 steps/min")]
    [TestCase(ExerciseBike, "12.5km", "30:00 · 300 calories · Resistance 8 · 85rpm")]
    [TestCase(RowingMachine, "6000m", "30:00 · 300 calories · Damper 8 · 28 strokes/min")]
    [TestCase(Treadmill, "5km", "30:00 · 300 calories · 2.5% · 10km/h")]
    public void Sessions_DetailHasLevelInclineAndRate(int id, string headline, string detail)
    {
        var definition = Find(id);
        var entry = id switch
        {
            StairClimber => Entry(duration: TimeSpan.FromMinutes(25), floors: 120, calories: 300, level: 8, rate: 70),
            ExerciseBike => Entry(duration: TimeSpan.FromMinutes(30), distance: 12.5m, calories: 300, level: 8, rate: 85),
            RowingMachine => Entry(duration: TimeSpan.FromMinutes(30), distance: 6000, calories: 300, level: 8, rate: 28),
            _ => Entry(duration: TimeSpan.FromMinutes(30), distance: 5, calories: 300, incline: 2.5m, rate: 10),
        };

        var session = CardioProgressItems.Sessions(definition, Progress(Session(1, Now, [entry], Headline(entry.Distance, entry.Duration))), TimeZoneInfo.Utc, Now).Single();

        Assert.That((session.Headline, session.Detail), Is.EqualTo((headline, detail)));
    }

    [Test]
    public void Sessions_TwoEntries_HeadlineIsTheBestAndDetailEachEntryInFull() =>
        Assert.That(CardioProgressItems.Sessions(Find(Run), Progress(Session(1, Now,
                [
                    Entry(duration: TimeSpan.FromMinutes(18), distance: 3m, pace: TimeSpan.FromMinutes(6), calories: 230),
                    Entry(duration: TimeSpan.FromMinutes(30), distance: 5.2m, pace: TimeSpan.FromSeconds(346), calories: 410),
                ], Headline(5.2m, TimeSpan.FromMinutes(30)))), TimeZoneInfo.Utc, Now).Single(),
            Is.EqualTo(new ExerciseSession(1, "Wed 28 Oct", "5.2km", "3km · 18:00 · 6:00/km · 230 calories / 5.2km · 30:00 · 5:46/km · 410 calories")));

    private static ExerciseDefinition Find(int id) => Catalog.All.Single(e => e.Id == id);

    private static CardioProgress Progress(params CardioSessionProgress[] sessions) => new(sessions, NoRecords);

    private static CardioBest Best(decimal distance) => new(distance, null, null, null, null);

    private static CardioSessionProgress Session(int workoutId, DateTimeOffset startedAt, CardioBest best) =>
        new(new CardioSession(workoutId, startedAt, [Entry()]), best);

    private static CardioSessionProgress Session(int workoutId, DateTimeOffset startedAt, CardioEntry[] entries, CardioBest best) =>
        new(new CardioSession(workoutId, startedAt, entries), best);

    /// <summary>Best values with only the headline metrics: distance and duration.</summary>
    private static CardioBest Headline(decimal? distance, TimeSpan? duration) => new(distance, duration, null, null, null);

    private static CardioEntry Entry(
        TimeSpan? duration = null, decimal? distance = null, int? floors = null, int? level = null, decimal? incline = null, int? calories = null, decimal? rate = null, TimeSpan? pace = null) =>
        new(duration, distance, floors, level, incline, calories, rate, pace);
}
