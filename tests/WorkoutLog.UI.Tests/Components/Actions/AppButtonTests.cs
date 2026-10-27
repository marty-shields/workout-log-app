using WorkoutLog.UI.Components.Actions;

namespace WorkoutLog.UI.Tests.Components.Actions;

public class AppButtonTests : ComponentTestBase
{
    [Test]
    public void Click_RaisesOnClick()
    {
        var clicked = false;
        var cut = Context.Render<AppButton>(p => p
            .Add(x => x.OnClick, () => clicked = true)
            .AddChildContent("Save"));

        cut.Find("button").Click();

        Assert.That(clicked, Is.True);
    }

    [Test]
    public async Task FocusAsync_FocusesTheButton()
    {
        var cut = Context.Render<AppButton>(p => p.AddChildContent("Save"));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync());

        Context.JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeElementReferenceTo(cut.Find("button"));
    }

    [Test]
    public void Disabled_SetsDisabledAttribute()
    {
        var cut = Context.Render<AppButton>(p => p.Add(x => x.Disabled, true));

        Assert.That(cut.Find("button").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public void Busy_ShowsTheRingBesideTheTextAndSetsAriaBusy()
    {
        var cut = Context.Render<AppButton>(p => p.Add(x => x.Busy, true).AddChildContent("Exporting…"));

        var button = cut.Find("button");
        Assert.Multiple(() =>
        {
            Assert.That(button.GetAttribute("aria-busy"), Is.EqualTo("true"));
            Assert.That(button.QuerySelectorAll(".ring"), Has.Length.EqualTo(1));
            Assert.That(button.TextContent.Trim(), Is.EqualTo("Exporting…"));
            Assert.That(button.HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void NotBusy_HasNoRingOrAriaBusy()
    {
        var cut = Context.Render<AppButton>(p => p.AddChildContent("Export"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button").HasAttribute("aria-busy"), Is.False);
            Assert.That(cut.FindAll(".ring"), Is.Empty);
        });
    }

    [Test]
    public void Busy_ClickIsIgnored()
    {
        var clicks = 0;
        var cut = Context.Render<AppButton>(p => p
            .Add(x => x.Busy, true)
            .Add(x => x.OnClick, () => clicks++)
            .AddChildContent("Exporting…"));

        cut.Find("button").Click();

        Assert.That(clicks, Is.Zero);
    }

    [TestCase(ButtonVariant.Primary, "primary")]
    [TestCase(ButtonVariant.Secondary, "secondary")]
    [TestCase(ButtonVariant.Danger, "danger")]
    public void Variant_SetsClass(ButtonVariant variant, string cssClass)
    {
        var cut = Context.Render<AppButton>(p => p.Add(x => x.Variant, variant));

        Assert.That(cut.Find("button").ClassList, Does.Contain(cssClass));
    }
}
