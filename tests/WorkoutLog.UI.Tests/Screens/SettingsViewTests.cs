using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class SettingsViewTests : ComponentTestBase
{
    [Test]
    public void Title_IsSettings()
    {
        var cut = Context.Render<SettingsView>(p => p.Add(x => x.OnBack, () => { }));

        Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Settings"));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<SettingsView>(p => p.Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }
}
