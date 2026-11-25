using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Navigation;

/// <summary>
/// The App's pages carry where a screen was opened from in <c>?from=</c> and return there. The App can't be referenced, so its files are read.
/// Whitespace is ignored.
/// </summary>
public partial class PageWiringTests
{
    [TestCase("Home.razor", "@page\"/\"")]
    [TestCase("Home.razor", "<ThisWeekPage")]
    [TestCase("Home.razor", "OnNew=\"@(()=>Navigation.GoTo(Origin.Link(\"workout/new\",Origin.ThisWeek)))\"")]
    [TestCase("Home.razor", "OnResume=\"@(()=>Navigation.GoTo(Origin.Resume(Origin.ThisWeek)))\"")]
    [TestCase("Home.razor", "OnNavigate=\"@(tab=>Navigation.GoTo(tab))\"")]
    [TestCase("Home.razor", "OnOpenWorkout=\"@(id=>Navigation.GoTo(Origin.Link($\"workout/{id}\",Origin.ThisWeek)))\"")]
    [TestCase("Home.razor", "OnOpenExercise=\"@(id=>Navigation.GoTo(Origin.Link($\"exercises/{id}\",Origin.ThisWeek)))\"")]
    [TestCase("Home.razor", "OnOpenLastWeek=\"@(week=>Navigation.GoTo(TrendsRoute.To(week)))\"")]
    [TestCase("Home.razor", "OnSettings=\"@(()=>Navigation.GoTo(\"settings\"))\"")]
    [TestCase("Trends.razor", "@page\"/trends\"")]
    [TestCase("Trends.razor", "<TrendsPage")]
    [TestCase("Trends.razor", "KindQuery=\"@Period\"")]
    [TestCase("Trends.razor", "DateQuery=\"@Date\"")]
    [TestCase("Trends.razor", "[SupplyParameterFromQuery]publicstring?Period{get;set;}")]
    [TestCase("Trends.razor", "[SupplyParameterFromQuery]publicstring?Date{get;set;}")]
    [TestCase("Trends.razor", "OnPeriodChanged=\"@(period=>Navigation.GoTo(TrendsRoute.To(period)))\"")]
    [TestCase("Trends.razor", "OnNew=\"@(period=>Navigation.GoTo(Origin.Link(\"workout/new\",Origin.Trends(period))))\"")]
    [TestCase("Trends.razor", "OnResume=\"@(period=>Navigation.GoTo(Origin.Resume(Origin.Trends(period))))\"")]
    [TestCase("Trends.razor", "OnNavigate=\"@(tab=>Navigation.GoTo(tab))\"")]
    [TestCase("Trends.razor", "OnSettings=\"@(period=>Navigation.GoTo(Origin.Link(\"settings\",Origin.Trends(period))))\"")]
    [TestCase("Trends.razor", "OnOpenWorkouts=\"@(opened=>Navigation.GoTo(WorkoutsRoute.To(opened.Dates,Origin.Trends(opened.Period))))\"")]
    [TestCase("Trends.razor", "OnOpenExercise=\"@(opened=>Navigation.GoTo(Origin.Link($\"exercises/{opened.ExerciseId}\",Origin.Trends(opened.Period))))\"")]
    [TestCase("Workouts.razor", "@page\"/workouts\"")]
    [TestCase("Workouts.razor", "<WorkoutsPageFirstQuery=\"@First\"LastQuery=\"@Last\"")]
    [TestCase("Workouts.razor", "[SupplyParameterFromQuery]publicstring?First{get;set;}")]
    [TestCase("Workouts.razor", "[SupplyParameterFromQuery]publicstring?Last{get;set;}")]
    [TestCase("Workouts.razor", "[SupplyParameterFromQuery]publicstring?From{get;set;}")]
    [TestCase("Workouts.razor", "OnBack=\"@(()=>Navigation.GoTo(Origin.Route(From)))\"")]
    [TestCase("Workouts.razor", "OnOpen=\"@(opened=>Navigation.GoTo(Origin.Link($\"workout/{opened.Id}\",Origin.Workouts(opened.Dates,From))))\"")]
    [TestCase("Exercise.razor", "@page\"/exercises/{Id:int}\"")]
    [TestCase("Exercise.razor", "OnBack=\"@(()=>Navigation.GoTo(Origin.Route(From)))\"")]
    [TestCase("NewWorkout.razor", "<NewWorkoutPageResume=\"Resume\"")]
    [TestCase("NewWorkout.razor", "[SupplyParameterFromQuery]publicboolResume{get;set;}")]
    [TestCase("NewWorkout.razor", "OnDiscarded=\"@(()=>Navigation.GoTo(Origin.Route(From)))\"")]
    [TestCase("NewWorkout.razor", "OnSaved=\"@(id=>Navigation.GoTo($\"workout/{id}/summary\"))\"")]
    [TestCase("NewWorkout.razor", "[SupplyParameterFromQuery]publicstring?From{get;set;}")]
    [TestCase("EditWorkout.razor", "OnSaved=\"@(()=>Navigation.GoTo(BackRoute))\"")]
    [TestCase("EditWorkout.razor", "OnBack=\"@(()=>Navigation.GoTo(BackRoute))\"")]
    [TestCase("EditWorkout.razor", "OnDeleted=\"@(()=>Navigation.GoTo(BackRoute))\"")]
    [TestCase("EditWorkout.razor", "BackRoute=>Origin.Route(From)")]
    [TestCase("WorkoutSummary.razor", "OnDone=\"@(()=>Navigation.GoTo(string.Empty))\"")]
    [TestCase("Settings.razor", "OnBack=\"@(()=>Navigation.GoTo(Origin.Route(From)))\"")]
    [TestCase("Settings.razor", "OnImportReview=\"@(()=>Navigation.GoTo(Origin.Link(\"settings/import\",From)))\"")]
    [TestCase("ImportReview.razor", "SettingsRoute=>Origin.Link(\"settings\",From)")]
    public void Page_IsWired(string file, string markup) =>
        Assert.That(
            WithoutWhitespace(RepoFiles.Read("src", "WorkoutLog.App", "Components", "Pages", file)),
            Does.Contain(WithoutWhitespace(markup)));

    /// <summary>A string parameter written <c>KindQuery="Period"</c> receives the text "Period", not the property; it needs <c>@Period</c>.</summary>
    [Test]
    public void QueryParameters_AreNotPassedAsText()
    {
        var pages = Directory.GetFiles(Path.Combine(RepoFiles.Root, "src", "WorkoutLog.App", "Components", "Pages"), "*.razor");
        var literals = pages.SelectMany(page => LiteralQuery().Matches(File.ReadAllText(page)).Select(m => $"{Path.GetFileName(page)}: {m.Value}"));
        Assert.That(literals, Is.Empty);
    }

    [Test]
    public void ThereIsNoExercisesListPage() =>
        Assert.That(File.Exists(Path.Combine(RepoFiles.Root, "src", "WorkoutLog.App", "Components", "Pages", "Exercises.razor")), Is.False);

    [GeneratedRegex(@"\b\w+Query=""[^@""][^""]*""")]
    private static partial Regex LiteralQuery();

    private static string WithoutWhitespace(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
}
