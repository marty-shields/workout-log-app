using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutSummaryPageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Curl = 132;

    private static readonly SavedWorkoutSummary Loaded = new(
        new TimeSpan(1, 5, 0),
        2,
        4,
        HasEarlierWorkout: true,
        [
            new SummaryExercise(Squat, new ExerciseBest(102.5m, 3, null, null), IsRecord: true, new BestChange(BestChangeKind.Up, new ExerciseBest(2.5m, null, null, null))),
            new SummaryExercise(Curl, new ExerciseBest(10m, 8, null, null), IsRecord: false, new BestChange(BestChangeKind.Equal, null)),
        ]);

    private FakeWorkoutSummaryService summaries = null!;

    [SetUp]
    public void SetUp()
    {
        summaries = new FakeWorkoutSummaryService { Result = new WorkoutSummaryResult(Loaded, Failed: false) };
        Context.Services.AddSingleton<IWorkoutSummaryService>(summaries);
    }

    [Test]
    public void Loaded_ShowsTheTotalsTheRecordAndEachChange()
    {
        var cut = Render(7);

        Assert.Multiple(() =>
        {
            Assert.That(summaries.Loads, Is.EqualTo(new[] { 7 }));
            Assert.That(cut.Find("[data-testid=summary-duration]").TextContent, Is.EqualTo("1:05:00"));
            Assert.That(cut.Find("[data-testid=summary-exercises]").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=summary-sets]").TextContent, Is.EqualTo("4"));
            Assert.That(cut.FindAll("[data-testid^=summary-record-]").Select(r => TestText.Normalise(r.TextContent)), Is.EqualTo(new[] { "Barbell Back Squat · 102.5kg × 3" }));
            Assert.That(TestText.Normalise(cut.Find($"[data-testid=summary-change-{Squat}]").TextContent), Does.Contain("up 2.5kg"));
            Assert.That(TestText.Normalise(cut.Find($"[data-testid=summary-change-{Curl}]").TextContent), Does.Contain("no change"));
        });
    }

    [Test]
    public void FirstWorkout_ShowsNoRecordsOrChanges()
    {
        summaries.Result = new WorkoutSummaryResult(Loaded with { HasEarlierWorkout = false }, Failed: false);

        var cut = Render(7);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=summary-first-workout]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=summary-changes]"), Is.Empty);
        });
    }

    [Test]
    public async Task WhileLoading_ShowsTheSpinnerAndDone()
    {
        summaries.Hold = true;
        var cut = Render(7);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=summary-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=done]"), Has.Count.EqualTo(1));
        });

        summaries.Complete();
        await WaitForRendererAsync();
        Assert.That(cut.FindAll("[data-testid=summary-loading]"), Is.Empty);
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndDoneStillWorks()
    {
        summaries.Result = new WorkoutSummaryResult(null, Failed: true);
        var done = 0;
        var cut = Render(7, () => done++);

        Assert.That(cut.FindAll("[data-testid=summary-error]"), Has.Count.EqualTo(1));
        cut.Find("[data-testid=done]").Click();

        Assert.That(done, Is.EqualTo(1));
    }

    [Test]
    public void TryAgain_LoadsAgain()
    {
        summaries.Result = new WorkoutSummaryResult(null, Failed: true);
        var cut = Render(7);
        summaries.Result = new WorkoutSummaryResult(Loaded, Failed: false);

        cut.Find("[data-testid=summary-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(summaries.Loads, Is.EqualTo(new[] { 7, 7 }));
            Assert.That(cut.FindAll("[data-testid=summary-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=summary-stats]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task DisposedWhileLoading_IgnoresTheResult()
    {
        summaries.Hold = true;
        Render(7);
        await Context.DisposeComponentsAsync();

        summaries.Complete();
        await Task.Delay(50);

        Assert.That(summaries.Loads, Is.EqualTo(new[] { 7 }));
    }

    [Test]
    public void EarlierWorkoutButNoShownExercises_ShowsNoSinceLastTimeCard()
    {
        summaries.Result = new WorkoutSummaryResult(Loaded with { Exercises = [] }, Failed: false);

        var cut = Render(7);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=summary-first-workout]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=summary-records-none]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=summary-changes]"), Is.Empty);
        });
    }

    [Test]
    public void Done_RaisesOnDone()
    {
        var done = 0;
        var cut = Render(7, () => done++);

        cut.Find("[data-testid=done]").Click();

        Assert.That(done, Is.EqualTo(1));
    }

    private IRenderedComponent<WorkoutSummaryPage> Render(int workoutId, Action? onDone = null) =>
        Context.Render<WorkoutSummaryPage>(p => p
            .Add(x => x.WorkoutId, workoutId)
            .Add(x => x.OnDone, onDone ?? (() => { })));
}
