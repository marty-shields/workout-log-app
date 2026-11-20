using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Settings: a back arrow and title, the theme, the export text and Export, and Import with its messages.</summary>
public sealed class SettingsScreen(WebView web)
{
    public const string Title = "Settings";

    /// <summary>The export text, once the count has loaded.</summary>
    public string ExportText()
    {
        web.WaitUntil(() => web.Exists("export-text"), "Settings didn't load");
        return web.Text("export-text");
    }

    /// <summary>Taps a theme in Appearance (system, light or dark) and waits until it is selected.</summary>
    public void ChooseTheme(string theme)
    {
        var radio = $"settings-theme-{theme}";
        web.Find(radio).FindElement(By.XPath("..")).Click();
        web.Eventually(() => web.Find(radio).Selected, Is.True, $"{theme} theme selected");
    }

    /// <summary>Stores a theme under the key an earlier version wrote, as that version wrote it: the bare word.</summary>
    public void StoreThemeAsEarlierVersion(string theme) =>
        web.Run($"localStorage.setItem('workoutlog.theme', '{theme}');");

    /// <summary>The theme the page applies: the root element's <c>data-theme</c>, empty when it follows the phone.</summary>
    public string AppliedTheme() =>
        web.Run("return document.documentElement.getAttribute('data-theme') ?? '';") as string ?? "";

    /// <summary>Whether the theme's radio in Appearance is selected.</summary>
    public bool ThemeSelected(string theme) => web.Find($"settings-theme-{theme}").Selected;

    /// <summary>The version line at the bottom, once Settings has opened.</summary>
    public string Version()
    {
        web.WaitUntil(() => web.Exists("app-version"), "Settings didn't show the version");
        return web.Text("app-version");
    }

    public bool ExportEnabled() => web.Find("export").Enabled;

    public void Export() => web.Tap("export");

    public void Import() => web.Tap("import");

    public string ImportText() => web.Text("import");

    /// <summary>The message under Import: done or failed; null when there is none.</summary>
    public string? ImportMessage() =>
        web.Exists("import-done") ? web.Text("import-done") : web.Exists("import-error") ? web.Text("import-error") : null;
}
