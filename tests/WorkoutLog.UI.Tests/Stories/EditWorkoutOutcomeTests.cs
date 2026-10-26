using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.UI.Stories.Samples;
using WorkoutLog.UI.Stories.Shared;

namespace WorkoutLog.UI.Tests.Stories;

public class EditWorkoutOutcomeTests : ComponentTestBase
{
    [SetUp]
    public void SetUp() => Context.Services.AddSingleton<TimeProvider>(new FakeTimeProvider());

    [Test]
    public void Back_NoChanges_ShowsHistory()
    {
        var cut = Context.Render<EditWorkoutOutcome>();

        cut.Find("[data-testid=back]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("History"));
            Assert.That(cut.FindAll("[data-testid^=history-row-]"), Has.Count.EqualTo(SampleData.HistoryRows.Count));
        });
    }

    [Test]
    public void Discard_ShowsHistoryAndOpeningARowShowsTheSavedWorkout()
    {
        var cut = Context.Render<EditWorkoutOutcome>();
        cut.Find("[data-testid=exercise-4-set-1-reps]").Input("9");
        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        var title = cut.Find("[data-testid=screen-title]").TextContent;

        cut.Find($"[data-testid=history-row-{SampleData.HistoryRows[0].Id}]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("History"));
            Assert.That(cut.Find("[data-testid=exercise-4-set-1-reps]").GetAttribute("value"), Is.EqualTo("8"));
        });
    }

    [Test]
    public void Save_ShowsTheDashboardAndItsHistoryLinkShowsHistory()
    {
        var cut = Context.Render<EditWorkoutOutcome>();

        cut.Find("[data-testid=save-workout]").Click();
        var dashboard = cut.Find("[data-testid=screen-title]").TextContent;
        cut.Find("[data-testid=history-link]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(dashboard, Is.EqualTo("Workout Log"));
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("History"));
        });
    }

    [Test]
    public void Delete_ShowsHistoryWithoutTheRowAndItStaysGone()
    {
        var cut = Context.Render<EditWorkoutOutcome>();
        cut.Find("[data-testid=delete-workout]").Click();
        cut.Find("[data-testid=confirm-dialog-confirm]").Click();
        var title = cut.Find("[data-testid=screen-title]").TextContent;
        var rows = cut.FindAll("[data-testid^=history-row-]").Count;

        cut.Find("[data-testid=back]").Click();
        cut.Find("[data-testid=history-link]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(title, Is.EqualTo("History"));
            Assert.That(rows, Is.EqualTo(SampleData.HistoryRows.Count - 1));
            Assert.That(cut.FindAll("[data-testid=history-row-1]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=history-row-]"), Has.Count.EqualTo(SampleData.HistoryRows.Count - 1));
        });
    }

    [Test]
    public void BackOnHistory_ShowsTheDashboard()
    {
        var cut = Context.Render<EditWorkoutOutcome>();
        cut.Find("[data-testid=back]").Click();

        cut.Find("[data-testid=back]").Click();

        Assert.That(cut.FindAll("[data-testid=history-link]"), Has.Count.EqualTo(1));
    }
}
