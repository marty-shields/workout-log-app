using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Models.Summary;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutSummaryViewTests : ComponentTestBase
{
    [Test]
    public void RecordsAndChanges_ShowsTotalsRecordsAndChanges()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workout saved"));
            Assert.That(cut.Find("[data-testid=summary-duration]").TextContent, Is.EqualTo("1:12:04"));
            Assert.That(cut.Find("[data-testid=summary-exercises]").TextContent, Is.EqualTo("6"));
            Assert.That(cut.Find("[data-testid=summary-sets]").TextContent, Is.EqualTo("17"));
            Assert.That(cut.Find("[data-testid=summary-records] h2").TextContent, Is.EqualTo("Records"));
            Assert.That(cut.FindAll("[data-testid^=summary-record-]").Select(r => TestText.Normalise(r.TextContent)),
                Is.EqualTo(new[] { "Barbell Bench Press · 90kg × 5", "Deadlift · 160kg × 3" }));
            Assert.That(cut.Find("[data-testid=summary-changes] h2").TextContent, Is.EqualTo("Since last time"));
            Assert.That(cut.FindAll("[data-testid^=summary-change-]"), Has.Count.EqualTo(6));
            Assert.That(cut.FindAll("[data-testid=summary-records-none], [data-testid=summary-first-workout]"), Is.Empty);
        });
    }

    [Test]
    public void Records_ShowTheRecordIconInSuccessTone()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        var icons = cut.FindComponents<Icon>().Where(i => i.Instance.Name == IconName.Record).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(icons, Has.Count.EqualTo(2));
            Assert.That(icons.Select(i => i.Instance.Tone), Is.All.EqualTo(IconTone.Success));
        });
    }

    [TestCase(8, "up 2.5kg", IconName.Up, "up", "success")]
    [TestCase(132, "down 2kg", IconName.Down, "down", "error")]
    public void Changes_ShowTheDifferenceWithItsIconAndAreHeardAsUpOrDown(int id, string heard, IconName name, string direction, string tone)
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        var change = cut.Find($"[data-testid=summary-change-{id}] .change");
        var expectedPath = Context.Render<Icon>(p => p.Add(x => x.Name, name)).Find("path").GetAttribute("d");

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo(heard));
            Assert.That(change.ClassList, Does.Contain(direction));
            Assert.That(change.QuerySelector("svg")!.ClassList, Does.Contain(tone));
            Assert.That(change.QuerySelector("path")!.GetAttribute("d"), Is.EqualTo(expectedPath));
        });
    }

    [Test]
    public void EqualChange_ShowsADashHeardAsNoChange()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        var change = cut.Find("[data-testid=summary-change-6] .change");

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo("—no change"));
            Assert.That(change.QuerySelector("[aria-hidden=true]")!.TextContent, Is.EqualTo("—"));
            Assert.That(change.QuerySelector("svg"), Is.Null);
        });
    }

    [Test]
    public void FirstTimeChange_SaysFirstTime()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        var change = cut.Find("[data-testid=summary-change-258] .change");

        Assert.Multiple(() =>
        {
            Assert.That(change.TextContent, Is.EqualTo("First time"));
            Assert.That(change.QuerySelector("svg"), Is.Null);
        });
    }

    [Test]
    public void Changes_ShowTheBestUnderTheExerciseInOrder()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        var rows = cut.FindAll("[data-testid^=summary-change-]").Select(r => (r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent));

        Assert.That(rows, Is.EqualTo(new[]
        {
            ("Barbell Back Squat", "100kg × 5"),
            ("Barbell Bench Press", "90kg × 5"),
            ("Deadlift", "160kg × 3"),
            ("Machine Assisted Pull-up", "20kg × 8"),
            ("Dumbbell Curl", "12kg × 10"),
            ("Plank", "1:00"),
        }));
    }

    [Test]
    public void NoRecords_SaysSoAndStillShowsChanges()
    {
        var cut = Render(SampleWorkoutSummary.NoRecords);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=summary-records-none]").TextContent, Is.EqualTo("No records this time"));
            Assert.That(cut.FindAll("[data-testid^=summary-record-]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=summary-change-]"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void FirstWorkout_ShowsTotalsAndANoteInPlaceOfRecordsAndChanges()
    {
        var cut = Render(SampleWorkoutSummary.FirstWorkout);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=summary-duration]").TextContent, Is.EqualTo("0:52:00"));
            Assert.That(cut.Find("[data-testid=summary-first-workout]").TextContent, Is.EqualTo("Your first workout. Records and changes show from your next one."));
            Assert.That(cut.FindAll("[data-testid=summary-records], [data-testid=summary-changes]"), Is.Empty);
        });
    }

    [Test]
    public void AllFirstTime_ShowsFirstTimeForEveryExercise()
    {
        var cut = Render(SampleWorkoutSummary.AllFirstTime);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=summary-change-] .change").Select(c => c.TextContent), Is.EqualTo(new[] { "First time", "First time", "First time" }));
            Assert.That(cut.Find("[data-testid=summary-records-none]"), Is.Not.Null);
            Assert.That(cut.FindAll("[data-testid=summary-first-workout]"), Is.Empty);
        });
    }

    [Test]
    public void Loading_ShowsTheSpinnerInsteadOfTheSummary()
    {
        var cut = Context.Render<WorkoutSummaryView>(p => p.Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=summary-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("[data-testid=summary-stats], [data-testid=summary-error]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Workout saved"));
            Assert.That(cut.FindAll("[data-testid=done]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheSummary()
    {
        var retries = 0;
        var cut = Context.Render<WorkoutSummaryView>(p => p
            .Add(x => x.Summary, SampleWorkoutSummary.RecordsAndChanges)
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=summary-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=summary-error]").TextContent, Is.EqualTo("Couldn't load the summary"));
            Assert.That(cut.Find("[data-testid=summary-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=summary-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=summary-stats]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=done]"), Has.Count.EqualTo(1));
            Assert.That(retries, Is.EqualTo(1));
        });
    }

    [Test]
    public void FailedWhileLoading_ShowsTheSpinner()
    {
        var cut = Context.Render<WorkoutSummaryView>(p => p.Add(x => x.Loading, true).Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=summary-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=summary-error]"), Is.Empty);
        });
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Done_IsInTheBottomBarInEveryStateAndRaisesOnDone(bool loading, bool failed)
    {
        var done = 0;
        var cut = Context.Render<WorkoutSummaryView>(p => p
            .Add(x => x.Summary, SampleWorkoutSummary.RecordsAndChanges)
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnDone, () => done++));

        var button = cut.Find("[data-testid=bottom-bar] [data-testid=done]");
        button.Click();

        Assert.Multiple(() =>
        {
            Assert.That(button.TextContent.Trim(), Is.EqualTo("Done"));
            Assert.That(done, Is.EqualTo(1));
        });
    }

    [Test]
    public void TopBar_HasNoBackArrow()
    {
        var cut = Render(SampleWorkoutSummary.RecordsAndChanges);

        Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
    }

    [Test]
    public async Task AndroidBack_RaisesOnDone()
    {
        var done = 0;
        var cut = Context.Render<WorkoutSummaryView>(p => p
            .Add(x => x.Summary, SampleWorkoutSummary.RecordsAndChanges)
            .Add(x => x.OnDone, () => done++));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(done, Is.EqualTo(1));
    }

    private IRenderedComponent<WorkoutSummaryView> Render(WorkoutSummary summary) =>
        Context.Render<WorkoutSummaryView>(p => p.Add(x => x.Summary, summary).Add(x => x.OnDone, () => { }));
}
