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

    [TestCase(ButtonVariant.Primary, "primary")]
    [TestCase(ButtonVariant.Secondary, "secondary")]
    [TestCase(ButtonVariant.Danger, "danger")]
    public void Variant_SetsClass(ButtonVariant variant, string cssClass)
    {
        var cut = Context.Render<AppButton>(p => p.Add(x => x.Variant, variant));

        Assert.That(cut.Find("button").ClassList, Does.Contain(cssClass));
    }
}
