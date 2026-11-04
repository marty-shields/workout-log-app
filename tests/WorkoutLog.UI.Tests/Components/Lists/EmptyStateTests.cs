using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class EmptyStateTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsMessageAndContent()
    {
        var cut = Context.Render<EmptyState>(p => p
            .Add(x => x.Message, "No workouts yet")
            .AddChildContent("<button>New Workout</button>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".message").TextContent, Is.EqualTo("No workouts yet"));
            Assert.That(cut.Find("button").TextContent, Is.EqualTo("New Workout"));
        });
    }

    [Test]
    public void Render_WithHint_ShowsHintUnderMessage()
    {
        var cut = Context.Render<EmptyState>(p => p
            .Add(x => x.Message, "No workouts yet")
            .Add(x => x.Hint, "Tap New Workout to log one")
            .Add(x => x.TestId, "empty"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=empty-message]").TextContent, Is.EqualTo("No workouts yet"));
            Assert.That(cut.Find("[data-testid=empty-hint]").TextContent, Is.EqualTo("Tap New Workout to log one"));
        });
    }

    [Test]
    public void Render_WithoutHint_ShowsNoHint()
    {
        var cut = Context.Render<EmptyState>(p => p
            .Add(x => x.Message, "No workouts yet")
            .Add(x => x.TestId, "empty"));

        Assert.That(cut.FindAll("[data-testid=empty-hint]"), Is.Empty);
    }
}
