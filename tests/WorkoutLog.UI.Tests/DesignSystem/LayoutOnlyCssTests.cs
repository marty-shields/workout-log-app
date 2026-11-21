using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>
/// App CSS only lays out: display, grid and flex, gap, margin, padding, sizes, position, overflow, alignment, order and z-index, with values
/// from kit tokens. How something looks (colour, background, border, radius, shadow, outline, font, text decoration, SVG paint) comes from a
/// kit component or parameter. An exception is a <c>file: property</c> in <see cref="Exceptions"/> with a reason. "Kit gap" names what the kit
/// lacks and what the app does meanwhile.
/// </summary>
public partial class LayoutOnlyCssTests
{
    private const string Rule = "App CSS only lays out. Use a kit component or parameter for the look, or ask for one in https://github.com/marty-shields/app-ui-kit "
        + "(AGENTS.md, 'No custom UI elements'). A real exception goes in LayoutOnlyCssTests.Exceptions with a reason.";

    private static readonly KeyValuePair<string, string>[] Listed =
    [
        ..For("src/WorkoutLog.UI/Components/Charts/Chart.razor.css", "kit gap: no line chart (an exercise progress line with points and a best ring); meanwhile this SVG chart paints its own strokes and text", "fill", "font-size", "font-variant-numeric", "font-weight", "stroke", "stroke-linejoin", "stroke-width"),
        ..For("src/WorkoutLog.UI/Components/Inputs/DateRangeField.razor.css", "kit gap: no collapsed date-range field that opens a RangeCalendar; meanwhile this button draws the input box", "background-color", "border", "border-radius", "color", "font-size", "font-variant-numeric", "outline", "outline-offset"),
        ..For("src/WorkoutLog.UI/Components/Inputs/NoteField.razor.css", "kit gap: no multi-line note field with a count and error outline; meanwhile this component draws its own box and text", "background-color", "border", "border-radius", "color", "font", "font-size", "font-variant-numeric", "font-weight"),
        ..For("src/WorkoutLog.UI/Components/Lists/AddExerciseSearch.razor.css", "kit gap: SearchField lacks the result-count line and Enter handling here; meanwhile a native search input draws the box", "background-color", "border", "border-radius"),
        ..For("src/WorkoutLog.UI/Components/Lists/RecordsCard.razor.css", "kit gap: ListRow cannot show a label, figure and unit on one line with a divider; meanwhile rows are drawn here", "border-bottom", "color", "font-size", "font-variant-numeric", "font-weight"),
        ..For("src/WorkoutLog.UI/Components/Lists/SessionRow.razor.css", "kit gap: ListRow has no date, headline and detail layout; meanwhile this row styles its own text and divider", "border-bottom", "color", "font-size", "font-variant-numeric", "font-weight"),
        ..For("src/WorkoutLog.UI/Components/ThisWeek/DayStrip.razor.css", "kit gap: DateStrip is a picker, not a read-only week strip with done, today and future days; meanwhile this strip draws its dots", "background-color", "border", "border-color", "border-radius", "border-style", "color", "font-size", "font-variant-numeric", "font-weight", "opacity", "outline", "outline-offset"),
        ..For("src/WorkoutLog.UI/Components/ThisWeek/LastWeekCard.razor.css", "kit gap: ListRow has no option for a card with one row and no bottom divider; meanwhile the divider is removed here", "border-bottom"),
        ..For("src/WorkoutLog.UI/Components/ThisWeek/WeekRecordsCard.razor.css", "kit gap: ListRow has no record row with a change line and group dividers; meanwhile these rows are styled here", "border-bottom", "border-top", "color", "font-size"),
        ..For("src/WorkoutLog.UI/Components/ThisWeek/WeekWorkoutsCard.razor.css", "kit gap: ListRow has no option to drop its divider or mute its detail; meanwhile it is styled here", "border-bottom", "color"),
        ..For("src/WorkoutLog.UI/Components/Trends/LongestStreakCard.razor.css", "kit gap: ListRow has no option for a card with one row and no bottom divider; meanwhile the divider is removed here", "border-bottom"),
        ..For("src/WorkoutLog.UI/Components/Trends/MonthWeeks.razor.css", "kit gap: no list of week rows with a proportional bar; meanwhile this list draws its own bars and text", "background-color", "background-image", "border", "border-bottom", "color", "font", "font-size", "font-variant-numeric"),
        ..For("src/WorkoutLog.UI/Components/Trends/RecordRows.razor.css", "kit gap: ListRow has no record row with an icon and change text; meanwhile these rows are styled here", "border-bottom", "border-top", "color", "font-size"),
        ..For("src/WorkoutLog.UI/Components/Weekly/WeekBars.razor.css", "kit gap: TrendBarChart has no per-week count bars with a muted axis and this-week accent; meanwhile this SVG chart paints its own bars", "color", "fill", "font-size", "stroke", "stroke-width"),
        ..For("src/WorkoutLog.UI/Components/Workout/EntryErrors.razor.css", "kit gap: InlineAlert is one boxed message, not several short field errors; meanwhile these lines are styled here", "color", "font-size", "font-weight"),
        ..For("src/WorkoutLog.UI/Components/Workout/ExerciseCard.razor.css", "kit gap: Typography has no variants for a small muted last-time line or an inline total; meanwhile this card styles them", "color", "font-size"),
        ..For("src/WorkoutLog.UI/Components/Workout/LastNote.razor.css", "kit gap: no collapsible one-line note with an icon; meanwhile this button draws its own surface and text", "background-color", "border", "color", "font-size"),
        ..For("src/WorkoutLog.UI/Components/Workout/RecordBadge.razor.css", "kit gap: no Record badge (success-coloured icon and bold small text); meanwhile this badge styles its text", "color", "font-size", "font-weight"),
        ..For("src/WorkoutLog.UI/Components/Workout/SetRow.razor.css", "kit gap: no entry-grid row primitive (index, last-time cell, fields); meanwhile this row styles its number and Last cell", "color", "font-size", "font-variant-numeric"),
        ..For("src/WorkoutLog.UI/Components/Workout/SetSummary.razor.css", "kit gap: ListRow has no numbered summary line with edit and delete actions; meanwhile this line styles its text", "color", "font-size", "font-variant-numeric", "font-weight"),
        ..For("src/WorkoutLog.UI/Screens/AddExerciseView.razor.css", "kit gap: AppScreen has no divider under a pinned tools area; meanwhile the divider is drawn here", "border-bottom"),
        ..For("src/WorkoutLog.UI/Screens/ThisWeekView.razor.css", "kit gap: Typography has no large bold heading variant for the week range; meanwhile it is sized here", "font-size", "font-weight"),
        ..For("src/WorkoutLog.UI/Screens/WorkoutSummaryView.razor.css", "kit gap: ListRow and Typography lack the record and change lines of a summary; meanwhile these lines are styled here", "border-bottom", "color", "font-size", "font-variant-numeric", "font-weight", "line-height"),
        ..For("src/WorkoutLog.UI/Screens/WorkoutsView.razor.css", "kit gap: no sticky month header with a count and divider, and no filter-tools divider; meanwhile they are styled here", "background-color", "border-bottom", "color", "font-size", "font-variant-numeric", "font-weight"),
    ];

