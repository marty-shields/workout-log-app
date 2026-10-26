using System.Globalization;
using Reqnroll;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Opening a saved workout and checking what it holds, now or after reopening it.</summary>
[Binding]
public sealed class EditSteps(WebView web, Journeys journeys, WorkoutScreen workout, DeviceClock clock, ScenarioState state)
{
    [Given(@"^I have opened workout (\d+) from History$")]
    [When(@"^I open workout (\d+) from History$")]
    public void OpenWorkout(int id) => journeys.OpenWorkout(id);

    [When(@"^I open the latest workout from History$")]
    public void WhenIOpenTheLatestWorkout() => journeys.OpenLatestWorkout();

    [When(@"^I open the latest workout from History and save it$")]
    public void WhenIOpenTheLatestWorkoutAndSaveIt()
    {
        journeys.OpenLatestWorkout();
        journeys.SaveAndWait();
    }

    [Then(@"^the title is yesterday's date and day$")]
    public void ThenTheTitleIsYesterday() =>
        web.Eventually(workout.Title, Is.EqualTo(DashboardSteps.Date(clock.LocalNow().Date.AddDays(-1))));

    [Then(@"^the time shows ""(.*)"" and doesn't count up$")]
    public void ThenTheTimeShowsAndDoesNotCountUp(string text)
    {
        web.Eventually(workout.Subtitle, Is.EqualTo(text));
        Thread.Sleep(TimeSpan.FromSeconds(1.5));
        Assert.That(workout.Subtitle(), Is.EqualTo(text), "after 1.5 s");
    }

    [Then(@"^the time still shows ""(.*)""$")]
    public void ThenTheTimeStillShows(string text) => web.Eventually(workout.Subtitle, Is.EqualTo(text));

    [Then(@"^the total time is at least (.+) and no more than the time I took$")]
    public void ThenTheTotalTimeIsBetween(string least)
    {
        var (from, _) = state.Started ?? throw new InvalidOperationException("No workout was started.");
        var took = clock.LocalNow() - from;
        var text = workout.Subtitle();
        Assert.That(text, Does.StartWith("Total "));
        var total = TimeSpan.ParseExact(text["Total ".Length..], @"h\:mm\:ss", CultureInfo.InvariantCulture);
        Assert.That(total, Is.InRange(TimeSpan.ParseExact(least, @"h\:mm\:ss", CultureInfo.InvariantCulture), took));
    }

    [Then(@"^Delete is shown$")]
    public void ThenDeleteIsShown() => Assert.That(workout.IsEdit(), Is.True);

    [Then(@"^it holds exactly what was saved, newest first$")]
    public void ThenItHoldsWhatWasSaved() => AssertHolds(state.SeededExercises[1].Reverse().ToList());

    [Then(@"^it holds exactly what I logged, newest first$")]
    public void ThenItHoldsWhatILogged() =>
        AssertHolds(state.Logged.Select(e => new ExerciseValues(e.Name, e.Entries.Select(v => new EntryValues(v)).ToList())).Reverse().ToList());

    [Then(@"^reopening workout (\d+) shows: (.+)$")]
    public void ThenReopeningShowsExercises(int id, string names)
    {
        Reopen(id);
        Assert.That(workout.ExerciseNames(), Is.EqualTo(names.Split(", ")));
    }

    [Then(@"^reopening workout (\d+) shows (\d+) (.+) sets$")]
    public void ThenReopeningShowsSetCount(int id, int count, string name)
    {
        Reopen(id);
        Assert.That(workout.SetCount(name), Is.EqualTo(count), $"{name} sets");
    }

    [Then(@"^reopening workout (\d+) shows (.+) sets: (.+)$")]
    public void ThenReopeningShowsSets(int id, string name, string sets)
    {
        Reopen(id);
        AssertEntries(name, EntryValues.ParseSets(sets));
    }

    [Then(@"^reopening (workout \d+|the latest workout) shows (.+?)((?: set \d+)?) at (.+)$")]
    public void ThenReopeningShowsValues(string which, string name, string set, string values)
    {
        Reopen(which.StartsWith("workout ", StringComparison.Ordinal) ? int.Parse(which["workout ".Length..], CultureInfo.InvariantCulture) : null);
        int? number = set.Length > 0 ? int.Parse(set[" set ".Length..], CultureInfo.InvariantCulture) : null;
        var expected = EntryValues.Parse(values).Values;
        Assert.That(workout.ReadAll(name, number, expected.Keys), Is.EquivalentTo(expected), name);
    }

    /// <summary>Opens the workout from History: by id, or the top row when <paramref name="id"/> is null.</summary>
    private void Reopen(int? id)
    {
        if (id is null)
        {
            journeys.OpenLatestWorkout();
        }
        else
        {
            journeys.OpenWorkout(id.Value);
        }
    }

    /// <summary>The exercises in order, each with exactly these sets or entry. Closed carry sets are checked by their summary line.</summary>
    private void AssertHolds(IReadOnlyList<ExerciseValues> expected)
    {
        Assert.That(workout.ExerciseNames(), Is.EqualTo(expected.Select(e => e.Name)), "exercises");
        foreach (var exercise in expected)
        {
            AssertEntries(exercise.Name, exercise.Entries);
        }
    }

    private void AssertEntries(string name, IReadOnlyList<EntryValues> entries)
    {
        var definition = Exercises.Named(name);
        var keys = Exercises.Keys(definition);
        if (definition.Type == ExerciseType.Cardio)
        {
            Assert.That(workout.ReadAll(name, null, keys), Is.EquivalentTo(Filled(keys, entries.Single())), name);
            return;
        }

        Assert.That(workout.SetCount(name), Is.EqualTo(entries.Count), $"{name} sets");
        for (var i = 0; i < entries.Count; i++)
        {
            var set = i + 1;
            if (workout.IsSummary(name, set))
            {
                Assert.That(workout.SummaryText(name, set), Is.EqualTo(Summary(definition, entries[i])), $"{name} set {set}");
            }
            else
            {
                Assert.That(workout.ReadAll(name, set, keys), Is.EquivalentTo(Filled(keys, entries[i])), $"{name} set {set}");
            }
        }
    }

    /// <summary>Every field's expected text: the value given, or empty.</summary>
    private static Dictionary<string, string> Filled(IEnumerable<string> keys, EntryValues entry) =>
        keys.ToDictionary(k => k, k => entry.Values.GetValueOrDefault(k, ""));

    /// <summary>A closed set's summary line, e.g. 40 m · 32 kg · 0:00:45.</summary>
    private static string Summary(ExerciseDefinition definition, EntryValues entry) =>
        string.Join(" · ", Exercises.Keys(definition).Where(entry.Values.ContainsKey).Select(key =>
        {
            var value = entry.Values[key];
            return key switch
            {
                "duration" or "pace" => value,
                "weight" => $"{value} kg",
                "distance" => $"{value} {DistanceUnit(definition)}",
                _ => $"{value} {key}",
            };
        }));

    private static string DistanceUnit(ExerciseDefinition definition) => definition.DistanceUnit switch
    {
        Core.Domain.Exercises.DistanceUnit.Km => "km",
        Core.Domain.Exercises.DistanceUnit.Metres => "m",
        _ => "ft",
    };
}
