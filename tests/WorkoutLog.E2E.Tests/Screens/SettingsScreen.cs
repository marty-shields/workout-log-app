namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Settings: a back arrow and title, and the export text and Export.</summary>
public sealed class SettingsScreen(WebView web)
{
    public const string Title = "Settings";

    /// <summary>The export text, once the count has loaded.</summary>
    public string ExportText()
    {
        web.WaitUntil(() => web.Exists("export-text"), "Settings didn't load");
        return web.Text("export-text");
    }

    public bool ExportEnabled() => web.Find("export").Enabled;

    public void Export() => web.Tap("export");
}
