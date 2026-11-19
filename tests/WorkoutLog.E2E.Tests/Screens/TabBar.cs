namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>The tab bar on This week and Trends: the tabs "This week" and "Trends", and the raised + (New) between them.</summary>
public sealed class TabBar(WebView web)
{
    public void Open(string tab) => web.Tap(TestId(tab));

    public bool Has(string tab) => web.Exists(TestId(tab));

    /// <summary>The raised + that opens a new workout.</summary>
    public void New() => web.Tap("new-link");

    public bool HasPlus() => web.Exists("new-link");

    /// <summary>The tab marked as the current page, or null when the bar isn't shown.</summary>
    public string? Current() =>
        web.FindAll("[data-testid=tab-bar] [aria-current=page]").Select(e => e.GetDomAttribute("data-testid")).SingleOrDefault() switch
        {
            "this-week-link" => "This week",
            "trends-link" => "Trends",
            _ => null,
        };

    private static string TestId(string tab) => tab switch
    {
        "This week" => "this-week-link",
        "Trends" => "trends-link",
        _ => throw new ArgumentException($"Unknown tab: {tab}.", nameof(tab)),
    };
}
