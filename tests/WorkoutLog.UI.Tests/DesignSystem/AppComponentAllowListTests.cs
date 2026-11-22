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
        ["ExerciseList"] = "app-only: the rows of the add-exercise list (name, kind, last session) and its no-match text, built from kit ListRow",
        ["RecordsCard"] = "app-only: records list of this app's domain, built from kit Card and ListRow",
        ["LastWeekCard"] = "app-only: last week's summary card of this app's domain, built from kit Card",
        ["MuscleSetsCard"] = "app-only: sets per muscle group, built from kit Card and BarList",
        ["WeekRecordsCard"] = "app-only: this week's records card, built from kit Card",
        ["WeekTiles"] = "app-only: this week's stat tiles, built from kit StatTile",
        ["WeekWorkoutsCard"] = "app-only: this week's workouts card, built from kit Card and ListRow",
        ["LongestStreakCard"] = "app-only: longest streak card, built from kit Card",
        ["RecordRows"] = "app-only: the records of a period (up to five rows, then n more), built from kit ListRow",
        ["TrendTiles"] = "app-only: trend stat tiles, built from kit StatTile",
        ["ExerciseCard"] = "app-only: an exercise being logged, with its sets, built from kit Card",
        ["SetPanel"] = "app-only: the open set of a carry, a selected kit Card of kit ValueFields",
        ["SetRow"] = "app-only: the open set as one kit EntryGridRow of kit fields",
        ["SetSummary"] = "app-only: a closed set as one kit ListRow with edit and delete buttons",
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
