using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class ListRowTests : ComponentTestBase
{
    [Test]
    public void WithOnClick_RendersButtonThatRaisesIt()
    {
        var clicked = false;
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Workouts")
            .Add(x => x.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.That(clicked, Is.True);
    }

    [Test]
    public void WithOnClick_EndsInAMutedChevron()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Workouts")
            .Add(x => x.OnClick, () => { }));

        var chevron = cut.FindComponent<Icon>().Instance;

        Assert.That((chevron.Name, chevron.Muted, chevron.Size), Is.EqualTo((IconName.Chevron, true, IconSize.Default)));
    }

    [Test]
    public void WithoutOnClick_RendersStaticRow()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Tue 20 Oct 2026 · 18:05"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button"), Is.Empty);
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Tue 20 Oct 2026 · 18:05"));
        });
    }
}
