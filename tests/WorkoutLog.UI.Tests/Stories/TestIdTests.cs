using AngleSharp.Dom;

namespace WorkoutLog.UI.Tests.Stories;

public class TestIdTests : ComponentTestBase
{
    [TestCaseSource(typeof(ScreenStates), nameof(ScreenStates.All))]
    public void ScreenState_InteractiveElementsHaveTestIds(ScreenState state)
    {
        var elements = state.Render(Context);

        Assert.That(WithoutTestId(elements), Is.Empty);
    }

    /// <summary>Buttons, inputs, links and elements with a click handler that have no data-testid, as their opening markup.</summary>
    public static IEnumerable<string> WithoutTestId(IEnumerable<IElement> elements) =>
        elements
            .Where(IsInteractive)
            .Where(e => string.IsNullOrEmpty(e.GetAttribute("data-testid")))
            .Select(e => e.OuterHtml[..(e.OuterHtml.IndexOf('>') + 1)]);

    private static bool IsInteractive(IElement element) =>
        element.LocalName is "button" or "input" or "select" or "textarea" or "a"
        || element.Attributes.Any(a => a.Name == "blazor:onclick");
}
