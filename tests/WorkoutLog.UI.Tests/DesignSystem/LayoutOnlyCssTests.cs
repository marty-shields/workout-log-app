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
        ..In("src/WorkoutLog.UI/Components/Charts/Chart.razor.css", "kit gap: no line chart or line-weight token; meanwhile this SVG chart paints its own strokes and text, and its 2px stroke borrows --focus-width, so it moves if the kit changes that token",
            (".best", ["fill", "stroke", "stroke-width"]), (".grid", ["stroke", "stroke-width"]), (".label", ["font-weight"]), (".latest", ["fill"]), (".line", ["fill", "stroke", "stroke-linejoin", "stroke-width"]), (".plot", ["font-size", "font-variant-numeric"]), (".point", ["fill", "stroke", "stroke-width"]), (".tick, .date, .label", ["fill"])),
        ..In("src/WorkoutLog.UI/Components/Inputs/DateRangeField.razor.css", "kit gap: no collapsed date-range field that opens a RangeCalendar; meanwhile this button draws the input box",
            (".count", ["color", "font-size", "font-variant-numeric"]), (".dates", ["font-size", "font-variant-numeric"]), (".field", ["background-color", "border", "border-radius", "color"]), (".field[aria-expanded=\"true\"]", ["outline", "outline-offset"])),
        ..In("src/WorkoutLog.UI/Components/Inputs/NoteField.razor.css", "kit gap: no multi-line note field with a count and error outline; meanwhile this component draws its own box and text",
            (".count", ["font-variant-numeric"]), (".error", ["color", "font-size", "font-weight"]), (".heading", ["color", "font-size"]), (".input", ["background-color", "border", "border-radius", "color", "font"]), (".invalid", ["border"]), (".over", ["color", "font-weight"]), (".summary", ["background-color", "border", "color"]), (".summary.invalid", ["border", "border-radius"])),
        ..In("src/WorkoutLog.UI/Components/Lists/AddExerciseSearch.razor.css", "kit gap: SearchField lacks the result-count line and Enter handling here; meanwhile a native search input draws the box",
            (".search-input", ["background-color", "border", "border-radius"])),
        ..In("src/WorkoutLog.UI/Components/Lists/RecordsCard.razor.css", "kit gap: ListRow cannot show a label, figure and unit on one line with a divider; meanwhile rows are drawn here",
            (".date", ["color", "font-size"]), (".row", ["border-bottom"]), (".value", ["font-variant-numeric", "font-weight"])),
        ..In("src/WorkoutLog.UI/Components/Lists/SessionRow.razor.css", "kit gap: ListRow has no date, headline and detail layout; meanwhile this row styles its own text and divider",
            (".date, .headline", ["font-weight"]), (".detail", ["color", "font-size"]), (".row", ["border-bottom", "font-variant-numeric"])),
        ..In("src/WorkoutLog.UI/Components/ThisWeek/DayStrip.razor.css", "kit gap: DateStrip is a picker, not a read-only week strip with done, today and future days; meanwhile this strip draws its dots",
            (".day", ["color", "font-size"]), (".done .dot", ["background-color", "border-color", "border-style", "color"]), (".dot", ["border", "border-radius", "font-variant-numeric"]), (".future .dot", ["opacity"]), (".today", ["color", "font-weight"]), (".today .dot", ["outline", "outline-offset"])),
        ..In("src/WorkoutLog.UI/Components/ThisWeek/LastWeekCard.razor.css", "kit gap: ListRow has no option for a card with one row and no bottom divider; meanwhile the divider is removed here",
            (".single ::deep .row", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/ThisWeek/WeekRecordsCard.razor.css", "kit gap: ListRow has no record row with a change line and group dividers; meanwhile these rows are styled here",
            (".more", ["border-top", "color", "font-size"]), (".none", ["color"]), (".rows li:last-child ::deep .row", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/ThisWeek/WeekWorkoutsCard.razor.css", "kit gap: ListRow has no option to drop its divider or mute its detail; meanwhile it is styled here",
            (".none", ["color"]), (".rows li:last-child ::deep .row", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/Trends/LongestStreakCard.razor.css", "kit gap: ListRow has no option for a card with one row and no bottom divider; meanwhile the divider is removed here",
            (".single ::deep .row", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/Trends/MonthWeeks.razor.css", "kit gap: no list of week rows with a proportional bar; meanwhile this list draws its own bars and text",
            (".none", ["color"]), (".tap", ["background-color", "border", "color", "font"]), (".tap:active", ["background-image"]), (".totals", ["color", "font-size"]), (".week", ["border-bottom", "font-variant-numeric"]), (".week:last-child", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/Trends/RecordRows.razor.css", "kit gap: ListRow has no record row with an icon and change text; meanwhile these rows are styled here",
            (".more", ["border-top", "color", "font-size"]), (".none", ["color"]), (".rows li:last-child ::deep .row", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Components/Weekly/WeekBars.razor.css", "kit gap: TrendBarChart has no per-week count bars with a muted axis and this-week accent; meanwhile this SVG chart paints its own bars",
            (".axis", ["color", "font-size"]), (".bar", ["fill"]), (".baseline", ["stroke", "stroke-width"]), (".none", ["color"]), (".this-week", ["fill"])),
        ..In("src/WorkoutLog.UI/Components/Workout/EntryErrors.razor.css", "kit gap: InlineAlert is one boxed message, not several short field errors; meanwhile these lines are styled here",
            (".error", ["color", "font-size", "font-weight"])),
        ..In("src/WorkoutLog.UI/Components/Workout/ExerciseCard.razor.css", "kit gap: Typography has no variants for a small muted last-time line or an inline total; meanwhile this card styles them",
            (".last-time", ["color", "font-size"]), (".note", ["color", "font-size"]), (".set-header", ["color", "font-size"]), (".weights", ["color", "font-size"])),
        ..In("src/WorkoutLog.UI/Components/Workout/LastNote.razor.css", "kit gap: no collapsible one-line note with an icon; meanwhile this button draws its own surface and text",
            (".last-note", ["background-color", "border", "color", "font-size"])),
        ..In("src/WorkoutLog.UI/Components/Workout/RecordBadge.razor.css", "kit gap: no Record badge (success-coloured icon and bold small text); meanwhile this badge styles its text",
            (".badge", ["color", "font-size", "font-weight"])),
        ..In("src/WorkoutLog.UI/Components/Workout/SetPanel.razor.css", "kit gap: Card has no selected tone or heading level; meanwhile this h3 title is sized here inside a plain Card",
            (".title", ["font-size", "font-weight"])),
        ..In("src/WorkoutLog.UI/Components/Workout/SetRow.razor.css", "kit gap: no entry-grid row primitive (index, last-time cell, fields); meanwhile this row styles its number and Last cell",
            (".last", ["color", "font-size", "font-variant-numeric"]), (".last-line", ["color", "font-size", "font-variant-numeric"]), (".number", ["color", "font-variant-numeric"])),
        ..In("src/WorkoutLog.UI/Components/Workout/SetSummary.razor.css", "kit gap: ListRow has no numbered summary line with edit and delete actions; meanwhile this line styles its text",
            (".heading", ["font-size", "font-variant-numeric", "font-weight"]), (".number", ["color"])),
        ..In("src/WorkoutLog.UI/Screens/AddExerciseView.razor.css", "kit gap: AppScreen has no divider under a pinned tools area; meanwhile the divider is drawn here",
            (".tools", ["border-bottom"])),
        ..In("src/WorkoutLog.UI/Screens/ThisWeekView.razor.css", "kit gap: Typography has no large bold heading variant for the week range; meanwhile it is sized here",
            (".range", ["font-size", "font-weight"])),
        ..In("src/WorkoutLog.UI/Screens/WorkoutSummaryView.razor.css", "kit gap: ListRow and Typography lack the record and change lines of a summary; meanwhile these lines are styled here",
            (".change", ["font-size"]), (".detail", ["color", "font-size"]), (".down", ["color"]), (".label", ["color", "font-size"]), (".none, .note", ["color"]), (".row", ["border-bottom"]), (".row:last-child", ["border-bottom"]), (".same", ["color"]), (".text, .detail, .change", ["font-variant-numeric"]), (".up", ["color"]), (".value", ["color", "font-size", "font-variant-numeric", "font-weight", "line-height"])),
        ..In("src/WorkoutLog.UI/Screens/WorkoutsView.razor.css", "kit gap: no sticky month header with a count and divider, and no filter-tools divider; meanwhile they are styled here",
            (".month-count", ["color", "font-variant-numeric", "font-weight"]), (".month-title", ["background-color", "border-bottom", "font-size", "font-weight"]), (".tools", ["border-bottom"])),
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
        var excepted = AppCss.InCss("src/WorkoutLog.UI/Components/Charts/Chart.razor.css", ".new { color: red; }");

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
