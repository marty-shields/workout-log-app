using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Workout;

namespace WorkoutLog.UI.Tests.Components.Workout;

public class RecordBadgeTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheRecordIconAndText()
    {
        var cut = Context.Render<RecordBadge>(p => p.Add(x => x.TestId, "badge"));

        var icon = cut.FindComponent<Icon>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(cut.Find("[data-testid=badge]").TextContent), Is.EqualTo("Record"));
            Assert.That((icon.Name, icon.Tone, icon.Size), Is.EqualTo((IconName.Record, IconTone.Success, IconSize.Small)));
        });
    }

    [Test]
    public void Render_IsNotInteractive()
    {
        var cut = Context.Render<RecordBadge>();

        Assert.That(cut.FindAll("button, a, input"), Is.Empty);
    }
}
