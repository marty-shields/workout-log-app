using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Shields.UI.Components.Layout;
using Shields.UI.Lifecycle;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>The month and year views of <see cref="TrendsPage"/>, and switching between the kinds.</summary>
public class TrendsPageMonthYearTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Run = 311;

    /// <summary>Wed 18 Nov 2026, 10:00 UTC; 11:00 in UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly Period November = Period.Containing(PeriodKind.Month, new DateOnly(2026, 11, 1));
    private static readonly Period September = Period.Containing(PeriodKind.Month, new DateOnly(2026, 9, 1));
    private static readonly Period Year2026 = Period.Containing(PeriodKind.Year, new DateOnly(2026, 1, 1));
    private static readonly Period Year2025 = Period.Containing(PeriodKind.Year, new DateOnly(2025, 1, 1));
    private static readonly Period Year2024 = Period.Containing(PeriodKind.Year, new DateOnly(2024, 1, 1));
    private static readonly Period ThisWeek = Period.Containing(PeriodKind.Week, new DateOnly(2026, 11, 16));

    private FakeTimeProvider time = null!;
    private FakePeriodTotalsService totals = null!;
    private FakePeriodTrainingService training = null!;
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
        lifecycle = new AppLifecycle(new FakeLogger<AppLifecycle>());
        Context.Services.AddSingleton<IAppStart>(lifecycle);
        Fill();
    }

    [Test]
    public void Month_ReadsEverythingItNeedsTogetherAndNothingForTheOtherKinds()
    {
        totals.SeriesGate = new TaskCompletionSource();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        Assert.Multiple(() =>
        {
            Assert.That(totals.ComparisonPeriods, Is.EqualTo(new[] { November }));
            Assert.That(totals.VolumeReads, Is.EqualTo(new[] { November.Start }));
            Assert.That(totals.SeriesReads, Is.EqualTo(new[] { November.Start }));
            Assert.That(totals.LimitKinds, Is.EqualTo(new[] { PeriodKind.Month }));
            Assert.That(training.RecordPeriods, Is.EqualTo(new[] { November }));
            Assert.That(training.MostTrainedReads, Is.EqualTo(new[] { November.Start }));
            Assert.That(training.MuscleReads, Is.Empty);
            Assert.That(totals.StreakYears, Is.Empty);
        });
        totals.SeriesGate.SetResult();
        cut.WaitForElement("[data-testid=trends-tiles]");
    }

    [Test]
    public void Year_ReadsEverythingItNeedsTogetherAndNothingForTheOtherKinds()
    {
        totals.SeriesGate = new TaskCompletionSource();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-06-01"));

        Assert.Multiple(() =>
        {
            Assert.That(totals.ComparisonPeriods, Is.EqualTo(new[] { Year2025 }));
            Assert.That(totals.SeriesReads, Is.EqualTo(new[] { Year2025.Start }));
            Assert.That(totals.LimitKinds, Is.EqualTo(new[] { PeriodKind.Year }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2025 }));
            Assert.That(training.RecordPeriods, Is.EqualTo(new[] { Year2025 }));
            Assert.That(training.MuscleReads, Is.Empty);
            Assert.That(training.MostTrainedReads, Is.Empty);
        });
        totals.SeriesGate.SetResult();
        cut.WaitForElement("[data-testid=trends-tiles]");
    }

    [Test]
    public void Month_WhileLoading_ShowsTheSpinnerWithTheKindControlAndTheStepper()
    {
        training.MostTrainedGate = new TaskCompletionSource();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026"));
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("trends-link"));
        });
    }

    [Test]
    public void Year_WhileLoading_ShowsTheSpinnerWithTheKindControlAndTheStepper()
    {
        totals.SeriesGate = new TaskCompletionSource();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026"));
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Month_Current_ShowsTheTilesAgainstTheSameDayLastMonthAndEachCard()
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-month]").HasAttribute("checked"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against October to the 18th"));
            Assert.That(cut.FindAll("[data-testid=trends-tiles] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "11", "10h 55m", "172", "41,200 kg" }));
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .marked").Count, Is.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week .dates").Select(d => d.TextContent),
                Is.EqualTo(new[] { "1 Nov", "2 – 8 Nov", "9 – 15 Nov", "16 – 18 Nov" }));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .totals").Select(d => d.TextContent),
                Is.EqualTo(new[] { "1 workout · 30m", "4 workouts · 4h 10m", "4 workouts · 4h 10m", "2 workouts · 1h 30m" }));
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("2 this month"));
            Assert.That(cut.FindAll("[data-testid^=trends-records-row-]")
                    .Select(r => (r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent)),
                Is.EqualTo(new[] { ("Barbell Back Squat · 70kg × 3", "Wed 18 Nov"), ("Outdoor Run · 5.2km", "Mon 16 Nov") }));
            Assert.That(cut.FindAll("[data-testid=trends-most-trained-rows] .row").Select(r => (r.QuerySelector(".name")!.TextContent, r.QuerySelector(".count")!.FirstChild!.TextContent)),
                Is.EqualTo(new[] { ("Barbell Back Squat", "5"), ("Outdoor Run", "2") }));
            Assert.That(cut.FindAll("[data-testid=trends-muscles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void Month_Past_SaysThatMonthAndComparesWithTheMonthBefore()
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-10-15"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("October 2026"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against September"));
            Assert.That(cut.Find("[data-testid=trends-records-aside]").TextContent, Is.EqualTo("2 that month"));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week"), Has.Count.EqualTo(6));
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Month_January_ComparesWithTheDecemberBeforeWithItsYear()
    {
        time.SetUtcNow(new DateTimeOffset(2027, 2, 20, 10, 0, 0, TimeSpan.Zero));
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(September, Period.Containing(PeriodKind.Month, new DateOnly(2027, 2, 1)));

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2027-01-10"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against December 2026"));
    }

    [Test]
    public void Month_WithNoWorkouts_ShowsZerosAndWhatIsMissing()
    {
        totals.ThisWeek = Comparison(PeriodKind.Month, new WeekTally(0, TimeSpan.Zero, 0), new WeekTally(0, TimeSpan.Zero, 0));
        totals.LastWeek = totals.ThisWeek;
        totals.Volume = new PeriodVolume(November, 0, 0);
        totals.Series = new PeriodSeries(November, Days(), [], Weeks(0), 0);
        training.Records = new PeriodRecords([], []);
        training.MostTrained = new ReadResult<IReadOnlyList<TrainedExercise>>(Array.Empty<TrainedExercise>());

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-10-01"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-tiles] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "0", "0m", "0", "0 kg" }));
            Assert.That(cut.FindAll("[data-testid=trends-calendar-days] .marked"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records that month"));
            Assert.That(cut.Find("[data-testid=trends-most-trained-rows-none]").TextContent, Is.EqualTo("No exercises that month"));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void Month_NoWorkoutsEver_ShowsTheEmptyCurrentMonthWithBothArrowsDisabled()
    {
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(November, November);
        training.Records = new PeriodRecords([], []);

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [TestCase("comparison")]
    [TestCase("volume")]
    [TestCase("series")]
    [TestCase("limits")]
    [TestCase("most")]
    public void Month_ARequiredReadFails_ShowsTheErrorKeepsTheControlsAndLogsIt(string failing)
    {
        Fail(failing);

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-error]").TextContent, Is.EqualTo("Couldn't load Trends")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-stepper]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsLoadFailed, LogLevel.Warning, "Loading Trends failed") }));
        });
    }

    [Test]
    public void Month_RecordsFail_LeaveTheirCardEmptyAndTheRestShowing()
    {
        training.Records = new ReadFailed();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-records-none]").TextContent, Is.EqualTo("No new records this month"));
            Assert.That(cut.FindAll("[data-testid=trends-most-trained-rows] .row"), Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week"), Is.Not.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsRecordsFailed, LogLevel.Warning, "Loading the records for Trends failed; the card is empty") }));
        });
    }

    [Test]
    public void Month_Limits_FirstMonthDisablesPreviousAndTheCurrentOneNext()
    {
        var first = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-09-12"));
        first.WaitForAssertion(() => Assert.That(first.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var current = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));
        current.WaitForAssertion(() => Assert.That(current.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        Assert.Multiple(() =>
        {
            Assert.That(first.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("September 2026"));
            Assert.That(first.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(first.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
            Assert.That(current.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False);
            Assert.That(current.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void Month_OpenedBeforeTheFirstWorkoutOrAfterToday_ClampsLikeTheWeek()
    {
        var changed = new List<Period>();

        var early = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2001-01-01").Add(x => x.OnPeriodChanged, changed.Add));
        early.WaitForAssertion(() => Assert.That(early.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("September 2026")));
        early.WaitForAssertion(() => Assert.That(early.FindAll("[data-testid=trends-loading]"), Is.Empty));
        var late = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2030-05-05"));

        late.WaitForAssertion(() => Assert.That(late.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        Assert.That(changed, Is.EqualTo(new[] { September }));
    }

    [Test]
    public void Year_OpenedBeforeTheFirstWorkoutOrAfterToday_ClampsLikeTheWeek()
    {
        var changed = new List<Period>();

        var early = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2001-01-01").Add(x => x.OnPeriodChanged, changed.Add));
        early.WaitForAssertion(() => Assert.That(early.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2025")));
        early.WaitForAssertion(() => Assert.That(early.FindAll("[data-testid=trends-loading]"), Is.Empty));
        var late = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2030-05-05"));

        late.WaitForAssertion(() => Assert.That(late.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026")));
        Assert.That(changed, Is.EqualTo(new[] { Year2025 }));
    }

    [Test]
    public void Month_StepPrevious_ShowsTheMonthBeforeAndRaisesIt()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));

        cut.Find("[data-testid=trends-stepper-previous]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("October 2026")));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 1)) }));
            Assert.That(totals.LimitKinds, Is.EqualTo(new[] { PeriodKind.Month }));
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Month_StepNextAndPreviousAtTheLimits_DoNothing()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.InvokeAsync(() => cut.FindComponent<TrendsView>().Instance.OnNext.InvokeAsync());

        Assert.That(changed, Is.Empty);
        Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(1));
    }

    [Test]
    public void Month_StepPreviousAtTheFirstMonth_DoesNothing()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-09-12").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.InvokeAsync(() => cut.FindComponent<TrendsView>().Instance.OnPrevious.InvokeAsync());

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.Empty);
            Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("September 2026"));
        });
    }

    [Test]
    public void Year_StepNextAndPreviousAtTheLimits_DoNothing()
    {
        var changed = new List<Period>();
        var first = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-12-12").Add(x => x.OnPeriodChanged, changed.Add));
        first.WaitForAssertion(() => Assert.That(first.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var current = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.OnPeriodChanged, changed.Add));
        current.WaitForAssertion(() => Assert.That(current.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        first.InvokeAsync(() => first.FindComponent<TrendsView>().Instance.OnPrevious.InvokeAsync());
        current.InvokeAsync(() => current.FindComponent<TrendsView>().Instance.OnNext.InvokeAsync());

        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.Empty);
            Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(2));
            Assert.That(first.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2025"));
            Assert.That(current.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026"));
        });
    }

    [Test]
    public void Month_CurrentOnAMonday_ListsTheLastWeekAsThatDay()
    {
        StartAt(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=trends-weeks-rows] .week .dates").Select(d => d.TextContent),
            Is.EqualTo(new[] { "1 Nov", "2 – 8 Nov", "9 – 15 Nov", "16 Nov" }));
    }

    [Test]
    public void Month_JanuaryCurrent_ComparesWithTheDecemberBeforeWithItsYear()
    {
        time.SetUtcNow(new DateTimeOffset(2027, 1, 18, 10, 0, 0, TimeSpan.Zero));
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(September, Period.Containing(PeriodKind.Month, new DateOnly(2027, 1, 1)));

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against December 2026 to the 18th"));
    }

    [Test]
    public void Month_TrainedExerciseNotInTheCatalog_IsLeftOut()
    {
        training.MostTrained = new ReadResult<IReadOnlyList<TrainedExercise>>(new TrainedExercise[] { new(Squat, 5), new(999999, 4), new(Run, 2) });

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-most-trained-rows] .row .name").Select(n => n.TextContent), Is.EqualTo(new[] { "Barbell Back Squat", "Outdoor Run" }));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void Month_Tapped_RecordAndMostTrainedRowOpenTheExerciseWithTheMonthShown()
    {
        var opened = new List<RecordOpened>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-10-10").Add(x => x.OnOpenExercise, opened.Add));

        cut.WaitForElement("button[data-testid=trends-records-row-311]").Click();
        cut.Find("[data-testid=trends-most-trained-rows-barbell-back-squat] button").Click();

        var october = Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 1));
        Assert.That(opened, Is.EqualTo(new[] { new RecordOpened(Run, october), new RecordOpened(Squat, october) }));
    }

    [Test]
    public void Month_CalendarDayWithAWorkoutAndWeekRowWithWorkouts_RaiseOnOpenWorkoutsWithTheirDatesAndTheMonthShown()
    {
        var opened = new List<WorkoutsOpened>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.OnOpenWorkouts, opened.Add));

        cut.WaitForElement("[data-testid=trends-calendar-days-day-2026-11-04]").Click();
        cut.Find("[data-testid=trends-weeks-rows-2-open]").Click();
        cut.Find("[data-testid=trends-weeks-rows-4-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(opened, Is.EqualTo(new[]
            {
                new WorkoutsOpened(new DateSpan(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 4)), November),
                new WorkoutsOpened(new DateSpan(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8)), November),
                new WorkoutsOpened(new DateSpan(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 18)), November),
            }));
            Assert.That(cut.FindAll("button[data-testid=trends-calendar-days-day-2026-11-03]"), Is.Empty);
        });
    }

    [Test]
    public void Year_MonthColumn_RaisesOnOpenWorkoutsWithTheMonthsDaysCutToTodayAndTheYearShown()
    {
        FillYear();
        var opened = new List<WorkoutsOpened>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.OnOpenWorkouts, opened.Add));

        cut.WaitForElement("[data-testid=trends-months-chart-1-open]").Click();
        cut.Find("[data-testid=trends-months-chart-11-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(opened, Is.EqualTo(new[]
            {
                new WorkoutsOpened(new DateSpan(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), Year2026),
                new WorkoutsOpened(new DateSpan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 18)), Year2026),
            }));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart-12-open]"), Is.Empty);
        });
    }

    [Test]
    public void Year_MonthColumns_AreOnlyTappableForMonthsThatHaveBegunAndHaveWorkouts()
    {
        FillYear();
        totals.Series = new PeriodSeries(Year2026, [.. Enumerable.Range(1, 12).Select(m => Bucket(new DateOnly(2026, m, 1), m is 2 or 12 ? 0 : 3))], [], [], 3.2);
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForElement("[data-testid=trends-months-chart-1-open]");
        Assert.That(cut.FindAll("[data-testid^=trends-months-chart-][data-testid$=-open]").Select(b => b.GetAttribute("data-testid")),
            Is.EqualTo(new[] { 1, 3, 4, 5, 6, 7, 8, 9, 10, 11 }.Select(m => $"trends-months-chart-{m}-open")));
    }

    [Test]
    public void Year_FutureMonthWithWorkouts_HasNoSpan()
    {
        FillYear();
        totals.Series = new PeriodSeries(Year2026, [.. Enumerable.Range(1, 12).Select(m => Bucket(new DateOnly(2026, m, 1), 3))], [], [], 3.2);
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForElement("[data-testid=trends-months-chart-11-open]");
        Assert.That(cut.FindAll("[data-testid=trends-months-chart-12-open]"), Is.Empty);
    }

    [Test]
    public void Year_Current_ShowsTheTilesAgainstTheSameDateLastYearAndEachCard()
    {
        FillYear();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-year]").HasAttribute("checked"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against 2025 to 18 Nov"));
            Assert.That(cut.FindAll("[data-testid=trends-tiles] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "146", "148h 00m", "2310", "548 t" }));
            Assert.That(cut.Find("[data-testid=trends-months-aside]").TextContent, Is.EqualTo("3.2 a week"));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart-values] li").Select(li => li.TextContent).Take(2),
                Is.EqualTo(new[] { "January: 14 workouts", "February: 12 workouts" }));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart-values] li").Last().TextContent, Is.EqualTo("December: no workouts"));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .bar.now"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .axis .current").Select(a => a.TextContent), Is.EqualTo(new[] { "N" }));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .values .value").Select(v => v.TextContent), Is.EqualTo(new[] { "17", "11" }));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("23 weeks"));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .detail").TextContent, Is.EqualTo("Still going"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-aside]").TextContent, Is.EqualTo("37 this year"));
            Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows] .row .name").Select(n => n.TextContent), Is.EqualTo(MonthNames(11)));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void Year_OnTheFirstOfTheMonth_ShowsThatMonthAsCurrentAndInTheRecords()
    {
        StartAt(new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));
        FillYear();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var rows = cut.FindAll("[data-testid=trends-records-by-month-rows] .row .name").Select(n => n.TextContent).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .axis .current").Select(a => a.TextContent), Is.EqualTo(new[] { "N" }));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart] .bar.now"), Has.Count.EqualTo(1));
            Assert.That(rows, Has.Count.EqualTo(11));
            Assert.That(rows.Last(), Is.EqualTo("Nov"));
        });
    }

    [Test]
    public void Switching_WhileTheNewKindLoads_KeepsPreviousDisabledWhenItsFirstMonthIsTheOneShown()
    {
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(November, November);
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        totals.SeriesGate = new TaskCompletionSource();

        cut.Find("[data-testid=trends-month]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
        totals.SeriesGate.SetResult();
    }

    [Test]
    public void Switching_ToAKindWhoseLimitsFail_KeepsPreviousDisabled()
    {
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        totals.LimitsOfKind[PeriodKind.Month] = new ReadFailed();

        cut.Find("[data-testid=trends-month]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-error]"), Has.Count.EqualTo(1)));
        Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Year_Past_SaysThatYearShowsAllTwelveMonthsAndAStreakThatIsNotRunning()
    {
        FillYear();
        totals.Streaks = new WeekStreaks(5, new int[12], new LongestRun(14, false));

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-03-03"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2025"));
            Assert.That(cut.Find("[data-testid=trends-tiles-against]").TextContent, Is.EqualTo("Against 2024"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-aside]").TextContent, Is.EqualTo("37 that year"));
            Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows] .row"), Has.Count.EqualTo(12));
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("14 weeks"));
            Assert.That(cut.FindAll("[data-testid=trends-streak-row] .detail"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void Year_WithNoWorkouts_ShowsZerosAndWhatIsMissing()
    {
        totals.ThisWeek = Comparison(PeriodKind.Year, new WeekTally(0, TimeSpan.Zero, 0), new WeekTally(0, TimeSpan.Zero, 0));
        totals.LastWeek = totals.ThisWeek;
        totals.Volume = new PeriodVolume(Year2024, 0, 0);
        totals.Series = new PeriodSeries(Year2024, Enumerable.Range(1, 12).Select(m => Bucket(new DateOnly(2024, m, 1), 0)).ToList(), [], [], 0);
        totals.Streaks = new WeekStreaks(0, new int[12], new LongestRun(0, false));
        training.Records = new PeriodRecords([], new int[12]);

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2024-05-05"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-months-chart-none]").TextContent, Is.EqualTo("No workouts that year"));
            Assert.That(cut.FindAll("[data-testid=trends-months-aside]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=trends-streak-row] .aside").TextContent, Is.EqualTo("None"));
            Assert.That(cut.Find("[data-testid=trends-records-by-month-rows-none]").TextContent, Is.EqualTo("No records that year"));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
        });
    }

    [Test]
    public void Year_NoWorkoutsEver_ShowsTheEmptyCurrentYearWithBothArrowsDisabled()
    {
        totals.LimitsOfKind[PeriodKind.Year] = new PeriodLimits(Year2026, Year2026);

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026"));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
        });
    }

    [TestCase("comparison")]
    [TestCase("volume")]
    [TestCase("series")]
    [TestCase("limits")]
    [TestCase("streaks")]
    public void Year_ARequiredReadFails_ShowsTheErrorKeepsTheControlsAndLogsIt(string failing)
    {
        Fail(failing);

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-error]").TextContent, Is.EqualTo("Couldn't load Trends")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-kind]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-stepper]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsLoadFailed, LogLevel.Warning, "Loading Trends failed") }));
        });
    }

    [Test]
    public void Year_RecordsFail_LeaveTheirCardEmptyAndTheRestShowing()
    {
        FillYear();
        training.Records = new ReadFailed();

        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-records-by-month-rows-none]").TextContent, Is.EqualTo("No records this year"));
            Assert.That(cut.FindAll("[data-testid=trends-months-chart-values] li"), Has.Count.EqualTo(12));
            Assert.That(cut.FindAll("[data-testid=trends-error]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.TrendsRecordsFailed, LogLevel.Warning, "Loading the records for Trends failed; the card is empty") }));
        });
    }

    [Test]
    public void Year_Limits_FirstYearDisablesPreviousAndTheCurrentOneNext()
    {
        totals.LimitsOfKind[PeriodKind.Year] = new PeriodLimits(Year2025, Year2026);
        var first = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-12-12"));
        first.WaitForAssertion(() => Assert.That(first.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        var current = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));
        current.WaitForAssertion(() => Assert.That(current.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        Assert.Multiple(() =>
        {
            Assert.That(first.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
            Assert.That(first.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.False);
            Assert.That(current.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False);
            Assert.That(current.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void Year_Step_ShowsTheYearBeforeReadsItsStreakAndRaisesIt()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.False));

        cut.Find("[data-testid=trends-stepper-previous]").Click();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2025")));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Year2025 }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026, 2025 }));
            Assert.That(totals.LimitKinds, Is.EqualTo(new[] { PeriodKind.Year }));
        });
    }

    [Test]
    public void Year_RecordsByMonthAreNotTappable()
    {
        FillYear();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.OnOpenExercise, _ => { }));

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=trends-records-by-month-rows] button"), Is.Empty);
    }

    [Test]
    public void Switching_FromTheCurrentWeek_GoesUpToTodaysMonthThenYear()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-weeks]"), Has.Count.EqualTo(1)));
        cut.Find("[data-testid=trends-year]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026")));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-months]"), Has.Count.EqualTo(1)));

        Assert.That(changed, Is.EqualTo(new[] { November, Year2026 }));
    }

    [Test]
    public void Switching_FromAPastWeek_KeepsThePeriodHoldingItsFirstDay()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-11-09").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        cut.Find("[data-testid=trends-year]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026")));
        Assert.That(changed, Is.EqualTo(new[] { November, Year2026 }));
    }

    [Test]
    public void Switching_DownFromAPastYear_KeepsThePeriodHoldingItsFirstDay()
    {
        totals.Limits = new PeriodLimits(Period.Containing(PeriodKind.Week, new DateOnly(2024, 12, 30)), ThisWeek);
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(Period.Containing(PeriodKind.Month, new DateOnly(2025, 1, 1)), November);
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-03-05"));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("January 2025")));
        cut.Find("[data-testid=trends-week]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("30 Dec – 5 Jan")));
    }

    [Test]
    public void Switching_FromTheCurrentYearOrMonthDown_GoesToTodaysPeriod()
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year"));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        cut.Find("[data-testid=trends-week]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("16 – 22 Nov")));
        Assert.That(cut.Find("[data-testid=trends-stepper-next]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Switching_ToAKindWhoseFirstPeriodIsLater_ClampsToItAndRaisesIt()
    {
        var changed = new List<Period>();
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(November, November);
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.DateQuery, "2026-10-28").Add(x => x.OnPeriodChanged, changed.Add));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty));
        Assert.Multiple(() =>
        {
            Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Month, new DateOnly(2026, 10, 1)), November }));
            Assert.That(cut.Find("[data-testid=trends-stepper-previous]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void Switching_ReadsLimitsOfTheNewKindAndKeepsThemWhileStepping()
    {
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=trends-month]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-weeks]"), Has.Count.EqualTo(1)));
        cut.Find("[data-testid=trends-stepper-previous]").Click();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("October 2026")));

        Assert.That(totals.LimitKinds, Is.EqualTo(new[] { PeriodKind.Week, PeriodKind.Month }));
    }

    [Test]
    public void Switching_ToTheKindAlreadyShown_DoesNothing()
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month"));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.InvokeAsync(() => cut.FindComponent<TrendsView>().Instance.OnKindChanged.InvokeAsync(PeriodKind.Month));

        Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(1));
    }

    [Test]
    public void TheLinkChangesKind_LoadsThatKind()
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "week"));
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.KindQuery, "year"));

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2026")));
        Assert.That(totals.ComparisonPeriods.Select(c => c.Kind), Is.EqualTo(new[] { PeriodKind.Week, PeriodKind.Year }));
    }

    [Test]
    public void TheLinkCatchesUpWithASwitch_DoesNotLoadAgain()
    {
        var cut = Context.Render<TrendsPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        cut.Find("[data-testid=trends-month]").Change(true);
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("November 2026")));

        cut.Render(p => p.Add(x => x.KindQuery, "month").Add(x => x.DateQuery, "2026-11-01"));

        Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(2));
    }

    [TestCase("month", "November 2026")]
    [TestCase("year", "2026")]
    public async Task AppStarted_ReloadsTheSamePeriodWithTheSpinner(string kind, string label)
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, kind));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        totals.ThisWeek = Comparison(kind == "month" ? PeriodKind.Month : PeriodKind.Year, new WeekTally(12, new TimeSpan(11, 0, 0), 180), new WeekTally(10, new TimeSpan(10, 15, 0), 163));

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-tiles-workouts] .value").TextContent, Is.EqualTo("12")));
        Assert.Multiple(() =>
        {
            Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(2));
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo(label));
        });
    }

    [Test]
    public async Task AppStarted_ShowingTheCurrentMonthAndANewMonthBegan_ShowsTheNewMonthAndRaisesIt()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "month").Add(x => x.OnPeriodChanged, changed.Add));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        time.SetUtcNow(new DateTimeOffset(2026, 11, 30, 23, 30, 0, TimeSpan.Zero));
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(September, Period.Containing(PeriodKind.Month, new DateOnly(2026, 12, 1)));

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("December 2026")));
        Assert.That(changed, Is.EqualTo(new[] { Period.Containing(PeriodKind.Month, new DateOnly(2026, 12, 1)) }));
    }

    [Test]
    public async Task AppStarted_ShowingAPastYear_StaysOnIt()
    {
        var changed = new List<Period>();
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, "year").Add(x => x.DateQuery, "2025-05-05").Add(x => x.OnPeriodChanged, changed.Add));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));
        time.SetUtcNow(new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero));

        await lifecycle.NotifyStartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=trends-stepper-label]").TextContent, Is.EqualTo("2025"));
            Assert.That(changed, Is.Empty);
        });
    }

    [TestCase("month")]
    [TestCase("year")]
    public async Task Refresh_ReloadsTheShownPeriodAndKeepsTheSpinnerForHalfASecond(string kind)
    {
        var cut = Context.Render<TrendsPage>(p => p.Add(x => x.KindQuery, kind));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1)));

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(totals.ComparisonPeriods, Has.Count.EqualTo(2)));
        await WaitForRendererAsync();
        time.Advance(TimeSpan.FromMilliseconds(499));
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=trends-loading]"), Has.Count.EqualTo(1));
        time.Advance(TimeSpan.FromMilliseconds(1));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=trends-loading]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=trends-tiles]"), Has.Count.EqualTo(1));
        });
    }

    /// <summary>Starts again with the clock at <paramref name="now"/>, which may be before <see cref="Now"/>, and everything filled as in <see cref="Fill"/>.</summary>
    private void StartAt(DateTimeOffset now)
    {
        time = new FakeTimeProvider(now);
        time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(time);
        totals = new FakePeriodTotalsService(time);
        Context.Services.AddSingleton<IPeriodTotalsService>(totals);
        Fill();
    }

    private static ReadResult<PeriodComparison> Comparison(PeriodKind kind, WeekTally totals, WeekTally earlier) =>
        new PeriodComparison(Period.Containing(kind, new DateOnly(2026, 11, 18)), totals, earlier);

    private static PeriodBucket Bucket(DateOnly start, int workouts) =>
        new(start, start.AddMonths(1).AddDays(-1), workouts, TimeSpan.FromMinutes(workouts * 30));

    private static string[] MonthNames(int count) =>
        [.. Enumerable.Range(1, count).Select(m => new DateOnly(2026, m, 1).ToString("MMM", System.Globalization.CultureInfo.InvariantCulture))];

    /// <summary>November's 30 days with workouts on the 2nd, 4th and 6th.</summary>
    private static List<PeriodBucket> Days() =>
        [.. Enumerable.Range(1, 30).Select(d => new PeriodBucket(new DateOnly(2026, 11, d), new DateOnly(2026, 11, d), d is 2 or 4 or 6 ? 1 : 0, d is 2 or 4 or 6 ? TimeSpan.FromMinutes(30) : TimeSpan.Zero))];

    /// <summary>November's weeks cut to the month: 1; 2 – 8; 9 – 15; 16 – 22; 23 – 29; 30. With <paramref name="each"/> 0, no workouts in any.</summary>
    private static List<PeriodBucket> Weeks(int each) =>
    [
        new(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1), each == 0 ? 0 : 1, each == 0 ? TimeSpan.Zero : TimeSpan.FromMinutes(30)),
        new(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), each * 4, each == 0 ? TimeSpan.Zero : new TimeSpan(4, 10, 0)),
        new(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), each * 4, each == 0 ? TimeSpan.Zero : new TimeSpan(4, 10, 0)),
        new(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), each * 2, each == 0 ? TimeSpan.Zero : new TimeSpan(1, 30, 0)),
        new(new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 29), 0, TimeSpan.Zero),
        new(new DateOnly(2026, 11, 30), new DateOnly(2026, 11, 30), 0, TimeSpan.Zero),
    ];

    /// <summary>
    /// The numbers every kind reads (the fake returns one answer whatever the period): 11 workouts, 10h 55m, 172 sets, 41,200 kg for a period holding today;
    /// a Squat record on Wed 18 Nov and a run record on Mon 16 Nov; the 37 records of a year; Squat done in 5 sessions and the run in 2; limits from
    /// September 2026, 2025 and the week of 26 Oct; a streak of 23 weeks still going.
    /// </summary>
    private void Fill()
    {
        totals.ThisWeek = Comparison(PeriodKind.Month, new WeekTally(11, new TimeSpan(10, 55, 0), 172), new WeekTally(10, new TimeSpan(10, 15, 0), 163));
        totals.LastWeek = totals.ThisWeek;
        totals.Volume = new PeriodVolume(November, 41200, 39615);
        totals.Limits = new PeriodLimits(Period.Containing(PeriodKind.Week, new DateOnly(2026, 10, 26)), ThisWeek);
        totals.LimitsOfKind[PeriodKind.Month] = new PeriodLimits(September, November);
        totals.LimitsOfKind[PeriodKind.Year] = new PeriodLimits(Year2025, Year2026);
        totals.Series = new PeriodSeries(November, Days(), [], Weeks(1), 0);
        totals.Streaks = new WeekStreaks(5, new int[12], new LongestRun(23, true));
        training.Records = new PeriodRecords(
        [
            new ExerciseRecord(Squat, Now, new ExerciseBest(70m, 3, null, null)),
            new ExerciseRecord(Run, new DateTimeOffset(2026, 11, 16, 18, 0, 0, TimeSpan.Zero), new ExerciseBest(null, null, null, 5.2m)),
        ], [5, 3, 4, 2, 6, 2, 3, 1, 3, 4, 4, 0]);
        training.MostTrained = new ReadResult<IReadOnlyList<TrainedExercise>>(new TrainedExercise[] { new(Squat, 5), new(Run, 2) });
    }

    /// <summary>A year's numbers in place of a month's: 146 workouts, 148h, 2,310 sets, 548 t; 3.2 workouts a week; and the workouts in each month.</summary>
    private void FillYear()
    {
        int[] workouts = [14, 12, 16, 13, 17, 11, 15, 8, 14, 15, 11, 0];
        totals.ThisWeek = Comparison(PeriodKind.Year, new WeekTally(146, new TimeSpan(148, 0, 0), 2310), new WeekTally(131, new TimeSpan(136, 0, 0), 2120));
        totals.LastWeek = totals.ThisWeek;
        totals.Volume = new PeriodVolume(Year2026, 548000, 503000);
        totals.Series = new PeriodSeries(Year2026, [.. workouts.Select((n, i) => Bucket(new DateOnly(2026, i + 1, 1), n))], [], [], 3.2);
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
            case "limits":
                totals.Limits = new ReadFailed();
                totals.LimitsOfKind.Clear();
                break;
            case "streaks":
                totals.Streaks = new ReadFailed();
                break;
            default:
                training.MostTrained = new ReadFailed();
                break;
        }
    }
}
