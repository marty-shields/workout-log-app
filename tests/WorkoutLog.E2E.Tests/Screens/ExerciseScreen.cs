using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>An Exercise page: a lift's or bodyweight move's chart, records and sessions.</summary>
public sealed class ExerciseScreen(WebView web)
{
    /// <summary>True once the page shows its content, spinner or load error.</summary>
    public bool IsShown() => web.Exists("exercise-chart-card") || web.Exists("exercise-loading") || web.Exists("exercise-error");

    /// <summary>Waits until the content or the load error shows.</summary>
    public void WaitUntilLoaded() =>
        web.WaitUntil(() => web.Exists("exercise-chart-card") || web.Exists("exercise-error"), "the Exercise page didn't load");

    public string Title() => web.Text("screen-title");

    public string Subtitle() => web.Text("screen-subtitle");

    /// <summary>Each chart point as its text alternative, e.g. "20 Oct: 70kg ×3", oldest first.</summary>
    public IReadOnlyList<string> ChartValues()
    {
        WaitUntilLoaded();
        return web.FindAll("[data-testid=\"exercise-chart-values\"] li").Select(e => e.GetDomProperty("textContent")!.Trim()).ToList();
    }

    /// <summary>The message in place of the chart; null while the chart shows.</summary>
    public string? NoDataText() => web.Exists("exercise-chart-empty") ? web.Text("exercise-chart-empty") : null;

    public string? Caption() => web.Exists("exercise-chart-caption") ? web.Text("exercise-chart-caption") : null;

    /// <summary>Each record as label, value and date; the date is empty when there is no value.</summary>
    public IReadOnlyList<(string Label, string Value, string Date)> Records()
    {
        WaitUntilLoaded();
        return web.FindAll("[data-testid=\"exercise-records\"] .row")
            .Select(r => (Part(r, ".label"), Part(r, ".value"), Part(r, ".date")))
            .ToList();
    }

    /// <summary>Each session as date, headline and sets, newest first.</summary>
    public IReadOnlyList<(string Date, string Headline, string Sets)> Sessions()
    {
        WaitUntilLoaded();
        return web.FindAll("[data-testid^=\"exercise-session-\"]")
            .Select(r => (Part(r, ".date"), Part(r, ".headline"), Part(r, ".detail")))
            .ToList();
    }

    /// <summary>Each tab's label, in order.</summary>
    public IReadOnlyList<string> Tabs()
    {
        WaitUntilLoaded();
        return web.FindAll("[data-testid^=\"exercise-tab-\"]").Select(e => e.FindElement(By.XPath("..")).GetDomProperty("textContent")!.Trim()).ToList();
    }

    /// <summary>Taps a tab's label: the radio inside it is hidden.</summary>
    public void Tab(string key) => web.Find($"exercise-tab-{key}").FindElement(By.XPath("..")).Click();

    /// <summary>Taps a range's label, e.g. 1y.</summary>
    public void Range(string key) => web.Find($"exercise-range-{key}").FindElement(By.XPath("..")).Click();

    private static string Part(IWebElement row, string css) => row.FindElements(By.CssSelector(css)).SingleOrDefault()?.Text.Trim() ?? "";
}
