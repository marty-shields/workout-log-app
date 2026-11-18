namespace WorkoutLog.UI.Tests.Navigation;

/// <summary>
/// The App's <c>/trends</c> page shows <c>TrendsPage</c> with the query, puts a step in the URL and opens an exercise from the week shown, so back
/// returns to it. The App can't be referenced, so its files are read. Whitespace is ignored.
/// </summary>
public class TrendsPageWiringTests
{
    [TestCase("@page\"/trends\"")]
    [TestCase("<TrendsPage")]
    [TestCase("KindQuery=\"Period\"")]
    [TestCase("DateQuery=\"Date\"")]
    [TestCase("[SupplyParameterFromQuery]publicstring?Period{get;set;}")]
    [TestCase("[SupplyParameterFromQuery]publicstring?Date{get;set;}")]
    [TestCase("OnPeriodChanged=\"@(week=>Navigation.GoTo(TrendsRoute.Week(week)))\"")]
    [TestCase("OnNew=\"@(() => Navigation.GoTo(\"workout/new\"))\"")]
    [TestCase("OnNavigate=\"@(tab => { if (tab == MainTab.ThisWeek) { Navigation.GoTo(string.Empty); } })\"")]
    [TestCase("OnSettings=\"@(() => Navigation.GoTo(\"settings\"))\"")]
    [TestCase("Navigation.GoTo($\"exercises/{opened.ExerciseId}?from={Uri.EscapeDataString(Origin.Trends(opened.Week))}\")")]
    public void TrendsPage_IsWiredToTheQueryTheUrlAndTheExercise(string markup) =>
        Assert.That(
            WithoutWhitespace(RepoFiles.Read("src", "WorkoutLog.App", "Components", "Pages", "Trends.razor")),
            Does.Contain(WithoutWhitespace(markup)));

    [Test]
    public void ExercisePage_ReturnsToWhereItOpenedFromWithOrigin() =>
        Assert.That(
            WithoutWhitespace(RepoFiles.Read("src", "WorkoutLog.App", "Components", "Pages", "Exercise.razor")),
            Does.Contain(WithoutWhitespace("Navigation.GoTo(Origin.Route(From, \"exercises\"))")));

    private static string WithoutWhitespace(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
}
