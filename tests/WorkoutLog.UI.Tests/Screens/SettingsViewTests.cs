using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.JSInterop;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Models.Settings;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class SettingsViewTests : ComponentTestBase
{
    [Test]
    public void Title_IsSettings()
    {
        var cut = Render();

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

    [Test]
    public void Loaded_ShowsTheHeadingTextAndAnEnabledSecondaryExport()
    {
        var cut = Render(count: 42);

        var export = cut.Find("[data-testid=export]");
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".heading").TextContent, Is.EqualTo("Your data"));
            Assert.That(cut.Find("[data-testid=export-text]").TextContent, Is.EqualTo("Export all 42 workouts to a JSON file"));
            Assert.That(export.TextContent.Trim(), Is.EqualTo("Export"));
            Assert.That(export.HasAttribute("disabled"), Is.False);
            Assert.That(export.ClassList, Does.Contain("secondary").And.Contain("full"));
            Assert.That(cut.FindAll("[data-testid=export-error]"), Is.Empty);
        });
    }

    [TestCase(1, "Export 1 workout to a JSON file")]
    [TestCase(2, "Export all 2 workouts to a JSON file")]
    [TestCase(999, "Export all 999 workouts to a JSON file")]
    [TestCase(1042, "Export all 1,042 workouts to a JSON file")]
    [TestCase(1234567, "Export all 1,234,567 workouts to a JSON file")]
    public void Count_SetsTheText(int count, string text) =>
        Assert.That(Render(count: count).Find("[data-testid=export-text]").TextContent, Is.EqualTo(text));

    [Test]
    public void NoWorkouts_SaysSoAndDisablesExport()
    {
        var cut = Render(count: 0);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=export-text]").TextContent, Is.EqualTo("No workouts to export"));
            Assert.That(cut.Find("[data-testid=export]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void Loading_ShowsOnlyTheSpinner()
    {
        var cut = Render(loading: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=settings-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(".heading"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=export-text]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=export]"), Is.Empty);
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndFocusesTryAgain()
    {
        var cut = Render(failed: true);

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=settings-retry]")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=settings-error]").TextContent, Is.EqualTo("Couldn't load your workouts"));
            Assert.That(cut.FindAll("[data-testid=export]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<SettingsView>(p => p.Add(x => x.Failed, true).Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=settings-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Export_RaisesOnExport()
    {
        var exports = 0;
        var cut = Context.Render<SettingsView>(p => p.Add(x => x.Count, 3).Add(x => x.OnExport, () => exports++));

        cut.Find("[data-testid=export]").Click();

        Assert.That(exports, Is.EqualTo(1));
    }

    [Test]
    public void Exporting_ShowsExportBusyAndIgnoresTaps()
    {
        var exports = 0;
        var cut = Context.Render<SettingsView>(p => p.Add(x => x.Count, 3).Add(x => x.Exporting, true).Add(x => x.OnExport, () => exports++));

        var export = cut.Find("[data-testid=export]");
        export.Click();

        Assert.Multiple(() =>
        {
            Assert.That(export.TextContent.Trim(), Is.EqualTo("Exporting…"));
            Assert.That(export.GetAttribute("aria-busy"), Is.EqualTo("true"));
            Assert.That(exports, Is.Zero);
        });
    }

    [TestCase(ExportMessage.Failed, "Couldn't export your workouts. Try again")]
    [TestCase(ExportMessage.TooLarge, "Too many workouts to export")]
    public void Message_ShowsUnderExport(ExportMessage message, string text)
    {
        var cut = Render(count: 3, message: message);

        var error = cut.Find("[data-testid=export-error]");
        Assert.Multiple(() =>
        {
            Assert.That(error.TextContent, Is.EqualTo(text));
            Assert.That(error.GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(error.PreviousElementSibling!.GetAttribute("data-testid"), Is.EqualTo("export"));
        });
    }

    [Test]
    public void ExportFailed_FocusesExport()
    {
        var cut = Render(count: 3);
        var export = cut.Find("[data-testid=export]").GetAttribute("blazor:elementReference");

        cut.Render(p => p.Add(x => x.ExportMessage, ExportMessage.Failed));

        cut.WaitForAssertion(() => Assert.That(((ElementReference)Focuses().Single().Arguments[0]!).Id, Is.EqualTo(export)));
    }

    [Test]
    public void ExportFailed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Render(count: 3, message: ExportMessage.Failed);
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Count, 4));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void ExportFailedAgainAfterClearing_FocusesExportAgain()
    {
        var cut = Render(count: 3, message: ExportMessage.Failed);
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.ExportMessage, ExportMessage.None));
        cut.Render(p => p.Add(x => x.ExportMessage, ExportMessage.Failed));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    [Test]
    public void TooLarge_DoesNotMoveFocus()
    {
        Render(count: 3, message: ExportMessage.TooLarge);

        Assert.That(Context.JSInterop.Invocations.Where(i => i.Identifier == "Blazor._internal.domWrapper.focus"), Is.Empty);
    }

    [Test]
    public void ExportFocusFails_LogsAWarningAndKeepsTheMessage()
    {
        var logger = new FakeLogger<SettingsView>();
        Context.Services.AddSingleton<ILogger<SettingsView>>(logger);
        Context.JSInterop.SetupVoid("Blazor._internal.domWrapper.focus", _ => true).SetException(new JSException("Focus failed"));

        var cut = Render(count: 3, message: ExportMessage.Failed);

        cut.WaitForAssertion(() => Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ExportFocusFailed, LogLevel.Warning, "Focusing Export failed") })));
        Assert.Multiple(() =>
        {
            Assert.That(logger.LatestRecord.Exception, Is.TypeOf<JSException>());
            Assert.That(cut.FindAll("[data-testid=export-error]"), Has.Count.EqualTo(1));
        });
    }

    private IRenderedComponent<SettingsView> Render(
        int count = 0, bool loading = false, bool failed = false, ExportMessage message = ExportMessage.None) =>
        Context.Render<SettingsView>(p => p
            .Add(x => x.Count, count)
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.ExportMessage, message)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnExport, () => { })
            .Add(x => x.OnBack, () => { }));

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
