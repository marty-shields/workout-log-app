using OpenQA.Selenium;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The add-exercise list, an overlay on the workout.</summary>
public sealed class AddExerciseScreen(WebView web)
{
    private const string Overlay = "[data-testid=\"add-exercise-overlay\"]";

    public bool IsOpen() => web.Exists("add-exercise-overlay");

    /// <summary>True once the last sessions have loaded and the rows show.</summary>
    public bool IsLoaded() => IsOpen() && !web.Exists("add-exercise-loading");

    public string Query() => web.Value("add-exercise-search");

    public void Search(string text) => web.Type("add-exercise-search", text);

    /// <summary>Taps a chip's label: the radio inside it is hidden.</summary>
    public void Filter(string kind) => web.Find($"add-exercise-filter-{kind.ToLowerInvariant()}").FindElement(By.XPath("..")).Click();

    public void Pick(int exerciseId) => web.Tap($"add-exercise-row-{exerciseId}");

    public void Back() => web.Tap("add-exercise-back");

    public string? NoMatchText() => web.Exists("add-exercise-none") ? web.Text("add-exercise-none") : null;

    /// <summary>Names listed, in order.</summary>
    public IReadOnlyList<string> Names() => Texts(".text");

    /// <summary>The kind shown at the right of each row, in order.</summary>
    public IReadOnlyList<string> Kinds() => Texts(".aside");

    /// <summary>The "Last …" line of the exercise's row; null when it has none.</summary>
    public string? Last(int exerciseId) =>
        web.Strings($"return [...document.querySelectorAll('{Overlay} [data-testid=\"add-exercise-row-{exerciseId}\"] .detail')].map(e => e.textContent.trim());")
            .SingleOrDefault();

    private IReadOnlyList<string> Texts(string part) =>
        web.Strings($"return [...document.querySelectorAll('{Overlay} [data-testid^=\"add-exercise-row-\"] {part}')].map(e => e.textContent.trim());");
}
