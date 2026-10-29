using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ExercisePageCardioTests : ComponentTestBase
{
    private const int Run = 311;
    private const int StairClimber = 309;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly CardioSession Long = new(61, Now.AddDays(-17), Entry(TimeSpan.FromMinutes(30), 5.2m, 410));
    private static readonly CardioSession Short = new(60, Now.AddDays(-24), Entry(TimeSpan.FromSeconds(1760), 5m, null));

    private FakeExerciseProgressService lifts = null!;
    private FakeCardioProgressService progress = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        Context.Services.AddSingleton<TimeProvider>(clock);
        lifts = new FakeExerciseProgressService();
        Context.Services.AddSingleton<IExerciseProgressService>(lifts);
        progress = new FakeCardioProgressService
        {
            Result = new CardioProgressResult([Short, Long], Failed: false),
            Summary = (_, range) => range == ChartRange.All
                ? new CardioProgress([], new CardioRecords(null, null, null, null, null, null))
                : new CardioProgress(
                    [
                        new CardioSessionProgress(Short, new CardioBest(5m, TimeSpan.FromSeconds(1760), TimeSpan.FromSeconds(352), null, null, null)),
                        new CardioSessionProgress(Long, new CardioBest(5.2m, TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(346), null, null, 410)),
                    ],
                    new CardioRecords(
                        new ProgressRecord<decimal>(5.2m, Long.StartedAt),
                        new ProgressRecord<TimeSpan>(TimeSpan.FromMinutes(30), Long.StartedAt),
                        new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(346), Long.StartedAt),
                        null,
                        null,
                        new ProgressRecord<int>(410, Long.StartedAt))),
        };
        Context.Services.AddSingleton<ICardioProgressService>(progress);
    }

    [Test]
    public async Task WhileLoading_ShowsTheSpinnerUnderTheNameAndCardio()
    {
        progress.Hold = true;
        var cut = Render(Run);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Outdoor Run"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Cardio"));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-card]"), Is.Empty);
        });

        progress.Complete();
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-loading]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Loaded_OpensOnDistanceForTheLast3MonthsThroughTheCardioService()
    {
        var cut = Render(Run);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Is.EqualTo(new[] { Run }));
            Assert.That(lifts.Loads, Is.Empty);
            Assert.That(progress.Summaries.Single().Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(progress.Summaries.Single().Range, Is.EqualTo(ChartRange.ThreeMonths));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Cardio"));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-tab-distance", "exercise-tab-duration", "exercise-tab-pace", "exercise-tab-calories" }));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-distance" }));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "4 Oct: 5km", "11 Oct: 5.2km" }));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-caption]"), Is.Empty);
            Assert.That((cut.FindComponent<Chart>().Instance.Flipped, cut.FindComponent<Chart>().Instance.TimeScale), Is.EqualTo((false, false)));
        });
    }

    [Test]
    public void Loaded_ShowsTheRecordsAndSessionsNewestFirst()
    {
        var cut = Render(Run);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent),
                Is.EqualTo(new[] { "5.2km", "30:00", "5:46/km", "410 calories" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]").Select(s => s.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-session-61", "exercise-session-60" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-61] span").Select(s => s.TextContent),
                Is.EqualTo(new[] { "Sun 11 Oct", "5.2km", "30:00 · 5:46/km · 410 calories" }));
        });
    }

    [Test]
    public void ChoosingPace_ShowsTheTimesWithTheCaptionWithoutLoadingAgain()
    {
        var cut = Render(Run);

        cut.Find("[data-testid=exercise-tab-pace]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "4 Oct: 5:52", "11 Oct: 5:46" }));
            Assert.That(cut.Find("[data-testid=exercise-chart-caption]").TextContent, Is.EqualTo("Up = faster"));
            Assert.That((cut.FindComponent<Chart>().Instance.Flipped, cut.FindComponent<Chart>().Instance.TimeScale), Is.EqualTo((true, true)));
        });
    }

    [Test]
    public void ChoosingDuration_ShowsTheTimes()
    {
        var cut = Render(Run);

        cut.Find("[data-testid=exercise-tab-duration]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "4 Oct: 29:20", "11 Oct: 30:00" }));
            Assert.That((cut.FindComponent<Chart>().Instance.Flipped, cut.FindComponent<Chart>().Instance.TimeScale), Is.EqualTo((false, true)));
        });
    }

    [Test]
    public void ChoosingATabWithNoValues_ShowsItsMessage()
    {
        progress.Summary = (_, _) => new CardioProgress(
            [new CardioSessionProgress(Short, new CardioBest(5m, TimeSpan.FromSeconds(1760), TimeSpan.FromSeconds(352), null, null, null))],
            new CardioRecords(null, null, null, null, null, null));
        var cut = Render(Run);

        cut.Find("[data-testid=exercise-tab-calories]").Change(true);

        Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No calories logged in the last 3 months"));
    }

    [Test]
    public void ChoosingARange_SummarisesForItWithoutLoadingAgain()
    {
        var cut = Render(Run);

        cut.Find("[data-testid=exercise-range-all]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(progress.Summaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths, ChartRange.All }));
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sessions yet"));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "—", "—", "—", "—" }));
        });
    }

    [Test]
    public void NoDistance_OpensOnDurationWithFloors()
    {
        progress.Result = new CardioProgressResult([], Failed: false);
        progress.Summary = (_, _) => new CardioProgress([], new CardioRecords(null, null, null, null, null, null));

        var cut = Render(StairClimber);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-tab-duration", "exercise-tab-floors", "exercise-tab-calories" }));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-duration" }));
        });
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainLoadsAgain()
    {
        progress.Result = new CardioProgressResult([], Failed: true);
        var cut = Render(Run);
        Assume.That(cut.Find("[data-testid=exercise-error]").TextContent, Is.EqualTo("Couldn't load this exercise"));
        progress.Result = new CardioProgressResult([Short, Long], Failed: false);

        cut.Find("[data-testid=exercise-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Is.EqualTo(new[] { Run, Run }));
            Assert.That(cut.FindAll("[data-testid=exercise-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    private IRenderedComponent<ExercisePage> Render(int exerciseId) =>
        Context.Render<ExercisePage>(p => p.Add(x => x.ExerciseId, exerciseId));

    private static CardioEntry Entry(TimeSpan duration, decimal distance, int? calories) =>
        new(duration, distance, null, null, null, calories, null);
}
