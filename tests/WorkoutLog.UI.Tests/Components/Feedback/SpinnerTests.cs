using WorkoutLog.UI.Components.Feedback;

namespace WorkoutLog.UI.Tests.Components.Feedback;

public class SpinnerTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsLabelAsStatus()
    {
        var cut = Context.Render<Spinner>(p => p
            .Add(x => x.Label, "Loading…")
            .Add(x => x.TestId, "startup-loading"));

        var spinner = cut.Find("[data-testid=startup-loading]");
        Assert.Multiple(() =>
        {
            Assert.That(spinner.GetAttribute("role"), Is.EqualTo("status"));
            Assert.That(spinner.TextContent.Trim(), Is.EqualTo("Loading…"));
        });
    }
}
