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
    public void Aside_IsBeforeTheChevron()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Dumbbell Curl")
            .Add(x => x.Aside, "Strength")
            .Add(x => x.OnClick, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button").Children.TakeLast(2).Select(c => c.ClassName), Is.EqualTo(new[] { "aside", "chevron" }));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("Strength"));
        });
    }

    [Test]
    public void Aside_WithoutOnClick_IsAtTheEnd()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Plank").Add(x => x.Aside, "Strength"));

        Assert.That(cut.Find("div.row").LastElementChild!.ClassName, Is.EqualTo("aside"));
    }

    [Test]
    public void NoAside_RendersNone() =>
        Assert.That(Context.Render<ListRow>(p => p.Add(x => x.Text, "Workouts")).FindAll(".aside"), Is.Empty);

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

    [Test]
    public void WithoutDetail_HasOneLine()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Workouts").Add(x => x.OnClick, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".detail"), Is.Empty);
            Assert.That(cut.Find(".row").ClassList, Does.Not.Contain("two-line"));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Detail_ShowsUnderTheText(bool tappable)
    {
        var cut = Context.Render<ListRow>(p =>
        {
            p.Add(x => x.Text, "Barbell Back Squat").Add(x => x.Detail, "Last Tue 20 Oct · 70kg × 3");
            if (tappable)
            {
                p.Add(x => x.OnClick, () => { });
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".texts > span").Select(s => s.TextContent), Is.EqualTo(new[] { "Barbell Back Squat", "Last Tue 20 Oct · 70kg × 3" }));
            Assert.That(cut.Find(".row").ClassList, Does.Contain("two-line"));
        });
    }
}
