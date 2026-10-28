using System.Globalization;
using Reqnroll;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>The Exercises list, opened from the Dashboard, and the last sessions on the add-exercise list.</summary>
[Binding]
public sealed class ExercisesSteps(WebView web, Native native, Journeys journeys, ExercisesScreen exercises, AddExerciseScreen addExercise, DeviceClock clock)
{
    [Given(@"^I have opened Exercises$")]
    [When(@"^I open Exercises$")]
    public void OpenExercises() => journeys.GoToExercises();

    [Then(@"^Exercises shows ""(.*)""$")]
    public void ThenExercisesShows(string text) => web.Eventually(exercises.EmptyText, Is.EqualTo(text));

    [Then(@"^Exercises says ""(.*)"" under the search$")]
    public void ThenExercisesSaysNoMatch(string text) => web.Eventually(exercises.NoMatchText, Is.EqualTo(text));

    /// <summary>
    /// Rows with columns exercise, last and kind. Last is a day as seed steps write it, e.g. "3 days ago", then optionally
    /// " · " and the best.
    /// </summary>
    [Then(@"^the Exercises rows read, in order:$")]
    public void ThenTheRowsRead(DataTable table)
    {
        var now = clock.LocalNow();
        var rows = table.Rows.Select(r => (Name: r["exercise"], Last: Last(r["last"], now), Kind: r["kind"])).ToList();
        web.Eventually(exercises.Names, Is.EqualTo(rows.Select(r => r.Name)));
        Assert.Multiple(() =>
        {
            Assert.That(exercises.Lasts(), Is.EqualTo(rows.Select(r => r.Last)), "last sessions");
            Assert.That(exercises.Kinds(), Is.EqualTo(rows.Select(r => r.Kind)), "kinds");
            Assert.That(exercises.TappableNames(), Is.EqualTo(rows.Where(r => Exercises.Named(r.Name).Category == ExerciseCategory.Lift).Select(r => r.Name)), "tappable rows: the lifts");
        });
    }

    [Then(@"^Exercises lists, in order: (.+)$")]
    public void ThenExercisesLists(string names) => web.Eventually(exercises.Names, Is.EqualTo(names.Split(", ")));

    [When(@"^I search Exercises for ""(.*)""$")]
    public void WhenISearch(string query) => exercises.Search(query);

    [When(@"^I choose the (Strength|Cardio) chip on Exercises$")]
    public void WhenIChooseTheChip(string kind)
    {
        native.HideKeyboard();
        exercises.Filter(kind);
    }

    [Then(@"^the add-exercise row for ""(.*)"" reads Last (.+)$")]
    public void ThenTheAddExerciseRowReads(string name, string last) =>
        web.Eventually(() => addExercise.Last(Exercises.Named(name).Id), Is.EqualTo(Last(last, clock.LocalNow())));

    /// <summary>"Last" and the day, with the year only when it isn't this year, then the best if given.</summary>
    private static string Last(string text, DateTime now)
    {
        var parts = text.Split(" · ", 2);
        var day = SeedTime.ResolveDay(parts[0], now);
        var date = day.ToString(day.Year == now.Year ? "ddd d MMM" : "ddd d MMM yyyy", CultureInfo.InvariantCulture);
        return parts.Length == 2 ? $"Last {date} · {parts[1]}" : $"Last {date}";
    }
}
