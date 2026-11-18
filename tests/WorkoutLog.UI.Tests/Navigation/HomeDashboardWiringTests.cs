namespace WorkoutLog.UI.Tests.Navigation;

/// <summary>
/// The home page shows <c>ThisWeekPage</c>, and opening a workout or an exercise from it carries <c>?from=dashboard</c> so back returns to This week.
/// The App can't be referenced, so its files are read. Whitespace is ignored.
/// </summary>
public class HomeDashboardWiringTests
{
    [TestCase("<ThisWeekPage")]
    [TestCase("OnOpenWorkout=\"@(id => Navigation.GoTo($\"workout/{id}?from=dashboard\"))\"")]
    [TestCase("OnOpenExercise=\"@(id => Navigation.GoTo($\"exercises/{id}?from=dashboard\"))\"")]
    public void Home_ShowsThisWeekAndOpensFromTheDashboard(string markup) =>
        Assert.That(
            WithoutWhitespace(RepoFiles.Read("src", "WorkoutLog.App", "Components", "Pages", "Home.razor")),
            Does.Contain(WithoutWhitespace(markup)));

    private static string WithoutWhitespace(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
}
