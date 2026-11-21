namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>
/// This repo holds no custom UI elements: general components come from the Shields.UI kit (app-ui-kit), and the app holds screens and the
/// few app-only components listed here, each with a reason. A new <c>*.razor</c> file, or a <c>ComponentBase</c> class, in WorkoutLog.UI or
/// WorkoutLog.App fails until it is on the list, and a listed component that is gone fails too, so the list stays exact.
/// A reason starting "kit gap" names what the kit lacks and what the app does meanwhile.
/// </summary>
public class AppComponentAllowListTests
{
    private const string Rule = "This repo holds no custom UI elements: use a kit component, or ask for one in https://github.com/marty-shields/app-ui-kit "
        + "(see AGENTS.md and docs/dev-setup-build.md, 'No custom UI elements'). Only a screen or a truly app-only component goes on the allow-list, with a one-line reason.";

    internal static readonly IReadOnlyDictionary<string, string> Allowed = new Dictionary<string, string>
    {
        // Screens (WorkoutLog.UI)
        ["AddExerciseView"] = "screen: pick an exercise to add",
        ["EditWorkoutPage"] = "screen page: edits a saved workout",
        ["ExercisePage"] = "screen page: one exercise's progress",
        ["ExerciseView"] = "screen: one exercise's progress",
        ["ImportReviewPage"] = "screen page: reviews an import",
        ["ImportReviewView"] = "screen: reviews an import",
        ["NewWorkoutPage"] = "screen page: logs a new workout",
        ["SettingsPage"] = "screen page: settings",
        ["SettingsView"] = "screen: settings",
        ["ThisWeekPage"] = "screen page: this week",
        ["ThisWeekView"] = "screen: this week",
        ["TrendsPage"] = "screen page: trends",
        ["TrendsView"] = "screen: trends",
        ["WorkoutLoadView"] = "screen: the workout being loaded or failed",
        ["WorkoutSummaryPage"] = "screen page: summary after saving",
        ["WorkoutSummaryView"] = "screen: summary after saving",
        ["WorkoutView"] = "screen: the workout being logged",
        ["WorkoutsPage"] = "screen page: workout history",
        ["WorkoutsView"] = "screen: workout history",
        // Pages and host (WorkoutLog.App)
        ["EditWorkout"] = "route: hosts EditWorkoutPage",
        ["Exercise"] = "route: hosts ExercisePage",
        ["Home"] = "route: hosts ThisWeekPage",
        ["ImportReview"] = "route: hosts ImportReviewPage",
        ["NewWorkout"] = "route: hosts NewWorkoutPage",
        ["Settings"] = "route: hosts SettingsPage",
        ["Trends"] = "route: hosts TrendsPage",
        ["WorkoutSummary"] = "route: hosts WorkoutSummaryPage",
        ["Workouts"] = "route: hosts WorkoutsPage",
        ["Routes"] = "router: this app's routes and layout",
        ["SystemBarInsets"] = "Android-only host bridge: sets the status and navigation bar insets (a ComponentBase with no markup)",
        // Components (WorkoutLog.UI)
        ["Chart"] = "kit gap: no line chart; meanwhile an SVG line chart of an exercise's progress",
        ["DateRangeField"] = "kit gap: no collapsed date-range picker row; meanwhile a button that opens the kit RangeCalendar",
        ["NoteField"] = "kit gap: no multi-line note field with a character count; meanwhile this component",
        ["AddExerciseSearch"] = "kit gap: SearchField has no match-count live region or Enter handling this screen needs; meanwhile a native search input",
        ["ExerciseList"] = "kit gap: no list of selectable rows with a custom-exercise row; meanwhile this list",
        ["RecordsCard"] = "app-only: records list of this app's domain, built from kit Card and ListRow",
        ["SessionRow"] = "kit gap: ListRow has no three-part date, headline, detail row; meanwhile this row",
        ["DayStrip"] = "kit gap: DateStrip is a picker, not a read-only week summary; meanwhile this seven-day strip",
        ["LastWeekCard"] = "app-only: last week's summary card of this app's domain, built from kit Card",
        ["MuscleSetsCard"] = "app-only: sets per muscle group, built from kit Card and BarList",
        ["WeekRecordsCard"] = "app-only: this week's records card, built from kit Card",
        ["WeekTiles"] = "app-only: this week's stat tiles, built from kit StatTile",
        ["WeekWorkoutsCard"] = "app-only: this week's workouts card, built from kit Card and ListRow",
        ["LongestStreakCard"] = "app-only: longest streak card, built from kit Card",
        ["MonthWeeks"] = "kit gap: no row list of week totals with a bar; meanwhile this list",
        ["RecordRows"] = "kit gap: ListRow has no record row with an icon and change text; meanwhile this list",
        ["TrendTiles"] = "app-only: trend stat tiles, built from kit StatTile",
        ["WeekBars"] = "kit gap: TrendBarChart has no unlabelled one-colour-per-week bars with a count summary; meanwhile this SVG chart",
        ["EntryErrors"] = "kit gap: no summary of several field errors under a set; meanwhile this alert list",
        ["ExerciseCard"] = "app-only: an exercise being logged, with its sets, built from kit Card",
        ["LastNote"] = "kit gap: no collapsible one-line note; meanwhile this button",
        ["RecordBadge"] = "kit gap: no record badge; meanwhile an icon with text",
        ["SetPanel"] = "app-only: the open set of a carry, built from kit Card and ValueField",
        ["SetRow"] = "app-only: the open set as one row of kit fields",
        ["SetSummary"] = "app-only: a closed set as one line with edit and delete buttons",
    };

    [Test]
    public void EveryAppComponent_IsOnTheAllowList()
    {
        Assert.That(Names().Except(Allowed.Keys).Order(), Is.Empty, Rule);
    }

    [Test]
    public void EveryAllowListEntry_IsAComponentThatExists()
    {
        Assert.That(Allowed.Keys.Except(Names()).Order(), Is.Empty, "remove the entry: the component is gone");
    }

    [Test]
    public void EveryAllowListEntry_HasAOneLineReason()
    {
        Assert.That(Allowed.Where(entry => string.IsNullOrWhiteSpace(entry.Value) || entry.Value.Contains('\n')).Select(entry => entry.Key), Is.Empty);
    }

    [Test]
    public void NoTwoComponentsShareAName_SoTheListKeysAreUnambiguous()
    {
        var repeated = AppComponents.Found().GroupBy(component => component.Name).Where(group => group.Count() > 1)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(component => component.Path))}");

        Assert.That(repeated, Is.Empty, "rename one: the allow-list is keyed by component name");
    }

    private static IEnumerable<string> Names() => AppComponents.Found().Select(component => component.Name);
}
