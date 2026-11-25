namespace WorkoutLog.UI.Tests.Lifecycle;

/// <summary>
/// <c>MainActivity</c> passes its create, start and destroy to the kit's <c>BackButtonBridge</c> and <c>AppLifecycleBridge</c>, which <c>HostBridges</c> connects to the BlazorWebView's scoped instances; the App also wires <c>AndroidKeyboardRestore</c> and <c>AndroidDiagnostics</c>.
/// Keyboard restore follows only <c>MainActivity</c>, not MAUI's own activities, except that destroy is passed on and each class ignores any activity but its own.
/// The App can't be referenced, so its files are read. Whitespace is ignored.
/// </summary>
public class ActivityWiringTests
{
    [TestCase("MauiProgram.cs", "AddDiagnostics(builder);")]
    [TestCase("MauiProgram.cs", ".OnStart(_ => Diagnostics().ActivityStarted())")]
    [TestCase("MauiProgram.cs", "AddKeyboardRestore(builder);")]
    [TestCase("MauiProgram.cs", "builder.Services.AddSingleton<AndroidKeyboardRestore>();")]
    [TestCase("MauiProgram.cs", ".OnPause(activity => { if (activity is MainActivity) { KeyboardRestore().Paused(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnCreate((activity, _) => { if (activity is MainActivity) { KeyboardRestore().Attach(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnResume(activity => { if (activity is MainActivity) { KeyboardRestore().Resumed(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => KeyboardRestore().Detach(activity))")]
    [TestCase("MauiProgram.cs", "AddUnfinishedFlush(builder);")]
    [TestCase("MauiProgram.cs", ".OnPause(activity => { if (activity is MainActivity) { Flush(); } })")]
    [TestCase("MauiProgram.cs", ".OnStop(activity => { if (activity is MainActivity) { Flush(); } })")]
    [TestCase("MauiProgram.cs", "static void Flush() => _ = IPlatformApplication.Current!.Services.GetRequiredService<UnfinishedAutosaver>().FlushAsync();")]
    [TestCase("Platforms/Android/MainActivity.cs", "backBridge = services.GetRequiredService<BackButtonBridge>();")]
    [TestCase("Platforms/Android/MainActivity.cs", "lifecycleBridge = services.GetRequiredService<AppLifecycleBridge>();")]
    [TestCase("Platforms/Android/MainActivity.cs", "backBridge.Attach(this);")]
    [TestCase("Platforms/Android/MainActivity.cs", "lifecycleBridge.Attach(this);")]
    [TestCase("Platforms/Android/MainActivity.cs", "backBridge.EnabledChanged += SyncBackCallback;")]
    [TestCase("Platforms/Android/MainActivity.cs", "_ = lifecycleBridge.StartedAsync();")]
    [TestCase("Platforms/Android/MainActivity.cs", "backBridge.EnabledChanged -= SyncBackCallback;")]
    [TestCase("Platforms/Android/MainActivity.cs", "backBridge.Detach(this);")]
    [TestCase("Platforms/Android/MainActivity.cs", "lifecycleBridge.Detach(this);")]
    [TestCase("Platforms/Android/MainActivity.cs", "backCallback.Enabled = backBridge.Enabled")]
    [TestCase("Platforms/Android/MainActivity.cs", "OnBackPressedDispatcher.AddCallback(this, backCallback);")]
    [TestCase("Platforms/Android/MainActivity.cs", "if (bridge.HandleAsync() is null) { Enabled = false; activity.OnBackPressedDispatcher.OnBackPressed(); }")]
    [TestCase("Components/Routes.razor", "<HostBridges />")]
    [TestCase("Components/Routes.razor", "<StartupGateInitialize=\"InitializeAsync\"")]
    [TestCase("Components/Routes.razor", "<AppErrorBoundaryTitle=\"WorkoutLog\"OnRestart=\"()=>Navigation.GoTo(string.Empty)\">")]
    [TestCase("Components/Routes.razor", "<Router")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "public void ActivityStarted() => Log.ActivityStarted(logger);")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "[LoggerMessage((int)AppEvent.ActivityStarted, LogLevel.Information, \"Activity started\")]")]
    public void App_PassesTheActivityLifecycle(string file, string code) =>
        Assert.That(WithoutWhitespace(RepoFiles.Read(["src", "WorkoutLog.App", .. file.Split('/')])), Does.Contain(WithoutWhitespace(code)));

    [Test]
    public void App_HasNoBridgeClassesOfItsOwn() =>
        Assert.That(
            RepoFiles.Find("*.cs", "src", "WorkoutLog.App").Select(Path.GetFileNameWithoutExtension),
            Does.Not.Contain("ActivityBridge").And.Not.Contain("BackButtonLink").And.Not.Contain("AndroidAppLifecycle").And.Not.Contain("AndroidBackButton"));

    [Test]
    public void E2E_FindsTheStartupErrorByTheKitsFailedStateId() =>
        Assert.That(RepoFiles.Read("tests", "WorkoutLog.E2E.Tests", "Screens", "ThisWeekScreen.cs"), Does.Contain("web.Exists(\"startup-failed\")"));

    [Test]
    public void Routes_PutsHostBridgesBeforeTheStartupGate() =>
        Assert.That(PositionsInRoutes("<HostBridges />", "<StartupGate"), Is.Ordered.And.Unique.And.All.GreaterThanOrEqualTo(0));

    [Test]
    public void Routes_WrapsTheRouterInTheErrorBoundaryInTheStartupGate() =>
        Assert.That(
            PositionsInRoutes("<StartupGate", "<AppErrorBoundary", "<Router", "</Router>", "</AppErrorBoundary>", "</StartupGate>"),
            Is.Ordered.And.Unique.And.All.GreaterThanOrEqualTo(0));

    private static int[] PositionsInRoutes(params string[] tags)
    {
        var routes = WithoutWhitespace(RepoFiles.Read("src", "WorkoutLog.App", "Components", "Routes.razor"));
        return [.. tags.Select(tag => routes.IndexOf(WithoutWhitespace(tag), StringComparison.Ordinal))];
    }

    private static string WithoutWhitespace(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
}