    internal static readonly IReadOnlyDictionary<string, string> Exceptions = Listed.ToDictionary(entry => entry.Key, entry => entry.Value);

    private static IEnumerable<KeyValuePair<string, string>> For(string file, string reason, params string[] properties) =>
        properties.Select(property => KeyValuePair.Create($"{file}: {property}", reason));

    [Test]
    public void AppCss_OnlyLaysOut()
    {
        Assert.That(Visual(AppCss.Files()).Except(Exceptions.Keys).Order(), Is.Empty, Rule);
    }

    [Test]
    public void EveryException_StillMatchesADeclaration()
    {
        Assert.That(Exceptions.Keys.Except(Visual(AppCss.Files())).Order(), Is.Empty, "remove the exception: the declaration is gone");
    }

    [Test]
    public void EveryException_HasAOneLineReason()
    {
        Assert.That(Exceptions.Where(entry => string.IsNullOrWhiteSpace(entry.Value) || entry.Value.Contains('\n')).Select(entry => entry.Key), Is.Empty);
    }

    [TestCase("color: red;")]
    [TestCase("background-color: var(--color-bg);")]
    [TestCase("border-bottom: var(--border-width) solid var(--color-border);")]
    [TestCase("border-radius: var(--radius-md);")]
    [TestCase("box-shadow: none;")]
    [TestCase("outline: none;")]
    [TestCase("font-size: var(--font-size-sm);")]
    [TestCase("font-variant-numeric: tabular-nums;")]
    [TestCase("text-decoration: underline;")]
    [TestCase("fill: currentColor;")]
    [TestCase("stroke-width: 1;")]
    public void VisualProperties_AreFound(string declaration)
    {
        Assert.That(VisualIn($".a {{ {declaration} }}"), Has.Count.EqualTo(1));
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
        Assert.That(VisualIn($".a {{ {declaration} }}"), Is.Empty);
    }

    [Test]
    public void SelectorWithAPseudoClass_IsNotADeclaration()
    {
        Assert.That(VisualIn(".a:hover { margin: 0; }\n.color:focus { gap: 0; }"), Is.Empty);
    }

    [Test]
    public void Comments_AreIgnored()
    {
        Assert.That(VisualIn(".a { /* color: red; */ margin: 0; }"), Is.Empty);
    }

    private static IEnumerable<string> Visual(IEnumerable<string> files) =>
        files.SelectMany(file => VisualIn(File.ReadAllText(file)).Select(property => $"{AppCss.Name(file)}: {property}")).Distinct();

    private static IReadOnlyList<string> VisualIn(string css) =>
        Declaration().Matches(Comment().Replace(css, ""))
            .Select(m => m.Groups[1].Value)
            .Where(property => VisualProperty().IsMatch(property))
            .ToList();

    [GeneratedRegex(@"(?<=[{;]\s*)([a-z-]+)\s*:")]
    private static partial Regex Declaration();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    // Colour, background, border (and radius), shadow, outline, font (and its variants), text decoration, case and line height, opacity, and SVG paint.
    [GeneratedRegex(@"^(color|background[a-z-]*|border[a-z-]*|box-shadow|outline[a-z-]*|font[a-z-]*|text-decoration[a-z-]*|text-transform|text-shadow|line-height|letter-spacing|opacity|fill[a-z-]*|stroke[a-z-]*)$")]
    private static partial Regex VisualProperty();
}
