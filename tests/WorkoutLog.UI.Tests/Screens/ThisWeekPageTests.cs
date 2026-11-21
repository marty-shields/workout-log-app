using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Shields.UI.Components.Layout;
using Shields.UI.Lifecycle;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ThisWeekPageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Run = 311;

    /// <summary>Wed 18 Nov 2026, 10:00 UTC; 11:00 in UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Monday = new(2026, 11, 16);

    private static readonly Period Week = Period.Containing(PeriodKind.Week, Monday);

    private FakeTimeProvider time = null!;
    private FakePeriodTotalsService totals = null!;
    private FakePeriodTrainingService training = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private FakeLogger<ThisWeekPage> logger = null!;
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
        logger = new FakeLogger<ThisWeekPage>();
        Context.Services.AddSingleton<ILogger<ThisWeekPage>>(logger);
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerWithTheBottomNav()
    {
        training.Hold = true;

        var cut = Context.Render<ThisWeekPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=week-tiles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("this-week-link"));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Is.Empty);
        });
    }

    [Test]
    public void Loading_ReadsEverythingTogetherForTheWeekAndTheWeekBefore()
    {
        training.Hold = true;

        Context.Render<ThisWeekPage>();

        Assert.Multiple(() =>
        {
            Assert.That(totals.Comparisons, Is.EqualTo(new[] { Monday, Monday.AddDays(-7) }));
            Assert.That(totals.SeriesReads, Is.EqualTo(new[] { Monday }));
            Assert.That(totals.StreakYears, Is.EqualTo(new[] { 2026 }));
            Assert.That(training.WorkoutsReads, Is.EqualTo(new[] { Monday }));
            Assert.That(training.MuscleReads, Is.EqualTo(new[] { Monday }));
            Assert.That(training.RecordReads, Is.EqualTo(new[] { Monday }));
        });
    }

    [Test]
    public void Today_IsTheDayInThePhonesZone()
    {
        time.SetUtcNow(new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero));

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=this-week-dates]").TextContent, Is.EqualTo("23 – 29 Nov")));
        Assert.That(totals.Comparisons, Is.EqualTo(new[] { new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 16) }));
    }

    [Test]
    public void Loaded_MidWeek_ShowsTheWeekFromTheServices()
    {
        MidWeek();

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=week-tiles]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=this-week-dates]").TextContent, Is.EqualTo("16 – 22 Nov"));
            Assert.That(cut.Find("[data-testid=this-week-streak]").TextContent, Is.EqualTo("5-week streak"));
            Assert.That(cut.FindAll("li[data-testid^=day-strip-]").Select(d => (d.GetAttribute("data-testid"), d.ClassList.Contains("done"))), Is.EqualTo(new[]
            {
                ("day-strip-monday", true), ("day-strip-tuesday", false), ("day-strip-wednesday", true),
                ("day-strip-thursday", false), ("day-strip-friday", false), ("day-strip-saturday", false), ("day-strip-sunday", false),
            }));
            Assert.That(cut.Find("[data-testid=week-tiles-workouts] .value").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=week-tiles-time] .value").TextContent, Is.EqualTo("1h 30m"));
            Assert.That(cut.Find("[data-testid=week-tiles-sets] .value").TextContent, Is.EqualTo("20"));
            Assert.That(cut.Find("[data-testid=week-tiles-against]").TextContent, Is.EqualTo("Against Monday to Wednesday last week"));
            Assert.That(cut.FindAll("[data-testid=last-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Is.Empty);
        });
    }

    [Test]
    public void Loaded_MidWeek_ShowsTheWorkoutsNewestFirstOnThePhonesClock()
    {
        MidWeek();

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid^=week-workouts-row-]"), Has.Count.EqualTo(2)));
        Assert.That(cut.FindAll("[data-testid^=week-workouts-row-]")
                .Select(r => (r.GetAttribute("data-testid"), r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent, r.QuerySelector(".aside")!.TextContent)),
            Is.EqualTo(new[]
            {
                ("week-workouts-row-32", "Wed 18 Nov · 11:00", "Barbell Back Squat, Outdoor Run", "1:00"),
                ("week-workouts-row-31", "Mon 16 Nov · 19:00", "Barbell Back Squat", "0:30"),
            }));
    }

    [Test]
    public void Loaded_MidWeek_ShowsTheRecordsAndTheMuscleGroups()
    {
        MidWeek();

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid^=week-records-row-]"), Has.Count.EqualTo(2)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=week-records-row-]")
                    .Select(r => (r.GetAttribute("data-testid"), r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent)),
                Is.EqualTo(new[]
                {
                    ("week-records-row-6", "Barbell Back Squat · 70kg × 3", "Wednesday"),
                    ("week-records-row-311", "Outdoor Run · 5.2km", "Monday"),
                }));
            Assert.That(cut.Find("[data-testid=week-muscles-legs] .sets").TextContent, Is.EqualTo("12 sets"));
            Assert.That(cut.Find("[data-testid=week-muscles-chest] .sets").TextContent, Is.EqualTo("Not trained"));
        });
    }

    [Test]
    public void Loaded_RecordNearMidnightUtc_ShowsTheWeekdayInThePhonesZone()
    {
        MidWeek();
        training.Records = new PeriodRecords(
            [new ExerciseRecord(Squat, new DateTimeOffset(2026, 11, 17, 23, 30, 0, TimeSpan.Zero), new ExerciseBest(70m, 3, null, null))], []);

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=week-records-row-6] .detail").TextContent, Is.EqualTo("Wednesday")));
    }

    [Test]
    public void Loaded_MoreThanFiveRecords_ShowsFiveThenHowManyMore()
    {
        MidWeek();
        training.Records = new PeriodRecords(
            [.. new[] { 6, 8, 9, 22, 24, 41, 115 }.Select(id => new ExerciseRecord(id, Now, new ExerciseBest(60m, 5, null, null)))], []);

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid^=week-records-row-]"), Has.Count.EqualTo(5)));
        Assert.That(cut.Find("[data-testid=week-records-more]").TextContent, Is.EqualTo("2 more"));
    }

    [Test]
    public void EmptyWeek_ShowsTheEmptyCardAndTheLastWeekRow()
    {
        totals.LastWeek = Comparison(new WeekTally(4, new TimeSpan(4, 10, 0), 67), WeekTally.None);

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=last-week]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=week-workouts-none]").TextContent, Is.EqualTo("Nothing yet this week. Tap + to start one."));
            Assert.That(cut.Find("[data-testid=last-week-row] .detail").TextContent, Is.EqualTo("4 workouts · 4h 10m · 67 sets"));
            Assert.That(cut.FindAll("button[data-testid=last-week-row]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=week-records]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=week-muscles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=this-week-streak]"), Is.Empty);
        });
    }

    [Test]
    public void NoWorkoutsEver_ShowsTheEmptyCardAndNoLastWeekRow()
    {
        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=week-workouts-none]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=last-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=week-records]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=this-week-error]"), Is.Empty);
        });
    }

    [TestCase("comparison")]
    [TestCase("lastWeek")]
    [TestCase("series")]
    [TestCase("streaks")]
    [TestCase("workouts")]
    [TestCase("muscles")]
    public void ARequiredReadFails_ShowsTheErrorAndLogsIt(string failing)
    {
        MidWeek();
        Fail(failing);

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=this-week-error]").TextContent, Is.EqualTo("Couldn't load This week")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=week-tiles]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=add-button]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=trends-link]"), Has.Count.EqualTo(1));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ThisWeekLoadFailed, LogLevel.Warning, "Loading This week failed") }));
        });
    }

    [Test]
    public void LoadFails_FocusesTryAgain()
    {
        MidWeek();
        Fail("workouts");

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=this-week-retry]")));
    }

    [Test]
    public async Task TryAgain_ShowsTheSpinnerWhileReloadingThenTheWeek()
    {
        MidWeek();
        Fail("workouts");
        var cut = Context.Render<ThisWeekPage>();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=this-week-retry]"), Has.Count.EqualTo(1)));
        training.Workouts = new ReadResult<IReadOnlyList<WorkoutOutline>>(Array.Empty<WorkoutOutline>());
        training.Hold = true;

        cut.Find("[data-testid=this-week-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=this-week-error]"), Is.Empty);
        });

        training.Complete();
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=week-tiles]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=this-week-error]"), Is.Empty);
    }

    [Test]
    public void RecordsFail_LeavesTheirCardOutShowsTheRestAndLogsIt()
    {
        MidWeek();
        training.Records = new ReadFailed();

        var cut = Context.Render<ThisWeekPage>();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=week-workouts]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=week-records]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=week-muscles]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=week-tiles]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=this-week-error]"), Is.Empty);
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ThisWeekRecordsFailed, LogLevel.Warning, "Loading the records for This week failed; the card is left out") }));
        });
    }

    [Test]
    public void Workout_Tapped_RaisesOnOpenWorkoutWithItsId()
    {
        MidWeek();
        var opened = new List<int>();
        var cut = Context.Render<ThisWeekPage>(p => p.Add(x => x.OnOpenWorkout, id => opened.Add(id)));

        cut.WaitForElement("button[data-testid=week-workouts-row-31]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 31 }));
    }

    [Test]
    public void Record_Tapped_RaisesOnOpenExerciseWithItsId()
    {
        MidWeek();
        var opened = new List<int>();
        var cut = Context.Render<ThisWeekPage>(p => p.Add(x => x.OnOpenExercise, id => opened.Add(id)));

        cut.WaitForElement("button[data-testid=week-records-row-311]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 311 }));
    }

    [Test]
    public void Buttons_RaiseTheirCallbacks()
    {
        var added = 0;
        var navigated = new List<MainTab>();
        var settings = 0;
        var cut = Context.Render<ThisWeekPage>(p => p
            .Add(x => x.OnNew, () => added++)
            .Add(x => x.OnNavigate, navigated.Add)
            .Add(x => x.OnSettings, () => settings++));

        cut.Find("[data-testid=add-button]").Click();
        cut.Find("[data-testid=trends-link]").Click();
        cut.Find("[data-testid=settings]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.EqualTo(1));
            Assert.That(navigated, Is.EqualTo(new[] { MainTab.Trends }));
            Assert.That(settings, Is.EqualTo(1));
        });
    }

    [Test]
    public void LastWeekRow_RaisesOnOpenLastWeekWithLastWeek()
    {
        totals.LastWeek = Comparison(new WeekTally(4, new TimeSpan(4, 10, 0), 67), WeekTally.None);
        var opened = new List<Period>();
        var cut = Context.Render<ThisWeekPage>(p => p.Add(x => x.OnOpenLastWeek, opened.Add));

        cut.WaitForElement("button[data-testid=last-week-row]").Click();

        Assert.That(opened, Is.EqualTo(new[] { Week.Previous() }));
    }

    [Test]
    public void AndroidBack_HasNoHandler()
    {
        Context.Render<ThisWeekPage>();

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public async Task AppStarted_ReloadsWithTheSpinnerThenShowsTheNewValues()
    {
        MidWeek();
        var cut = await LoadedAsync();
        training.Hold = true;
        totals.Streaks = Streaks(6);

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1)));
        Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2));
        training.Complete();
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=this-week-streak]").TextContent, Is.EqualTo("6-week streak")));
        Assert.That(totals.Comparisons, Has.Count.EqualTo(4));
        await started;
    }

    [Test]
    public async Task AppStarted_ADayLater_ReadsTheNewDay()
    {
        MidWeek();
        var cut = await LoadedAsync();
        time.SetUtcNow(new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero));

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=this-week-dates]").TextContent, Is.EqualTo("23 – 29 Nov")));
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        MidWeek();
        var cut = await LoadedAsync();
        Fail("series");

        await lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=this-week-error]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task AppStarted_WhileLoading_IsIgnored()
    {
        training.Hold = true;
        Context.Render<ThisWeekPage>();

        var started = lifecycle.NotifyStartedAsync();
        await WaitForRendererAsync();

        Assert.That(training.WorkoutsReads, Has.Count.EqualTo(1));
        training.Complete();
        await started.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        await LoadedAsync();

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(training.WorkoutsReads, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Disposed_WhileLoading_SkipsTheUpdate()
    {
        MidWeek();
        training.Hold = true;
        Context.Render<ThisWeekPage>();

        await Context.DisposeComponentsAsync();
        training.Workouts = new ReadFailed();
        training.Complete();
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(training.WorkoutsReads, Has.Count.EqualTo(1));
            Assert.That(logger.Collector.Count, Is.Zero);
        });
    }

    [Test]
    public async Task Disposed_WhileReloading_ReloadFinishesWithoutError()
    {
        MidWeek();
        await LoadedAsync();
        training.Hold = true;
        var started = lifecycle.NotifyStartedAsync();
        Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2));

        await Context.DisposeComponentsAsync();
        training.Complete();
        await WaitForRendererAsync();

        Assert.That(async () => await started, Throws.Nothing);
        Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
    }

    [Test]
    public async Task AppStarted_PageDisposedByAnEarlierHandler_DoesNotReload()
    {
        lifecycle.Register(() => Context.DisposeComponentsAsync());
        await LoadedAsync();

        await lifecycle.NotifyStartedAsync();

        Assert.Multiple(() =>
        {
            Assert.That(training.WorkoutsReads, Has.Count.EqualTo(1));
            Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
        });
    }

    [Test]
    public async Task Refresh_FastReload_KeepsTheSpinnerUpForHalfASecond()
    {
        MidWeek();
        var cut = await LoadedAsync();
        totals.Streaks = Streaks(6);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2)));
        await WaitForRendererAsync();
        time.Advance(TimeSpan.FromMilliseconds(499));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=week-tiles]"), Is.Empty);
        });
        time.Advance(TimeSpan.FromMilliseconds(1));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=this-week-streak]").TextContent, Is.EqualTo("6-week streak"));
        });
    }

    [Test]
    public async Task Refresh_SlowReload_ShowsTheNewValuesAsSoonAsItFinishes()
    {
        MidWeek();
        var cut = await LoadedAsync();
        totals.Streaks = Streaks(6);
        training.Hold = true;

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2)));
        time.Advance(TimeSpan.FromMilliseconds(800));
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1));
        training.Complete();
        await refresh;
        Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Is.Empty);
        Assert.That(cut.Find("[data-testid=this-week-streak]").TextContent, Is.EqualTo("6-week streak"));
    }

    [Test]
    public async Task Refresh_Failed_ShowsTheErrorAfterHalfASecond()
    {
        MidWeek();
        var cut = await LoadedAsync();
        Fail("streaks");

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.WaitForAssertion(() => Assert.That(training.WorkoutsReads, Has.Count.EqualTo(2)));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=this-week-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=this-week-error]"), Is.Empty);
        });
        time.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.That(cut.FindAll("[data-testid=this-week-error]"), Has.Count.EqualTo(1));
    }

    private static ReadResult<PeriodComparison> Comparison(WeekTally totals, WeekTally earlier) => new PeriodComparison(Week, totals, earlier);

    private static ReadResult<WeekStreaks> Streaks(int current) => new WeekStreaks(current, new int[12], new LongestRun(current + 3, true));

    private async Task<IRenderedComponent<ThisWeekPage>> LoadedAsync()
    {
        var cut = Context.Render<ThisWeekPage>();
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=week-tiles]"), Has.Count.EqualTo(1)));
        return cut;
    }

    /// <summary>
    /// Mon 16 and Wed 18 Nov trained: a 30 minute workout of squats on Monday 18:00 UTC and a one hour one of squats and a run on Wednesday
    /// 10:00 UTC, a 5-week streak, a Squat record on Wednesday and a run record on Monday, and 12 sets of Legs.
    /// </summary>
    private void MidWeek()
    {
        totals.ThisWeek = Comparison(new WeekTally(2, new TimeSpan(1, 30, 0), 20), new WeekTally(2, new TimeSpan(2, 0, 0), 33));
        totals.LastWeek = Comparison(new WeekTally(4, new TimeSpan(4, 10, 0), 67), WeekTally.None);
        totals.Series = new PeriodSeries(
            Week,
            [
                new PeriodBucket(Monday, Monday, 1, TimeSpan.FromMinutes(30)),
                new PeriodBucket(Monday.AddDays(1), Monday.AddDays(1), 0, TimeSpan.Zero),
                new PeriodBucket(Monday.AddDays(2), Monday.AddDays(2), 1, TimeSpan.FromHours(1)),
            ],
            [],
            [],
            0);
        totals.Streaks = Streaks(5);
        var monday = new DateTimeOffset(2026, 11, 16, 18, 0, 0, TimeSpan.Zero);
        training.Workouts = new ReadResult<IReadOnlyList<WorkoutOutline>>(new[]
        {
            new WorkoutOutline(31, monday, monday.AddMinutes(30), [Squat]),
            new WorkoutOutline(32, Now, Now.AddHours(1), [Squat, Run]),
        });
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
                break;
            case "lastWeek":
                totals.LastWeek = new ReadFailed();
                break;
            case "series":
                totals.Series = new ReadFailed();
                break;
            case "streaks":
                totals.Streaks = new ReadFailed();
                break;
            case "workouts":
                training.Workouts = new ReadFailed();
                break;
            default:
                training.Muscles = new ReadFailed();
                break;
        }
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
