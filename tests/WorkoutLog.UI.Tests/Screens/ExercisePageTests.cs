using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ExercisePageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int PushUp = 264;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly LiftSession Heavy = new(3, Now.AddDays(-8), [Set(8, 62.5m), Set(3, 70m)]);
    private static readonly LiftSession Light = new(2, Now.AddDays(-15), [Set(5, 60m), Set(8, null)]);

    private FakeExerciseProgressService progress = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        Context.Services.AddSingleton<TimeProvider>(clock);
        progress = new FakeExerciseProgressService
        {
            Result = new ExerciseProgressResult([Light, Heavy], Failed: false),
            Summary = (sessions, range) => range == ChartRange.All
                ? new LiftProgress([], new LiftRecords(null, null))
                : new LiftProgress(
                    [
                        new LiftSessionProgress(Light, Top(60m, 5), 70m),
                        new LiftSessionProgress(Heavy, Top(70m, 3), 62.5m * (1 + 8 / 30m)),
                    ],
                    new LiftRecords(new ProgressRecord<ExerciseBest>(Top(70m, 3), Heavy.StartedAt), new ProgressRecord<decimal>(62.5m * (1 + 8 / 30m), Heavy.StartedAt))),
        };
        Context.Services.AddSingleton<IExerciseProgressService>(progress);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerUnderTheNameAndSubtitle()
    {
        progress.Hold = true;

        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Barbell Back Squat"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Quadriceps · Barbell"));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-card]"), Is.Empty);
        });
    }

    [Test]
    public async Task LoadCompletes_ReplacesTheSpinnerWithTheContent()
    {
        progress.Hold = true;
        var cut = Render(Squat);

        progress.Complete();
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-loading]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void OneSession_ShowsOnePointItsRecordsAndItsRow()
    {
        progress.Result = new ExerciseProgressResult([Heavy], Failed: false);
        progress.Summary = (_, _) => new LiftProgress(
            [new LiftSessionProgress(Heavy, Top(70m, 3), 62.5m * (1 + 8 / 30m))],
            new LiftRecords(new ProgressRecord<ExerciseBest>(Top(70m, 3), Heavy.StartedAt), new ProgressRecord<decimal>(62.5m * (1 + 8 / 30m), Heavy.StartedAt)));

        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 70kg ×3" }));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "70kg × 3", "79.2kg" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]").Select(s => s.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-session-3" }));
        });
    }

    [Test]
    public void Loaded_OpensOnTopSetForTheLast3Months()
    {
        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Is.EqualTo(new[] { Squat }));
            Assert.That(progress.Summaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths }));
            Assert.That(progress.Summaries.Single().Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-top-set" }));
            Assert.That(cut.FindAll("[data-testid=exercise-range] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-range-3m" }));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "13 Oct: 60kg ×5", "20 Oct: 70kg ×3" }));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-caption]"), Is.Empty);
        });
    }

    [Test]
    public void Loaded_ShowsTheRecordsAndSessionsNewestFirst()
    {
        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "70kg × 3", "79.2kg" }));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .date").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct", "20 Oct" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]").Select(s => s.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-session-3", "exercise-session-2" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-2] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 13 Oct", "60kg × 5", "60×5 · 8 reps" }));
        });
    }

    [Test]
    public void ChoosingE1rm_ShowsTheEstimatesWithTheCaptionWithoutLoadingAgain()
    {
        var cut = Render(Squat);

        cut.Find("[data-testid=exercise-tab-e1rm]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "13 Oct: 70kg", "20 Oct: 79.2kg" }));
            Assert.That(cut.Find("[data-testid=exercise-chart-caption]").TextContent, Is.EqualTo("Estimated one-rep max"));
        });
    }

    [Test]
    public void ChoosingARange_SummarisesForItWithoutLoadingAgain()
    {
        var cut = Render(Squat);

        cut.Find("[data-testid=exercise-range-all]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(progress.Summaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths, ChartRange.All }));
            Assert.That(progress.Summaries[^1].Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sessions yet"));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "—", "—" }));
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Is.Empty);
        });
    }

    [Test]
    public void NoSessionsInTheRange_ShowsTheViewsMessage()
    {
        progress.Summary = (_, _) => new LiftProgress([], new LiftRecords(null, null));

        var cut = Render(Squat);

        Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sessions in the last 3 months"));
    }

    [Test]
    public void SessionsWithNoValuesForTheTab_ShowTheTabsMessage()
    {
        progress.Summary = (_, _) => new LiftProgress([new LiftSessionProgress(new LiftSession(1, Now, [Set(8, null)]), null, null)], new LiftRecords(null, null));

        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No weighted sets in the last 3 months"));
            Assert.That(cut.FindAll("[data-testid=exercise-session-1] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Wed 28 Oct", "8 reps", "8 reps" }));
        });
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainLoadsAgain()
    {
        progress.Result = new ExerciseProgressResult([], Failed: true);
        var cut = Render(Squat);
        Assume.That(cut.Find("[data-testid=exercise-error]").TextContent, Is.EqualTo("Couldn't load this exercise"));
        progress.Result = new ExerciseProgressResult([Light, Heavy], Failed: false);

        cut.Find("[data-testid=exercise-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Is.EqualTo(new[] { Squat, Squat }));
            Assert.That(progress.Summaries[^1].Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(cut.FindAll("[data-testid=exercise-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void NotALift_ShowsLoadFailedUnderItsName()
    {
        progress.Result = new ExerciseProgressResult([], Failed: true);

        var cut = Render(PushUp);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Push-up"));
            Assert.That(cut.Find("[data-testid=exercise-error]").TextContent, Is.EqualTo("Couldn't load this exercise"));
        });
    }

    [Test]
    public void UnknownExercise_ShowsLoadFailedWithNoName()
    {
        progress.Result = new ExerciseProgressResult([], Failed: true);

        var cut = Render(999);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=screen-subtitle]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-error]").TextContent, Is.EqualTo("Couldn't load this exercise"));
        });
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<ExercisePage>(p => p.Add(x => x.ExerciseId, Squat).Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    private IRenderedComponent<ExercisePage> Render(int exerciseId) =>
        Context.Render<ExercisePage>(p => p.Add(x => x.ExerciseId, exerciseId));

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);
}
