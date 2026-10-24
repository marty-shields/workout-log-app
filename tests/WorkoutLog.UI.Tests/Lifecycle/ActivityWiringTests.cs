namespace WorkoutLog.UI.Tests.Lifecycle;

/// <summary>
/// The App passes the activity's lifecycle to <c>IBackButton</c>, <c>IAppLifecycle</c> and <c>AndroidDiagnostics</c> (T46, T72, T74).
/// The App can't be referenced, so its files are read. Whitespace is ignored.
/// </summary>
public class ActivityWiringTests
{
    [TestCase("MauiProgram.cs", "AddBackButton(builder);")]
    [TestCase("MauiProgram.cs", ".OnCreate((activity, _) => BackButton().Attach((AndroidX.Activity.ComponentActivity)activity))")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => BackButton().Detach(activity))")]
    [TestCase("MauiProgram.cs", "AddAppLifecycle(builder);")]
    [TestCase("MauiProgram.cs", ".OnCreate((activity, _) => Lifecycle().Attach(activity))")]
    [TestCase("MauiProgram.cs", ".OnStart(activity => _ = Lifecycle().NotifyStartedAsync())")]
    [TestCase("MauiProgram.cs", ".OnDestroy(activity => Lifecycle().Detach(activity))")]
    [TestCase("MauiProgram.cs", "AddDiagnostics(builder);")]
    [TestCase("MauiProgram.cs", ".OnStart(_ => Diagnostics().ActivityStarted())")]
    [TestCase("Navigation/AndroidBackButton.cs", "backButton.Attach(activity);")]
    [TestCase("Navigation/AndroidBackButton.cs", "backButton.Detach(activity);")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "public void ActivityStarted() => Log.ActivityStarted(logger);")]
    [TestCase("Diagnostics/AndroidDiagnostics.cs", "[LoggerMessage((int)AppEvent.ActivityStarted, LogLevel.Information, \"Activity started\")]")]
    public void App_PassesTheActivityLifecycle(string file, string code) =>
        Assert.That(WithoutWhitespace(RepoFiles.Read(["src", "WorkoutLog.App", .. file.Split('/')])), Does.Contain(WithoutWhitespace(code)));

    private static string WithoutWhitespace(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
}
