using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class StatTileTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsValueAndLabel()
    {
        var cut = Context.Render<StatTile>(p => p
            .Add(x => x.Label, "This week")
            .Add(x => x.Value, 3));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".value").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find(".label").TextContent, Is.EqualTo("This week"));
        });
    }
}
