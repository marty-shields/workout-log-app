
namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The Exercise picker overlay.</summary>
public sealed class PickerScreen(WebView web)
{
    public bool IsOpen() => web.Exists("exercise-picker");

    public string Query() => web.Value("picker-search");

    public void Search(string text) => web.Type("picker-search", text);

    public void Pick(int exerciseId) => web.Tap($"picker-item-{exerciseId}");

    public void Back() => web.Tap("picker-back");

    public string? NoMatchText() => web.Exists("picker-none") ? web.Text("picker-none") : null;

    /// <summary>Names listed, in order.</summary>
    public IReadOnlyList<string> Names() => Items("[data-testid=\"exercise-picker\"] .item");

    /// <summary>Names listed under a group heading (Strength or Cardio), in order.</summary>
    public IReadOnlyList<string> Group(string kind) => Items($"[data-testid=\"picker-group-{kind}\"] + .list .item");

    /// <summary>Group headings, in order.</summary>
    public IReadOnlyList<string> Groups() => web.FindAll("[data-testid^=\"picker-group-\"]").Select(e => e.GetDomProperty("textContent")!.Trim()).ToList();

    private IReadOnlyList<string> Items(string css)
    {
        var script = $"return [...document.querySelectorAll('{css}')].map(b => b.querySelector('span').textContent.trim());";
        return web.Strings(script);
    }
}
