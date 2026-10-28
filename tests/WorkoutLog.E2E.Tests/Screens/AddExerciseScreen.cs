using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The add-exercise list: the Exercises list as an overlay on the workout.</summary>
public sealed class AddExerciseScreen(WebView web)
{
    private const string Overlay = "[data-testid=\"exercises-overlay\"]";

    public bool IsOpen() => web.Exists("exercises-overlay");

    /// <summary>True once the last sessions have loaded and the rows show.</summary>
    public bool IsLoaded() => IsOpen() && web.Exists("exercises-search");

    public string Query() => web.Value("exercises-search");

    public void Search(string text) => web.Type("exercises-search", text);

    /// <summary>Taps a chip's label: the radio inside it is hidden.</summary>
    public void Filter(string kind) => web.Find($"exercises-filter-{kind.ToLowerInvariant()}").FindElement(By.XPath("..")).Click();

    public void Pick(int exerciseId) => web.Tap($"exercises-row-{exerciseId}");

    public void Back() => web.Tap("exercises-back");

    public string? NoMatchText() => web.Exists("exercises-none") ? web.Text("exercises-none") : null;

    /// <summary>Names listed, in order.</summary>
    public IReadOnlyList<string> Names() => Texts(".text");

    /// <summary>The kind shown at the right of each row, in order.</summary>
    public IReadOnlyList<string> Kinds() => Texts(".aside");

    /// <summary>The "Last …" line of the exercise's row; null when it has none.</summary>
    public string? Last(int exerciseId) =>
        web.Strings($"return [...document.querySelectorAll('{Overlay} [data-testid=\"exercises-row-{exerciseId}\"] .detail')].map(e => e.textContent.trim());")
            .SingleOrDefault();

    private IReadOnlyList<string> Texts(string part) =>
        web.Strings($"return [...document.querySelectorAll('{Overlay} [data-testid^=\"exercises-row-\"] {part}')].map(e => e.textContent.trim());");
}
