using Reqnroll;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

[Binding]
public sealed class PickerSteps(WebView web, Native native, WorkoutScreen workout, PickerScreen picker)
{
    [When(@"^I open the exercise picker$")]
    public void WhenIOpenThePicker() => Open();

    [Given(@"^I have opened the exercise picker$")]
    public void GivenIHaveOpenedThePicker()
    {
        Open();
        native.HideKeyboard();
    }

    [When(@"^I open the exercise picker and close it$")]
    public void WhenIOpenAndCloseThePicker()
    {
        Open();
        picker.Back();
        ThenThePickerIsClosed();
    }

    [When(@"^I search the picker for ""(.*)""$")]
    public void WhenISearch(string query) => Search(query);

    [Given(@"^I have searched the picker for ""(.*)""$")]
    public void GivenIHaveSearched(string query)
    {
        Search(query);
        native.HideKeyboard();
    }

    [When(@"^I type (\d+) characters into the picker search$")]
    public void WhenITypeCharacters(int count) => Search(Characters(count));

    [When(@"^I close the picker with (the picker back arrow|Android back)$")]
    public void WhenICloseThePicker(string how)
    {
        if (how == "Android back")
        {
            native.BackKey();
        }
        else
        {
            picker.Back();
        }
    }

    [Then(@"^the search box is empty and focused$")]
    public void ThenTheSearchBoxIsEmptyAndFocused()
    {
        web.Eventually(web.Focused, Is.EqualTo("picker-search"), "focused element");
        Assert.That(picker.Query(), Is.Empty);
    }

    [Then(@"^the (Strength|Cardio) group lists (\d+) exercises A–Z((?:, after Strength)?)$")]
    public void ThenTheGroupLists(string kind, int count, string afterStrength)
    {
        var names = picker.Group(kind);
        Assert.That(names, Has.Count.EqualTo(count), $"{kind} exercises");
        Assert.That(names, Is.Ordered.Using((IComparer<string>)StringComparer.OrdinalIgnoreCase), $"{kind} A–Z");
        var expected = Exercises.All.Where(e => e.Type.ToString() == kind).Select(e => e.Name);
        Assert.That(names, Is.EquivalentTo(expected), $"{kind} exercises");
        if (afterStrength.Length > 0)
        {
            Assert.That(picker.Groups(), Is.EqualTo(new[] { nameof(ExerciseType.Strength), nameof(ExerciseType.Cardio) }), "groups");
        }
    }

    [Then(@"^the results include ""(.*)""$")]
    public void ThenTheResultsInclude(string name) => web.Eventually(picker.Names, Does.Contain(name));

    [Then(@"^the first result is ""(.*)""$")]
    public void ThenTheFirstResultIs(string name) => web.Eventually(() => picker.Names().FirstOrDefault(), Is.EqualTo(name));

    [Then(@"^the picker shows ""(.*)""$")]
    public void ThenThePickerShows(string text) => web.Eventually(picker.NoMatchText, Is.EqualTo(text));

    [Then(@"^the search box holds the first (\d+)$")]
    public void ThenTheSearchBoxHolds(int count) => web.Eventually(picker.Query, Is.EqualTo(Characters(60)[..count]));

    [Then(@"^the search box is empty and every exercise shows$")]
    public void ThenTheSearchBoxIsEmptyAndEveryExerciseShows()
    {
        Assert.That(picker.Query(), Is.Empty);
        web.Eventually(() => picker.Names().Count, Is.EqualTo(Exercises.All.Count));
    }

    [Then(@"^the picker is closed$")]
    public void ThenThePickerIsClosed() => web.Eventually(picker.IsOpen, Is.False, "picker open");

    [Then(@"^the picker is still open$")]
    public void ThenThePickerIsStillOpen()
    {
        web.Settle();
        Assert.That(picker.IsOpen(), Is.True, "picker open");
    }

    [Then(@"^the picker is closed on (New Workout|Edit Workout)$")]
    public void ThenThePickerIsClosedOn(string screen)
    {
        ThenThePickerIsClosed();
        Assert.That(workout.IsEdit(), Is.EqualTo(screen == "Edit Workout"), screen);
    }

    private void Open()
    {
        workout.OpenPicker();
        web.WaitUntil(picker.IsOpen, "the picker didn't open");
    }

    private void Search(string query)
    {
        if (!picker.IsOpen())
        {
            Open();
        }

        picker.Search(query);
    }

    /// <summary>Letters a to z repeated, <paramref name="count"/> long.</summary>
    private static string Characters(int count) => new(Enumerable.Range(0, count).Select(i => (char)('a' + i % 26)).ToArray());
}
