using Reqnroll;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class AddExerciseSteps(WebView web, Native native, WorkoutScreen workout, AddExerciseScreen list)
{
    [When(@"^I open the add-exercise list$")]
    public void WhenIOpenTheList() => Open();

    [Given(@"^I have opened the add-exercise list$")]
    public void GivenIHaveOpenedTheList()
    {
        Open();
        native.HideKeyboard();
    }

    [When(@"^I open the add-exercise list and close it$")]
    public void WhenIOpenAndCloseTheList()
    {
        Open();
        list.Back();
        ThenTheListIsClosed();
    }

    [When(@"^I search the add-exercise list for ""(.*)""$")]
    public void WhenISearch(string query) => Search(query);

    [Given(@"^I have searched the add-exercise list for ""(.*)""$")]
    public void GivenIHaveSearched(string query)
    {
        Search(query);
        native.HideKeyboard();
    }

    [Given(@"^I have chosen the (Strength|Cardio) chip$")]
    [When(@"^I choose the (Strength|Cardio) chip$")]
    public void WhenIChooseTheChip(string kind)
    {
        if (!list.IsLoaded())
        {
            Open();
        }

        native.HideKeyboard();
        list.Filter(kind);
    }

    [When(@"^I type (\d+) characters into the add-exercise search$")]
    public void WhenITypeCharacters(int count) => Search(Characters(count));

    [When(@"^I close the add-exercise list with (its back arrow|Android back)$")]
    public void WhenICloseTheList(string how)
    {
        if (how == "Android back")
        {
            native.BackKey();
        }
        else
        {
            list.Back();
        }
    }

    [Then(@"^the search box is empty and focused$")]
    public void ThenTheSearchBoxIsEmptyAndFocused()
    {
        web.Eventually(web.Focused, Is.EqualTo("exercises-search"), "focused element");
        Assert.That(list.Query(), Is.Empty);
    }

    [Then(@"^every exercise shows, Strength then Cardio, A–Z in each$")]
    public void ThenEveryExerciseShowsStrengthThenCardio()
    {
        var expected = Exercises.All.OrderBy(e => e.Type).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).Select(e => e.Name);
        web.Eventually(list.Names, Is.EqualTo(expected));
    }

    [Then(@"^only (Strength|Cardio) exercises show, A–Z$")]
    public void ThenOnlyKindShows(string kind)
    {
        var expected = Exercises.All.Where(e => e.Type.ToString() == kind).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).Select(e => e.Name);
        web.Eventually(list.Names, Is.EqualTo(expected));
    }

    [Then(@"^each row shows its kind$")]
    public void ThenEachRowShowsItsKind()
    {
        var kinds = list.Names().Select(n => Exercises.Named(n).Type.ToString());
        Assert.That(list.Kinds(), Is.EqualTo(kinds));
    }

    [Then(@"^the results are only (Strength|Cardio) exercises and include ""(.*)""$")]
    public void ThenTheResultsAreOnlyKind(string kind, string name)
    {
        web.Eventually(list.Names, Does.Contain(name));
        Assert.That(list.Names().Select(n => Exercises.Named(n).Type.ToString()), Is.All.EqualTo(kind));
    }

    [Then(@"^the results include ""(.*)""$")]
    public void ThenTheResultsInclude(string name) => web.Eventually(list.Names, Does.Contain(name));

    [Then(@"^the first result is ""(.*)""$")]
    public void ThenTheFirstResultIs(string name) => web.Eventually(() => list.Names().FirstOrDefault(), Is.EqualTo(name));

    [Then(@"^the add-exercise list shows ""(.*)""$")]
    public void ThenTheListShows(string text) => web.Eventually(list.NoMatchText, Is.EqualTo(text));

    [Then(@"^the search box holds the first (\d+)$")]
    public void ThenTheSearchBoxHolds(int count) => web.Eventually(list.Query, Is.EqualTo(Characters(60)[..count]));

    [Then(@"^the search box is empty, the All chip is on and every exercise shows$")]
    public void ThenTheSearchBoxIsEmptyAndEveryExerciseShows()
    {
        web.Eventually(list.IsLoaded, Is.True, "list loaded");
        Assert.That(list.Query(), Is.Empty);
        Assert.That(web.Find("exercises-filter-all").Selected, Is.True, "All chip");
        web.Eventually(() => list.Names().Count, Is.EqualTo(Exercises.All.Count));
    }

    [Then(@"^the add-exercise row for ""(.*)"" has no last session$")]
    public void ThenTheRowShowsNoLast(string name)
    {
        web.Eventually(list.IsLoaded, Is.True, "list loaded");
        Assert.That(list.Names(), Does.Contain(name), "row shows");
        Assert.That(list.Last(Exercises.Named(name).Id), Is.Null);
    }

    [Then(@"^the add-exercise list is closed$")]
    public void ThenTheListIsClosed() => web.Eventually(list.IsOpen, Is.False, "add-exercise list open");

    [Then(@"^the add-exercise list is still open$")]
    public void ThenTheListIsStillOpen()
    {
        web.Settle();
        Assert.That(list.IsOpen(), Is.True, "add-exercise list open");
    }

    [Then(@"^the add-exercise list is closed on (New Workout|Edit Workout)$")]
    public void ThenTheListIsClosedOn(string screen)
    {
        ThenTheListIsClosed();
        Assert.That(workout.IsEdit(), Is.EqualTo(screen == "Edit Workout"), screen);
    }

    private void Open()
    {
        workout.OpenAddExercise();
        web.WaitUntil(list.IsLoaded, "the add-exercise list didn't open");
    }

    private void Search(string query)
    {
        if (!list.IsLoaded())
        {
            Open();
        }

        list.Search(query);
    }

    /// <summary>Letters a to z repeated, <paramref name="count"/> long.</summary>
    private static string Characters(int count) => new(Enumerable.Range(0, count).Select(i => (char)('a' + i % 26)).ToArray());
}
