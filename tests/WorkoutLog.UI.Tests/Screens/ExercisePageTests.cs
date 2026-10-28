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
    private const int Plank = 258;
    private const int BulgarianSplitSquat = 58;
    private const int AssistedPullUp = 218;
    private const int FarmersWalk = 187;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly StrengthSession Heavy = new(3, Now.AddDays(-8), [Set(8, 62.5m), Set(3, 70m)]);
    private static readonly StrengthSession Light = new(2, Now.AddDays(-15), [Set(5, 60m), Set(8, null)]);
    private static readonly StrengthSession Pulled = new(5, Now.AddDays(-8), [Set(8, 5m), Set(5, null)]);
    private static readonly StrengthSession Planked = new(6, Now.AddDays(-8), [new(null, null, TimeSpan.FromSeconds(105), null), new(null, 10m, TimeSpan.FromSeconds(60), null)]);
    private static readonly StrengthSession Walked = new(7, Now.AddDays(-8), [new(null, 32m, TimeSpan.FromSeconds(45), 40m)]);

    private FakeExerciseProgressService progress = null!;
    private FakeCardioProgressService cardio = null!;

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
        cardio = new FakeCardioProgressService();
        Context.Services.AddSingleton<ICardioProgressService>(cardio);
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
            Assert.That(cardio.Loads, Is.Empty);
            Assert.That(progress.LiftSummaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths }));
            Assert.That(progress.LiftSummaries.Single().Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(progress.BodyweightSummaries, Is.Empty);
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
            Assert.That(progress.LiftSummaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths, ChartRange.All }));
            Assert.That(progress.LiftSummaries[^1].Sessions, Is.SameAs(progress.Result.Sessions));
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
        progress.Summary = (_, _) => new LiftProgress([new LiftSessionProgress(new StrengthSession(1, Now, [Set(8, null)]), null, null)], new LiftRecords(null, null));

        var cut = Render(Squat);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sets in the last 3 months"));
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
            Assert.That(progress.LiftSummaries[^1].Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(cut.FindAll("[data-testid=exercise-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercise-session-]"), Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Assisted_ShowsOneFlippedLeastAssistChartWithNoSwitcher()
    {
        progress.AssistedSummary = (_, _) => new AssistedProgress(
            [new AssistedSessionProgress(Pulled, new ExerciseBest(0m, 5, null, null))],
            new AssistedRecords(new ProgressRecord<ExerciseBest>(new ExerciseBest(0m, 5, null, null), Pulled.StartedAt)));

        var cut = Render(AssistedPullUp);

        var view = cut.FindComponent<ExerciseView>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That(progress.AssistedSummaries.Select(s => s.Range), Is.EqualTo(new[] { ChartRange.ThreeMonths }));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercise-chart-card] .card-title").TextContent, Is.EqualTo("Least assist per session"));
            Assert.That(view.Flipped, Is.True);
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 0kg ×5" }));
            Assert.That(cut.Find("[data-testid=exercise-chart-caption]").TextContent, Is.EqualTo("Up = less assist"));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "0kg × 5" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-5] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 20 Oct", "0kg × 5", "5kg×8 · 0kg×5" }));
        });
    }

    [Test]
    public void Hold_OpensOnDurationAndWeightShowsTheHeaviestWithItsDuration()
    {
        var heaviest = new ExerciseBest(10m, null, TimeSpan.FromSeconds(60), null);
        progress.HoldSummary = (_, _) => new HoldProgress(
            [new HoldSessionProgress(Planked, TimeSpan.FromSeconds(105), heaviest)],
            new HoldRecords(new ProgressRecord<TimeSpan>(TimeSpan.FromSeconds(105), Planked.StartedAt), new ProgressRecord<ExerciseBest>(heaviest, Planked.StartedAt)));
        var cut = Render(Plank);
        Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-duration" }));
        Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 1:45" }));
        Assert.That(cut.FindComponent<ExerciseView>().Instance.TimeScale, Is.True);

        cut.Find("[data-testid=exercise-tab-weight]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(cut.FindComponent<ExerciseView>().Instance.TimeScale, Is.False);
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 10kg 1:00" }));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "1:45", "10kg · 1:00" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-6] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 20 Oct", "1:45", "1:45 · 10kg 1:00" }));
        });
    }

    [Test]
    public void Hold_WeightWithNoWeightedSets_ShowsNoSets()
    {
        progress.HoldSummary = (_, _) => new HoldProgress([new HoldSessionProgress(Planked, TimeSpan.FromSeconds(105), null)], new HoldRecords(null, null));
        var cut = Render(Plank);

        cut.Find("[data-testid=exercise-tab-weight]").Change(true);

        Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sets in the last 3 months"));
    }

    [Test]
    public void Carry_OpensOnWeightAndDistanceShowsTheLongestInMetres()
    {
        var best = new ExerciseBest(32m, null, null, 40m);
        progress.CarrySummary = (_, _) => new CarryProgress(
            [new CarrySessionProgress(Walked, best, best)],
            new CarryRecords(new ProgressRecord<ExerciseBest>(best, Walked.StartedAt), new ProgressRecord<ExerciseBest>(best, Walked.StartedAt)));
        var cut = Render(FarmersWalk);
        Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-weight" }));
        Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 32kg 40m" }));

        cut.Find("[data-testid=exercise-tab-distance]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 40m 32kg" }));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "32kg · 40m", "40m · 32kg" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-7] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 20 Oct", "32kg · 40m", "32kg 40m 0:45" }));
        });
    }

    [Test]
    public void Carry_WeightWithNoWeightedSets_ShowsNoSets()
    {
        progress.CarrySummary = (_, _) => new CarryProgress(
            [new CarrySessionProgress(Walked, null, new ExerciseBest(null, null, null, 60m))], new CarryRecords(null, null));

        var cut = Render(FarmersWalk);

        Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sets in the last 3 months"));
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

    [Test]
    public void Bodyweight_OpensOnBestSetWithItsTabsRecordsAndSessions()
    {
        UseBodyweight();

        var cut = Render(PushUp);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-tab-best-set", "exercise-tab-total", "exercise-tab-top-set" }));
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-tab-best-set" }));
            Assert.That(cut.FindAll("[data-testid=exercise-range] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercise-range-3m" }));
            Assert.That(progress.BodyweightSummaries.Single().Sessions, Is.SameAs(progress.Result.Sessions));
            Assert.That(progress.LiftSummaries, Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "13 Oct: 25", "20 Oct: 20" }));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "25 reps", "63 reps", "16kg × 8" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-6] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 20 Oct", "16kg × 8", "20 · 16×8" }));
            Assert.That(cut.FindAll("[data-testid=exercise-session-5] span").Select(s => s.TextContent), Is.EqualTo(new[] { "Tue 13 Oct", "25 reps", "25 · 20 · 18" }));
        });
    }

    [TestCase("total", new[] { "13 Oct: 63", "20 Oct: 20" })]
    [TestCase("top-set", new[] { "20 Oct: 16kg ×8" })]
    public void Bodyweight_ChoosingATab_ShowsItsValuesWithoutLoadingAgain(string tab, string[] values)
    {
        UseBodyweight();
        var cut = Render(PushUp);

        cut.Find($"[data-testid=exercise-tab-{tab}]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(progress.Loads, Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(values));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-caption]"), Is.Empty);
        });
    }

    [Test]
    public void LoadedBodyweight_HasTheE1rmTabAndRecord()
    {
        UseBodyweight();
        var cut = Render(BulgarianSplitSquat);

        cut.Find("[data-testid=exercise-tab-e1rm]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercise-tabs] input").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercise-tab-best-set", "exercise-tab-total", "exercise-tab-top-set", "exercise-tab-e1rm" }));
            Assert.That(cut.FindAll("[data-testid=exercise-chart-values] li").Select(v => v.TextContent), Is.EqualTo(new[] { "20 Oct: 20.3kg" }));
            Assert.That(cut.Find("[data-testid=exercise-chart-caption]").TextContent, Is.EqualTo("Estimated one-rep max"));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "25 reps", "63 reps", "16kg × 8", "20.3kg" }));
        });
    }

    [Test]
    public void Bodyweight_TabWithNoValues_ShowsNoSets()
    {
        var session = new StrengthSession(1, Now, [Set(20, null)]);
        progress.Result = new ExerciseProgressResult([session], Failed: false);
        progress.BodyweightSummary = (_, _) => new BodyweightProgress(
            [new BodyweightSessionProgress(session, 20, 20, null, null)],
            new BodyweightRecords(new ProgressRecord<int>(20, Now), new ProgressRecord<int>(20, Now), null, null));
        var cut = Render(PushUp);

        cut.Find("[data-testid=exercise-tab-top-set]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercise-chart-empty]").TextContent, Is.EqualTo("No sets in the last 3 months"));
            Assert.That(cut.FindAll("[data-testid=exercise-records] .value").Select(v => v.TextContent), Is.EqualTo(new[] { "20 reps", "20 reps", "—" }));
        });
    }

    /// <summary>Two bodyweight sessions: 13 Oct unweighted (25 · 20 · 18), 20 Oct mixed (20 · 16×8).</summary>
    private void UseBodyweight()
    {
        var unweighted = new StrengthSession(5, Now.AddDays(-15), [Set(25, null), Set(20, null), Set(18, null)]);
        var mixed = new StrengthSession(6, Now.AddDays(-8), [Set(20, null), Set(8, 16m)]);
        var e1rm = 16m * (1 + 8 / 30m);
        progress.Result = new ExerciseProgressResult([unweighted, mixed], Failed: false);
        progress.BodyweightSummary = (_, _) => new BodyweightProgress(
            [
                new BodyweightSessionProgress(unweighted, 25, 63, null, null),
                new BodyweightSessionProgress(mixed, 20, 20, Top(16m, 8), e1rm),
            ],
            new BodyweightRecords(
                new ProgressRecord<int>(25, unweighted.StartedAt),
                new ProgressRecord<int>(63, unweighted.StartedAt),
                new ProgressRecord<ExerciseBest>(Top(16m, 8), mixed.StartedAt),
                new ProgressRecord<decimal>(e1rm, mixed.StartedAt)));
    }

    private IRenderedComponent<ExercisePage> Render(int exerciseId) =>
        Context.Render<ExercisePage>(p => p.Add(x => x.ExerciseId, exerciseId));

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);
}
