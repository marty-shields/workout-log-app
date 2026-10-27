namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Settings: a back arrow and title, the export text and Export, and Import with its messages.</summary>
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

    public void Import() => web.Tap("import");

    public string ImportText() => web.Text("import");

    /// <summary>The message under Import: done or failed; null when there is none.</summary>
    public string? ImportMessage() =>
        web.Exists("import-done") ? web.Text("import-done") : web.Exists("import-error") ? web.Text("import-error") : null;
}
