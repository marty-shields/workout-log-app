using WorkoutLog.UI.Components.Actions;

namespace WorkoutLog.UI.Tests.Components.Actions;

public class IconButtonTests : ComponentTestBase
{
    [Test]
    public void Click_RaisesOnClick()
    {
        var clicked = false;
        var cut = Context.Render<IconButton>(p => p
            .Add(x => x.Icon, IconName.Settings)
            .Add(x => x.AriaLabel, "Settings")
            .Add(x => x.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.That(clicked, Is.True);
    }

    [Test]
    public async Task FocusAsync_FocusesTheButton()
    {
        var cut = Context.Render<IconButton>(p => p.Add(x => x.Icon, IconName.Delete).Add(x => x.AriaLabel, "Delete"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("button"));
    }

    [Test]
    public void Attributes_LabelTheButtonAndHideTheIcon()
    {
        var cut = Context.Render<IconButton>(p => p
            .Add(x => x.Icon, IconName.Back)
            .Add(x => x.AriaLabel, "Back")
            .Add(x => x.TestId, "back"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button").GetAttribute("aria-label"), Is.EqualTo("Back"));
            Assert.That(cut.Find("button").GetAttribute("data-testid"), Is.EqualTo("back"));
            Assert.That(cut.Find("svg").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void IconMutedAndSize_ArePassedToTheIcon()
    {
        var cut = Context.Render<IconButton>(p => p
            .Add(x => x.Icon, IconName.Delete)
            .Add(x => x.AriaLabel, "Delete")
            .Add(x => x.Muted, true)
            .Add(x => x.Size, IconSize.Small));
        var icon = Context.Render<Icon>(p => p.Add(x => x.Name, IconName.Delete));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("path").GetAttribute("d"), Is.EqualTo(icon.Find("path").GetAttribute("d")));
            Assert.That(cut.Find("svg").ClassList, Does.Contain("muted").And.Contain("small"));
            Assert.That(cut.Find("button").ClassList, Is.EquivalentTo(new[] { "icon-btn" }));
        });
    }
}
