using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class TrendsPageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Run = 311;

    /// <summary>Wed 18 Nov 2026, 10:00 UTC; 11:00 in UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Monday = new(2026, 11, 16);
    private static readonly DateOnly LastMonday = new(2026, 11, 9);
    private static readonly DateOnly FirstMonday = new(2026, 10, 26);

    private static readonly Period Week = Period.Containing(PeriodKind.Week, Monday);

    private FakeTimeProvider time = null!;
    private FakePeriodTotalsService totals = null!;
    private FakePeriodTrainingService training = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private FakeLogger<TrendsPage> logger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        time = new FakeTimeProvider(Now);
        time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(time);
        totals = new FakePeriodTotalsService(time);
        Context.Services.AddSingleton<IPeriodTotalsService>(totals);
        training = new FakePeriodTrainingService();
        Context.Services.AddSingleton<IPeriodTrainingService>(training);
        logger = new FakeLogger<TrendsPage>();
        Context.Services.AddSingleton<ILogger<TrendsPage>>(logger);
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerWithTheStepperAndTheTabBar()
    {
        training.HoldMuscles = true;

        var cut = Context.Render<TrendsPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("trends-link"));
        });
    }

    [Test]
    public void WeekMonthAndYearAreOffered_WithTheWeekPickedByDefault()
    {
        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-kind] input").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "trends-week", "trends-month", "trends-year" }));
            Assert.That(cut.Find("[data-testid=trends-week]").HasAttribute("checked"), Is.True);
        });
    }

    [Test]
    public void Loading_ReadsEverythingTogetherForTheWeekAndItsLimits()
    {
        training.HoldMuscles = true;

        Context.Render<TrendsPage>();

        Assert.Multiple(() =>
        {
            Assert.That(totals.Comparisons, Is.EqualTo(new[] { Monday }));
            Assert.That(totals.VolumeReads, Is.EqualTo(new[] { Monday }));
            Assert.That(totals.SeriesReads, Is.EqualTo(new[] { Monday }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026 }));
            Assert.That(totals.LimitReads, Is.EqualTo(1));
            Assert.That(training.MuscleReads, Is.EqualTo(new[] { Monday }));
            Assert.That(training.RecordReads, Is.EqualTo(new[] { Monday }));
        });
    }

    [Test]
    public void Loaded_CurrentWeek_ShowsTheTilesAgainstTheSamePointLastWeek()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=trends-tiles-workouts] .value").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=trends-tiles-time] .value").TextContent, Is.EqualTo("1h 30m"));
            Assert.That(cut.Find("[data-testid=trends-tiles-sets] .value").TextContent, Is.EqualTo("20"));
            Assert.That(cut.Find("[data-testid=trends-tiles-volume] .value").TextContent, Is.EqualTo("4,200 kg"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against Monday to Wednesday last week"));
            Assert.That(cut.Find("[data-testid=trends-days-chart]").GetAttribute("aria-label"), Is.EqualTo("Time trained each day this week beside the same day last week"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("5-week streak"));
        });
    }

    [Test]
    public void Loaded_Monday_ComparesWithMondayLastWeek()
    {
        MidWeek();
        time.SetUtcNow(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero));

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against Monday last week")));
    }

    [Test]
    public void Loaded_ShowsEachDayWithTheSameDayLastWeek()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-days-chart-values] li"), Has.Count.EqualTo(7)));
        Assert.That(cut.FindAll("[data-testid=trends-days-chart-values] li").Select(li => li.TextContent), Is.EqualTo(new[]
        {
            "Monday: 30m (last week 1h 05m)",
            "Tuesday: rest day (last week rest day)",
            "Wednesday: 1h 00m (last week 55m)",
            "Thursday: still to come (last week rest day)",
            "Friday: still to come (last week 1h 00m)",
            "Saturday: still to come (last week rest day)",
            "Sunday: still to come (last week rest day)",
        }));
    }

    [Test]
    public void Loaded_ShowsTheRecordsAndTheMuscleGroups()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid^=trends-records-row-]"), Has.Count.EqualTo(2)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("2 this week"));
            Assert.That(cut.FindAll("[data-testid^=trends-records-row-]")
                    .Select(r => (r.GetAttribute("data-testid"), r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent)),
                Is.EqualTo(new[]
                {
                    ("trends-records-row-6", "Barbell Back Squat · 70kg × 3", "Wednesday"),
                    ("trends-records-row-311", "Outdoor Run · 5.2km", "Monday"),
                }));
            Assert.That(cut.Find("[data-testid=trends-muscles-legs] .sets").TextContent, Is.EqualTo("12 sets"));
            Assert.That(cut.Find("[data-testid=trends-muscles-chest] .sets").TextContent, Is.EqualTo("Not trained"));
        });
    }

    [Test]
    public void MuscleGroupRows_AreNotTappable()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-muscles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=trends-muscles] button"), Is.Empty);
    }

    [Test]
    public void WeekWithNoWorkouts_ShowsZerosAndWhatIsMissing()
    {
        totals.Limits = new PeriodLimits(Period.Containing(PeriodKind.Week, FirstMonday), Week);
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-02"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-tiles-workouts] .value").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=trends-days-chart-none]").TextContent, Is.EqualTo("No workouts that week"));
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that week"));
            Assert.That(cut.FindAll("[data-testid=trends-muscles] .sets").Select(s => s.TextContent), Is.All.EqualTo("Not trained"));
            Assert.That(cut.FindAll("[data-testid=trends-recent-weeks-aside]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void NoWorkoutsEver_ShowsTheEmptyCurrentWeekWithBothArrowsDisabled()
    {
        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-days-chart-none]").TextContent, Is.EqualTo("No workouts this week"));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [TestCase("comparison")]
    [TestCase("volume")]
    [TestCase("series")]
    [TestCase("streaks")]
    [TestCase("limits")]
    [TestCase("muscles")]
    public void ARequiredReadFails_ShowsTheErrorKeepsTheStepperAndLogsIt(string failing)
    {
        MidWeek();
        Fail(failing);

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-error]").TextContent, Is.EqualTo("Couldn't load Trends")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-stepper]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=tab-bar]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsLoadFailed, LogLevel.Warning, "Loading Trends failed") }));
        });
    }

    [Test]
    public void LoadFails_FocusesTryAgain()
    {
        MidWeek();
        Fail("series");

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=trends-retry]")));
    }

    [Test]
    public async Task TryAgain_ShowsTheSpinnerWhileReloadingThenTheWeek()
    {
        MidWeek();
        Fail("muscles");
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-retry]"), Has.Count.EqualTo(1)));
        MidWeek();
        training.HoldMuscles = true;

        cut.Find("[data-testid=trends-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(training.MuscleReads, Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });

        training.CompleteMuscles();
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
    }

    [Test]
    public void RecordsFail_LeavesTheirCardEmptyShowsTheRestAndLogsIt()
    {
        MidWeek();
        training.Records = new ReadFailed();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=trends-records-row-]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records this week"));
            Assert.That(cut.FindAll("[data-testid=trends-muscles]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-days]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsRecordsFailed, LogLevel.Warning, "Loading the records for Trends failed; the card is empty") }));
        });
    }

    [Test]
    public void Limits_CurrentWeekAfterTheFirst_PreviousEnabledNextDisabled()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));
        Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Limits_FirstWeek_PreviousDisabledNextEnabled()
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-10-28"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("26 Oct – 1 Nov"));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Previous_ShowsTheWeekBeforeAndRaisesItsMonday()
    {
        MidWeek();
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));

        cut.Find("[data-testid=trends-stepper-previous]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("9 – 15 Nov")));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Week, LastMonday) }));
            Assert.That(totals.Comparisons.Last(), Is.EqualTo(LastMonday));
            Assert.That(training.RecordReads.Last(), Is.EqualTo(LastMonday));
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against the week before"));
            Assert.That(cut.Find("[data-testid=trends-days-chart-none]").TextContent, Is.EqualTo("No workouts that week"));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("5-week streak"));
        });
    }

    [Test]
    public void Next_ReturnsToTheCurrentWeekAndDisablesNext()
    {
        MidWeek();
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False));

        cut.Find("[data-testid=trends-stepper-next]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov")));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Week }));
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public async Task SteppingWhileLoading_ShowsTheLastWeekRequestedNotAnEarlierSlowerOne()
    {
        MidWeek();
        training.HoldMuscles = true;
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09"));
        cut.WaitForAssertion(() => Assert.That(training.MuscleReads, Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-stepper-next]").Click();
        Assert.That(training.MuscleReads, Is.EqualTo(new[] { LastMonday, Monday }));
        Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
        training.CompleteMuscles(1);
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty));
        training.CompleteMuscles(0);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=trends-tiles-workouts] .value").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against Monday to Wednesday last week"));
        });
    }

    [Test]
    public void Stepping_ShowsTheNewWeeksLabelAtOnceWhileItLoads()
    {
        MidWeek();
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));
        training.HoldMuscles = true;

        cut.Find("[data-testid=trends-stepper-previous]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("9 – 15 Nov"));
        });
    }

    [Test]
    public void Stepping_KeepsTheStreakAndLimitsItReadBefore()
    {
        MidWeek();
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));

        cut.Find("[data-testid=trends-stepper-previous]").Click();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("9 – 15 Nov")));

        Assert.Multiple(() =>
        {
            Assert.That(totals.LimitReads, Is.EqualTo(1));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026 }));
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("5-week streak"));
        });
    }

    [Test]
    public async Task NextOnTheCurrentWeekAndPreviousOnTheFirst_DoNothing()
    {
        MidWeek();
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var view = cut.FindComponent<TrendsView>().Instance;

        await cut.InvokeAsync(view.OnNext.InvokeAsync);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.Empty);
            Assert.That(totals.Comparisons, Has.Count.EqualTo(1));
            Assert.That(training.MuscleReads, Has.Count.EqualTo(1));
        });

        var first = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-10-28").Add(x => x.OnPeriodChanged, changed.Add));
        first.WaitForAssertion(() => Assert.That(first.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var reads = totals.Comparisons.Count;
        var muscleReads = training.MuscleReads.Count;

        await first.InvokeAsync(first.FindComponent<TrendsView>().Instance.OnPrevious.InvokeAsync);

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.Empty);
            Assert.That(totals.Comparisons, Has.Count.EqualTo(reads));
            Assert.That(training.MuscleReads, Has.Count.EqualTo(muscleReads));
            Assert.That(first.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("26 Oct – 1 Nov"));
        });
    }

    [Test]
    public void TodayWithNoWorkout_IsARestDayAndMarkedAsToday()
    {
        MidWeek();
        totals.Series = new PeriodSeries(Week, [new PeriodBucket(Monday, Monday, 1, TimeSpan.FromMinutes(30))], [], [], 0);

        var cut = Context.Render<TrendsPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-days-chart-values] li"), Has.Count.EqualTo(7)));
        var values = cut.FindAll("[data-testid=trends-days-chart-values] li").Select(li => li.TextContent).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(values[2], Is.EqualTo("Wednesday: rest day (last week rest day)"));
            Assert.That(values[3], Is.EqualTo("Thursday: still to come (last week rest day)"));
            Assert.That(cut.FindAll("[data-testid=trends-days] span.current").Select(e => e.TextContent), Is.EqualTo(new[] { "Wed" }));
        });
    }

    [Test]
    public void OpenedOnAWeekBeforeTheFirstWorkout_ShowsTheFirstWeekAndRaisesIt()
    {
        MidWeek();
        var changed = new List<Period>();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2001-01-01").Add(x => x.OnPeriodChanged, changed.Add));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("26 Oct – 1 Nov")));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Week, FirstMonday) }));
            Assert.That(totals.Comparisons.Last(), Is.EqualTo(FirstMonday));
            Assert.That(totals.LimitReads, Is.EqualTo(1));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
        });
    }

    [TestCase("2026-11-09", "9 – 15 Nov")]
    [TestCase("2026-11-11", "9 – 15 Nov")]
    [TestCase("2026-11-15", "9 – 15 Nov")]
    [TestCase("2026-11-17", "16 – 22 Nov")]
    [TestCase("2026-12-01", "16 – 22 Nov")]
    [TestCase("nonsense", "16 – 22 Nov")]
    [TestCase("2026-13-40", "16 – 22 Nov")]
    [TestCase("", "16 – 22 Nov")]
    [TestCase(null, "16 – 22 Nov")]
    public void OpenedFromALink_ShowsTheWeekHoldingTheDateOrTheCurrentWeek(string? date, string label)
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "week").Add(x => x.DateQuery, date));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo(label)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Month")]
    [TestCase("nonsense")]
    public void OpenedWithNoKnownKind_ShowsTheWeek(string? kind)
    {
        MidWeek();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, kind).Add(x => x.DateQuery, "2026-11-09"));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("9 – 15 Nov")));
        Assert.That(cut.Find("[data-testid=trends-week]").HasAttribute("checked"), Is.True);
    }

    [Test]
    public void OpenedFromALink_ReadsThatWeekAndTheWeeksStreakRowStillEndsNow()
    {
        MidWeek();

        Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09"));

        Assert.Multiple(() =>
        {
            Assert.That(totals.Comparisons, Is.EqualTo(new[] { LastMonday }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026 }));
        });
    }

    [Test]
    public void OpenedOnADateFromAnEarlierYear_TheStreakRowStillReadsTheCurrentYear()
    {
        MidWeek();
        totals.Limits = new PeriodLimits(Period.Containing(PeriodKind.Week, new DateOnly(2025, 12, 29)), Week);

        Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2025-12-31"));

        Assert.Multiple(() =>
        {
            Assert.That(totals.Comparisons, Is.EqualTo(new[] { new DateOnly(2025, 12, 29) }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026 }));
        });
    }

    [Test]
    public void TheLinkChanges_LoadsTheNewWeek()
    {
        MidWeek();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09"));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.DateQuery, "2026-11-02"));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2 – 8 Nov")));
        Assert.That(totals.Comparisons, Is.EqualTo(new[] { LastMonday, new DateOnly(2026, 11, 2) }));
    }

    [Test]
    public void TheLinkCatchesUpWithAStep_DoesNotLoadAgain()
    {
        MidWeek();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09"));
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));
        cut.Find("[data-testid=trends-stepper-previous]").Click();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2 – 8 Nov")));

        cut.Render(p => p.Add(x => x.DateQuery, "2026-11-02"));

        Assert.That(totals.Comparisons, Is.EqualTo(new[] { LastMonday, new DateOnly(2026, 11, 2) }));
    }

    [Test]
    public void Record_Tapped_RaisesOnOpenExerciseWithItsIdAndTheWeekShown()
    {
        MidWeek();
        var opened = new List<RecordOpened>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-11").Add(x => x.OnOpenExercise, opened.Add));

        cut.WaitForElement("button[data-testid=trends-records-row-311]").Click();

        Assert.That(opened, Is.EqualTo(new[] { new RecordOpened(Run, Period.Containing(PeriodKind.Week, LastMonday)) }));
    }

    [Test]
    public void Buttons_RaiseTheirCallbacks()
    {
        var newWorkouts = 0;
        var navigated = new List<MainTab>();
        var settings = 0;
        var cut = Context.Render<TrendsPage>(p => p
            .Add(x => x.OnNew, () => newWorkouts++)
            .Add(x => x.OnNavigate, navigated.Add)
            .Add(x => x.OnSettings, () => settings++));

        cut.Find("[data-testid=new-link]").Click();
        cut.Find("[data-testid=this-week-link]").Click();
        cut.Find("[data-testid=trends-link]").Click();
        cut.Find("[data-testid=settings]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(newWorkouts, Is.EqualTo(1));
            Assert.That(navigated, Is.EqualTo(new[] { MainTab.ThisWeek }));
            Assert.That(settings, Is.EqualTo(1));
        });
    }

    [Test]
    public void AndroidBack_HasNoHandler()
    {
        Context.Render<TrendsPage>();

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public async Task AppStarted_ReloadsTheSameWeekWithTheSpinnerThenShowsTheNewValues()
    {
        MidWeek();
        var cut = await LoadedAsync(p => p.Add(x => x.DateQuery, "2026-11-09"));
        training.HoldMuscles = true;
        totals.Streaks = Streaks(6);

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1)));
        Assert.That(training.MuscleReads, Is.EqualTo(new[] { LastMonday, LastMonday }));
        training.CompleteMuscles();
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("6-week streak")));
        await started;
    }

    [Test]
    public async Task AppStarted_ShowingThisWeekAndANewWeekBegan_ShowsTheNewWeekAndRaisesIt()
    {
        MidWeek();
        var changed = new List<Period>();
        var cut = await LoadedAsync(p => p.Add(x => x.OnPeriodChanged, changed.Add));
        time.SetUtcNow(new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero));

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("23 – 29 Nov")));
        Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Week, new DateOnly(2026, 11, 23)) }));
    }

    [Test]
    public async Task AppStarted_ShowingAnEarlierWeekAndANewWeekBegan_StaysOnThatWeek()
    {
        MidWeek();
        var changed = new List<Period>();
        var cut = await LoadedAsync(p => p.Add(x => x.DateQuery, "2026-11-09").Add(x => x.OnPeriodChanged, changed.Add));
        time.SetUtcNow(new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero));

        await lifecycle.NotifyStartedAsync();

        Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("9 – 15 Nov"));
        Assert.That(changed, Is.Empty);
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        MidWeek();
        var cut = await LoadedAsync();
        Fail("series");

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-error]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task AppStarted_WhileLoading_IsIgnored()
    {
        training.HoldMuscles = true;
        Context.Render<TrendsPage>();

        var started = lifecycle.NotifyStartedAsync();
        await WaitForRendererAsync();

        Assert.That(training.MuscleReads, Has.Count.EqualTo(1));
        training.CompleteMuscles();
        await started.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        await LoadedAsync();

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(training.MuscleReads, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Disposed_WhileLoading_SkipsTheUpdate()
    {
        MidWeek();
        training.HoldMuscles = true;
        Context.Render<TrendsPage>();

        await Context.DisposeComponentsAsync();
        training.Records = new ReadFailed();
        training.CompleteMuscles();
        await WaitForRendererAsync();

        Assert.That(logger.Collector.Count, Is.Zero);
    }

    [Test]
    public async Task Refresh_FastReload_KeepsTheSpinnerUpForHalfASecond()
    {
        MidWeek();
        var cut = await LoadedAsync();
        totals.Streaks = Streaks(6);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(training.MuscleReads, Has.Count.EqualTo(2)));
        await WaitForRendererAsync();
        time.Advance(TimeSpan.FromMilliseconds(499));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
        });
        time.Advance(TimeSpan.FromMilliseconds(1));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-recent-weeks-aside]").TextContent, Is.EqualTo("6-week streak"));
        });
    }

    [Test]
    public async Task Refresh_Failed_ShowsTheErrorAfterHalfASecond()
    {
        MidWeek();
        var cut = await LoadedAsync();
        Fail("streaks");

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(training.MuscleReads, Has.Count.EqualTo(2)));
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        time.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.That(cut.FindAll("[data-testid=trends-error]"), Has.Count.EqualTo(1));
    }

    private static ReadResult<PeriodComparison> Comparison(Period week, WeekTally totals, WeekTally earlier) => new PeriodComparison(week, totals, earlier);

    private static ReadResult<WeekStreaks> Streaks(int current) => new WeekStreaks(current, new int[12], new LongestRun(current + 3, true));

    private async Task<IRenderedComponent<TrendsPage>> LoadedAsync(Action<ComponentParameterCollectionBuilder<TrendsPage>>? parameters = null)
    {
        var cut = Context.Render(parameters);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        return cut;
    }

    /// <summary>
    /// Wed 18 Nov 2026 with the week of Mon 16 trained on Monday (30 minutes) and Wednesday (one hour), 2 workouts, 20 sets and 4,200 kg,
    /// the week before trained on Monday (1h 05m), Wednesday (55m) and Friday (one hour), a 5-week streak, a Squat record on Wednesday and a
    /// run record on Monday, 12 sets of Legs, and the first workout in the week of Mon 26 Oct. The weeks before this one read as this week's
    /// numbers with the dates of the week asked for.
    /// </summary>
    private void MidWeek()
    {
        totals.ThisWeek = Comparison(Week, new WeekTally(2, new TimeSpan(1, 30, 0), 20), new WeekTally(2, new TimeSpan(2, 0, 0), 33));
        totals.LastWeek = Comparison(Week.Previous(), new WeekTally(4, new TimeSpan(4, 10, 0), 67), WeekTally.None);
        totals.Volume = new PeriodVolume(Week, 4200, 3900);
        totals.Limits = new PeriodLimits(Period.Containing(PeriodKind.Week, FirstMonday), Week);
        totals.Series = new PeriodSeries(
            Week,
            [
                new PeriodBucket(Monday, Monday, 1, TimeSpan.FromMinutes(30)),
                new PeriodBucket(Monday.AddDays(2), Monday.AddDays(2), 1, TimeSpan.FromHours(1)),
            ],
            [
                new PeriodBucket(LastMonday, LastMonday, 1, TimeSpan.FromMinutes(65)),
                new PeriodBucket(LastMonday.AddDays(2), LastMonday.AddDays(2), 1, TimeSpan.FromMinutes(55)),
                new PeriodBucket(LastMonday.AddDays(4), LastMonday.AddDays(4), 1, TimeSpan.FromHours(1)),
            ],
            [],
            0);
        totals.Streaks = Streaks(5);
        var monday = new DateTimeOffset(2026, 11, 16, 18, 0, 0, TimeSpan.Zero);
        training.Records = new PeriodRecords(
        [
            new ExerciseRecord(Squat, Now, new ExerciseBest(70m, 3, null, null)),
            new ExerciseRecord(Run, monday, new ExerciseBest(null, null, null, 5.2m)),
        ], []);
        training.Muscles = new ReadResult<IReadOnlyList<MuscleGroupCount>>(
            Enum.GetValues<MuscleGroup>().Select(g => new MuscleGroupCount(g, g == MuscleGroup.Legs ? 12 : 0)).ToList());
    }

    private void Fail(string read)
    {
        switch (read)
        {
            case "comparison":
                totals.ThisWeek = new ReadFailed();
                totals.LastWeek = new ReadFailed();
                break;
            case "volume":
                totals.Volume = new ReadFailed();
                break;
            case "series":
                totals.Series = new ReadFailed();
                break;
            case "streaks":
                totals.Streaks = new ReadFailed();
                break;
            case "limits":
                totals.Limits = new ReadFailed();
                break;
            default:
                training.Muscles = new ReadFailed();
                break;
        }
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
