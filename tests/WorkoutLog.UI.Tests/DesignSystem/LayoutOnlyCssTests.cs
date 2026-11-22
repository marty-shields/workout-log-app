using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>
/// App styles only lay out: display, grid and flex, gap, margin, padding, sizes, position, overflow, alignment, order and z-index, with values
/// from kit tokens. How something looks (colour, background, border, radius, shadow, outline, font, text decoration, filter, mask, SVG paint) comes
/// from a kit component or parameter. An exception is a file, selector and property in <see cref="Exceptions"/> with a reason, so a new visual
/// declaration in an excepted file still fails. "Kit gap" names what the kit lacks and what the app does meanwhile. Stylesheets, style blocks and
/// inline styles are all scanned (see <see cref="AppCss"/>).
/// </summary>
public partial class LayoutOnlyCssTests
{
    private const string Rule = "App styles only lay out. Use a kit component or parameter for the look, or ask for one in https://github.com/marty-shields/app-ui-kit "
        + "(AGENTS.md, 'No custom UI elements'). A real exception goes in LayoutOnlyCssTests.Exceptions with a reason.";

    private static readonly KeyValuePair<string, string>[] Listed =
    [
        ..In("src/WorkoutLog.UI/Components/ThisWeek/DayStrip.razor.css", "kit gap: the kit has no read-only week strip with done, today and future days (DateStrip steps one day, MonthCalendar is a month grid); meanwhile this strip draws its dots and day names",
            (".day", ["color", "font-size"]), (".done .dot", ["background-color", "border-color", "border-style", "color"]), (".dot", ["border", "border-radius", "font-variant-numeric"]), (".future .dot", ["opacity"]), (".today", ["color", "font-weight"]), (".today .dot", ["outline", "outline-offset"])),
    ];

    /// <summary>Allowed visual declarations, keyed <c>file | selector | property</c>, each with its reason.</summary>
    internal static readonly IReadOnlyDictionary<string, string> Exceptions = Listed.ToDictionary(entry => entry.Key, entry => entry.Value);

    private static IEnumerable<KeyValuePair<string, string>> In(string file, string reason, params (string Selector, string[] Properties)[] rules) =>
        rules.SelectMany(rule => rule.Properties.Select(property => KeyValuePair.Create(Key(file, rule.Selector, property), reason)));

    [Test]
    public void AppStyles_OnlyLayOut()
    {
        Assert.That(Visual(AppCss.All()).Except(Exceptions.Keys).Order(), Is.Empty, Rule);
    }

    [Test]
    public void EveryException_StillMatchesADeclaration()
    {
        Assert.That(Exceptions.Keys.Except(Visual(AppCss.All())).Order(), Is.Empty, "remove the exception: the declaration is gone");
    }

    [Test]
    public void EveryException_HasAOneLineReason()
    {
        Assert.That(Exceptions.Where(entry => string.IsNullOrWhiteSpace(entry.Value) || entry.Value.Contains('\n')).Select(entry => entry.Key), Is.Empty);
    }

    [Test]
    public void ANewVisualDeclaration_InAnExceptedFile_StillFails()
    {
        var excepted = AppCss.InCss("src/WorkoutLog.UI/Components/ThisWeek/DayStrip.razor.css", ".new { color: red; }");

        Assert.That(Visual(excepted).Except(Exceptions.Keys), Is.Not.Empty);
    }

    [TestCase("color: red;")]
    [TestCase("COLOR: red;")]
    [TestCase("background-color: var(--color-bg);")]
    [TestCase("border-bottom: var(--border-width) solid var(--color-border);")]
    [TestCase("border-radius: var(--radius-md);")]
    [TestCase("box-shadow: none;")]
    [TestCase("outline: none;")]
    [TestCase("font-size: var(--font-size-sm);")]
    [TestCase("font-variant-numeric: tabular-nums;")]
    [TestCase("text-decoration: underline;")]
    [TestCase("text-transform: uppercase;")]
    [TestCase("text-shadow: none;")]
    [TestCase("letter-spacing: 1px;")]
    [TestCase("opacity: 0.5;")]
    [TestCase("fill: currentColor;")]
    [TestCase("stroke-width: 1;")]
    [TestCase("caret-color: red;")]
    [TestCase("accent-color: red;")]
    [TestCase("scrollbar-color: red blue;")]
    [TestCase("-webkit-tap-highlight-color: transparent;")]
    [TestCase("-webkit-text-fill-color: red;")]
    [TestCase("-webkit-text-stroke: 1px;")]
    [TestCase("filter: blur(2px);")]
    [TestCase("backdrop-filter: blur(2px);")]
    [TestCase("mask-image: none;")]
    [TestCase("column-rule: 1px solid;")]
    [TestCase("text-emphasis: filled;")]
    public void VisualProperties_AreFound(string declaration)
    {
        Assert.That(Visual(AppCss.InCss("a.css", $".a {{ {declaration} }}")).Count(), Is.EqualTo(1));
    }

    [TestCase("display: grid;")]
    [TestCase("gap: var(--space-2);")]
    [TestCase("margin: 0;")]
    [TestCase("min-width: 0;")]
    [TestCase("align-items: center;")]
    [TestCase("grid-template-columns: repeat(2, 1fr);")]
    [TestCase("overflow-wrap: anywhere;")]
    [TestCase("white-space: nowrap;")]
    [TestCase("text-align: center;")]
    [TestCase("list-style: none;")]
    public void LayoutProperties_AreNotFound(string declaration)
    {
        Assert.That(Visual(AppCss.InCss("a.css", $".a {{ {declaration} }}")), Is.Empty);
    }

    [Test]
    public void InlineStylesAndStyleBlocks_AreChecked()
    {
        var markup = "<div style=\"color: red; margin: 0\"></div><style>.a { border: none; }</style>";

        Assert.That(Visual(AppCss.InMarkup("a.razor", markup)), Is.EqualTo(new[] { "a.razor | .a | border", "a.razor | style=\"\" | color" }));
    }

    [Test]
    public void SelectorWithAPseudoClass_IsNotADeclaration()
    {
        Assert.That(Visual(AppCss.InCss("a.css", ".a:hover { margin: 0; }\n.color:focus { gap: 0; }")), Is.Empty);
    }

    [Test]
    public void Comments_AreIgnored()
    {
        Assert.That(Visual(AppCss.InCss("a.css", ".a { /* color: red; */ margin: 0; }")), Is.Empty);
    }

    private static string Key(string file, string selector, string property) => $"{file} | {selector} | {property}";

    private static IEnumerable<string> Visual(IEnumerable<AppCss.Declaration> declarations) =>
        declarations.Where(d => VisualProperty().IsMatch(d.Property)).Select(d => Key(d.File, d.Selector, d.Property)).Distinct();

    // Colour (any property ending in -color), background, border and radius, shadow, outline, font and its variants, text decoration, emphasis, case,
    // shadow, spacing and line height, opacity, filters, masks, column rules, and SVG paint.
    [GeneratedRegex(@"^(color|[\w-]*-color|background[\w-]*|border[\w-]*|box-shadow|outline[\w-]*|font[\w-]*|text-decoration[\w-]*|text-emphasis[\w-]*|-webkit-text-[\w-]+|text-transform|text-shadow|line-height|letter-spacing|opacity|filter|backdrop-filter|mask[\w-]*|column-rule[\w-]*|fill[\w-]*|stroke[\w-]*)$")]
    private static partial Regex VisualProperty();
}
