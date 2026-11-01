using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>Sets per muscle group: one row per group with its sets in the last 7 days.</summary>
public sealed class MusclesScreen(WebView web)
{
    public const string Title = "Sets per muscle group";

    /// <summary>Waits until the groups or the load error show.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("muscles-chest") || web.Exists("muscles-error"), "Sets per muscle group didn't load");

    /// <summary>Each row's group and sets, in order, once loaded.</summary>
    public IReadOnlyList<(string Group, string Sets)> Rows()
    {
        WaitUntilLoaded();
        return web.FindAll("li[data-testid^=\"muscles-\"]")
            .Select(r => r.FindElements(By.CssSelector("span")).Select(s => s.Text.Trim()).ToList())
            .Select(s => (s[0], s[1]))
            .ToList();
    }

    public void Back() => web.Tap("back");
}
