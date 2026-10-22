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
}
