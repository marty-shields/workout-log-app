namespace WorkoutLog.UI.Tests.Lifecycle;

/// <summary>
/// The App passes the activity's lifecycle to the kit's <c>IBackButton</c> and <c>IAppLifecycle</c> (through <c>AndroidBackButton</c> and <c>AndroidAppLifecycle</c>, which <c>ActivityBridge</c> connects to the BlazorWebView's scoped instances), <c>AndroidKeyboardRestore</c> and <c>AndroidDiagnostics</c>;
/// the back button, app start and keyboard restore follow only <c>MainActivity</c>, not MAUI's own activities, except that destroy is passed on and each class ignores any activity but its own.
/// The App can't be referenced, so its files are read. Whitespace is ignored.
/// </summary>
public class ActivityWiringTests
{
    [TestCase("MauiProgram.cs", "AddBackButton(builder);")]
    [TestCase("MauiProgram.cs", "if (activity is MainActivity main) { BackButton().Attach(main); }")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => BackButton().Detach(activity))")]
    [TestCase("MauiProgram.cs", "AddAppLifecycle(builder);")]
    [TestCase("MauiProgram.cs", "if (activity is MainActivity) { Lifecycle().Attach(activity); }")]
    [TestCase("MauiProgram.cs", "if (activity is MainActivity) { Lifecycle().NotifyStarted(); }")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => Lifecycle().Detach(activity))")]
    [TestCase("MauiProgram.cs", "AddDiagnostics(builder);")]
    [TestCase("MauiProgram.cs", ".OnStart(_ => Diagnostics().ActivityStarted())")]
    [TestCase("MauiProgram.cs", "AddKeyboardRestore(builder);")]
    [TestCase("MauiProgram.cs", "builder.Services.AddSingleton<AndroidKeyboardRestore>();")]
    [TestCase("MauiProgram.cs", ".OnPause(activity => { if (activity is MainActivity) { KeyboardRestore().Paused(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnCreate((activity, _) => { if (activity is MainActivity) { KeyboardRestore().Attach(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnResume(activity => { if (activity is MainActivity) { KeyboardRestore().Resumed(activity); } })")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => KeyboardRestore().Detach(activity))")]
    [TestCase("MauiProgram.cs", "builder.Services.AddSingleton<AndroidAppLifecycle>();")]
    [TestCase("Navigation/BackButtonLink.cs", "backButton?.Attach(activity);")]
    [TestCase("Navigation/BackButtonLink.cs", "scoped.Attach(current);")]
    [TestCase("Navigation/BackButtonLink.cs", "backButton.Changed -= update;")]
    [TestCase("Navigation/BackButtonLink.cs", "backButton?.Detach(activity);")]
    [TestCase("Lifecycle/AndroidAppLifecycle.cs", "lifecycle?.Attach(activity);")]
    [TestCase("Lifecycle/AndroidAppLifecycle.cs", "scoped.Attach(current);")]
    [TestCase("Lifecycle/AndroidAppLifecycle.cs", "lifecycle?.Detach(activity);")]
    [TestCase("Components/ActivityBridge.cs", "AndroidBackButton.Connect(BackButton);")]
    [TestCase("Components/ActivityBridge.cs", "AndroidLifecycle.Connect(Lifecycle);")]
    [TestCase("Components/ActivityBridge.cs", "AndroidBackButton.Disconnect(BackButton);")]
    [TestCase("Components/ActivityBridge.cs", "AndroidLifecycle.Disconnect(Lifecycle);")]
    [TestCase("Components/Routes.razor", "<ActivityBridge />")]
    [TestCase("Components/Routes.razor", "<StartupGateInitialize=\"InitializeAsync\"")]
    [TestCase("Components/Routes.razor", "<AppErrorBoundaryTitle=\"WorkoutLog\"OnRestart=\"()=>Navigation.GoTo(string.Empty)\">")]
    [TestCase("Components/Routes.razor", "<Router")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "public void ActivityStarted() => Log.ActivityStarted(logger);")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "[LoggerMessage((int)AppEvent.ActivityStarted, LogLevel.Information, \"Activity started\")]")]
    public void App_PassesTheActivityLifecycle(string file, string code) =>
        Assert.That(WithoutWhitespace(RepoFiles.Read(["src", "WorkoutLog.App", .. file.Split('/')])), Does.Contain(WithoutWhitespace(code)));

    [Test]
    public void Routes_PutsTheActivityBridgeBeforeTheStartupGate() =>
        Assert.That(PositionsInRoutes("<ActivityBridge />", "<StartupGate"), Is.Ordered.And.Unique.And.All.GreaterThanOrEqualTo(0));

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
