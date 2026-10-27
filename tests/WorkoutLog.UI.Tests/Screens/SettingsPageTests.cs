using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Logging;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Sharing;

namespace WorkoutLog.UI.Tests.Screens;

public class SettingsPageTests : ComponentTestBase
{
    private const string FilePath = "/cache/workout-log-2026-10-26-1805.json";
    private static readonly ExportResult Exported = new(ExportStatus.Exported, FilePath);

    private FakeExportService export = null!;
    private FakeFileShare share = null!;
    private FakeLogger<SettingsPage> logger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        export = new FakeExportService();
        Context.Services.AddSingleton<IExportService>(export);
        share = new FakeFileShare();
        Context.Services.AddSingleton<IFileShare>(share);
        logger = new FakeLogger<SettingsPage>();
        Context.Services.AddSingleton<ILogger<SettingsPage>>(logger);
        lifecycle = new AppLifecycle(new FakeLogger<AppLifecycle>());
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<SettingsPage>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }

    [Test]
    public async Task AndroidBack_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<SettingsPage>(p => p.Add(x => x.OnBack, () => backs++));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(backs, Is.EqualTo(1));
    }

    [Test]
    public void WhileLoading_ShowsOnlyTheSpinner()
    {
        var cut = Context.Render<SettingsPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=settings-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=export]"), Is.Empty);
            Assert.That(export.CountCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheCount()
    {
        var cut = await LoadedAsync(42);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=export-text]").TextContent, Is.EqualTo("Export all 42 workouts to a JSON file"));
            Assert.That(cut.FindAll("[data-testid=settings-loading]"), Is.Empty);
        });
    }

    [Test]
    public async Task NoWorkouts_DisablesExport()
    {
        var cut = await LoadedAsync(0);

        Assert.That(cut.Find("[data-testid=export]").HasAttribute("disabled"), Is.True);
    }

    [Test]
    public async Task LoadFails_ShowsTheErrorAndTryAgainReloads()
    {
        var cut = Context.Render<SettingsPage>();
        export.CompleteCount(new ExportCount(0, Failed: true));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=settings-error]").TextContent, Is.EqualTo("Couldn't load your workouts")));

        cut.Find("[data-testid=settings-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(export.CountCalls, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=settings-loading]"), Has.Count.EqualTo(1));
        });
        export.CompleteCount(new ExportCount(3, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export-text]").TextContent, Is.EqualTo("Export all 3 workouts to a JSON file")));
    }

    [Test]
    public async Task AppStarted_ReloadsTheCount()
    {
        var cut = await LoadedAsync(3);

        var started = lifecycle.NotifyStartedAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=settings-loading]"), Has.Count.EqualTo(1)));
        export.CompleteCount(new ExportCount(4, Failed: false));
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export-text]").TextContent, Is.EqualTo("Export all 4 workouts to a JSON file")));
        Assert.That(export.CountCalls, Is.EqualTo(2));
    }

    [Test]
    public async Task AppStarted_WhileLoading_IsIgnored()
    {
        Context.Render<SettingsPage>();

        await lifecycle.NotifyStartedAsync();

        Assert.That(export.CountCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task AppStarted_WhileExporting_IsIgnored()
    {
        var cut = await LoadedAsync(3);
        cut.Find("[data-testid=export]").Click();

        await lifecycle.NotifyStartedAsync();

        Assert.That(export.CountCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        await LoadedAsync(3);

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(export.CountCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task Export_ShowsBusyThenSharesTheFileWithTheTitle()
    {
        var cut = await LoadedAsync(3);

        cut.Find("[data-testid=export]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=export]").TextContent.Trim(), Is.EqualTo("Exporting…"));
            Assert.That(export.ExportCalls, Is.EqualTo(1));
        });
        export.CompleteExport(Exported);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(share.Calls, Is.EqualTo(new[] { (FilePath, "Export workouts") })));
        Assert.That(cut.Find("[data-testid=export]").TextContent.Trim(), Is.EqualTo("Exporting…"));
        share.Complete();
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export]").TextContent.Trim(), Is.EqualTo("Export")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=export-error]"), Is.Empty);
            Assert.That(logger.Collector.Count, Is.Zero);
        });
    }

    [Test]
    public async Task Export_TapsWhileExporting_AreIgnored()
    {
        var cut = await LoadedAsync(3);

        cut.Find("[data-testid=export]").Click();
        cut.Find("[data-testid=export]").Click();
        export.CompleteExport(Exported);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(share.Calls, Has.Count.EqualTo(1)));
        cut.Find("[data-testid=export]").Click();

        Assert.That(export.ExportCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task Export_Fails_ShowsTheFailedMessageAndDoesNotShare()
    {
        var cut = await LoadedAsync(3);
        var button = cut.Find("[data-testid=export]").GetAttribute("blazor:elementReference");

        cut.Find("[data-testid=export]").Click();
        export.CompleteExport(new ExportResult(ExportStatus.Failed, null));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export-error]").TextContent, Is.EqualTo("Couldn't export your workouts. Try again")));
        Assert.Multiple(() =>
        {
            Assert.That(share.Calls, Is.Empty);
            Assert.That(cut.Find("[data-testid=export]").TextContent.Trim(), Is.EqualTo("Export"));
        });
        cut.WaitForAssertion(() => Assert.That(
            ((ElementReference)Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"].Single().Arguments[0]!).Id, Is.EqualTo(button)));
    }

    [Test]
    public async Task Export_TooLarge_ShowsTooManyWorkoutsAndDoesNotShare()
    {
        var cut = await LoadedAsync(3);

        cut.Find("[data-testid=export]").Click();
        export.CompleteExport(new ExportResult(ExportStatus.TooLarge, null));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export-error]").TextContent, Is.EqualTo("Too many workouts to export")));
        Assert.That(share.Calls, Is.Empty);
    }

    [Test]
    public async Task Share_Throws_ShowsTheFailedMessageAndLogsTheError()
    {
        var cut = await LoadedAsync(3);
        var exception = new InvalidOperationException("No activity");

        cut.Find("[data-testid=export]").Click();
        export.CompleteExport(Exported);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(share.Calls, Has.Count.EqualTo(1)));
        share.Fail(exception);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=export-error]").TextContent, Is.EqualTo("Couldn't export your workouts. Try again")));
        Assert.Multiple(() =>
        {
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (UiEvent.ShareFailed, LogLevel.Error, "Opening the share sheet failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    [Test]
    public async Task Message_ClearsOnTheNextExportTap()
    {
        var cut = await ExportFailedAsync();

        cut.Find("[data-testid=export]").Click();

        Assert.That(cut.FindAll("[data-testid=export-error]"), Is.Empty);
    }

    [Test]
    public async Task Message_ClearsOnAppStartReload()
    {
        var cut = await ExportFailedAsync();

        var started = lifecycle.NotifyStartedAsync();
        export.CompleteCount(new ExportCount(3, Failed: false));
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=export]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=export-error]"), Is.Empty);
    }

    [Test]
    public async Task Disposed_WhileExporting_DoesNotShare()
    {
        var cut = await LoadedAsync(3);
        cut.Find("[data-testid=export]").Click();

        await Context.DisposeComponentsAsync();
        export.CompleteExport(Exported);
        await WaitForRendererAsync();

        Assert.That(share.Calls, Is.Empty);
    }

    private async Task<IRenderedComponent<SettingsPage>> LoadedAsync(int count)
    {
        var cut = Context.Render<SettingsPage>(p => p.Add(x => x.OnBack, () => { }));
        export.CompleteCount(new ExportCount(count, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=export]"), Has.Count.EqualTo(1)));
        return cut;
    }

    private async Task<IRenderedComponent<SettingsPage>> ExportFailedAsync()
    {
        var cut = await LoadedAsync(3);
        cut.Find("[data-testid=export]").Click();
        export.CompleteExport(new ExportResult(ExportStatus.Failed, null));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=export-error]"), Has.Count.EqualTo(1)));
        return cut;
    }

    /// <summary>Each call waits until the test completes it.</summary>
    private sealed class FakeExportService : IExportService
    {
        private TaskCompletionSource<ExportCount> count = new();
        private TaskCompletionSource<ExportResult> export = new();

        public int CountCalls { get; private set; }

        public int ExportCalls { get; private set; }

        public Task<ExportCount> CountAsync()
        {
            CountCalls++;
            count = new TaskCompletionSource<ExportCount>();
            return count.Task;
        }

        public Task<ExportResult> ExportAsync()
        {
            ExportCalls++;
            export = new TaskCompletionSource<ExportResult>();
            return export.Task;
        }

        /// <summary>Completes the latest count.</summary>
        public void CompleteCount(ExportCount result) => count.SetResult(result);

        /// <summary>Completes the latest export.</summary>
        public void CompleteExport(ExportResult result) => export.SetResult(result);
    }

    /// <summary>Records each share; each waits until the test completes or fails it.</summary>
    private sealed class FakeFileShare : IFileShare
    {
        private TaskCompletionSource pending = new();

        public List<(string FilePath, string Title)> Calls { get; } = [];

        public Task ShareAsync(string filePath, string title)
        {
            Calls.Add((filePath, title));
            pending = new TaskCompletionSource();
            return pending.Task;
        }

        public void Complete() => pending.SetResult();

        public void Fail(Exception exception) => pending.SetException(exception);
    }
}
