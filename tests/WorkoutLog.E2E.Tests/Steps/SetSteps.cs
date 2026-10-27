using System.Globalization;
using System.Text.RegularExpressions;
using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>
/// Sets, entries and their fields: typing, Add set, remove, summaries, what a card shows and where focus is.
/// A subject is "Barbell Back Squat set 1", "Treadmill" (its entry or the exercise), or "set 1" for the exercise added last.
/// </summary>
[Binding]
public sealed partial class SetSteps(WebView web, Native native, WorkoutScreen workout, ScenarioState state)
{
    private const string NotAnExercise = "(?!the |History |This |reopening |I |.* is still open and shows )";

    [When(@"^I enter ""(.*)"" in (.+?)((?: and leave the field)?)$")]
    public void WhenIEnter(string text, string field, string leave)
    {
        Enter(FieldRef.Parse(field), text);
        if (leave.Length > 0)
        {
            web.Tap("screen-title");
            web.Settle();
        }
    }

    [Given(@"^I have entered ""(.*)"" in (.+)$")]
    public void GivenIHaveEntered(string text, string field)
    {
        Enter(FieldRef.Parse(field), text);
        native.HideKeyboard();
    }

    [When(@"^I clear (.+)$")]
    public void WhenIClear(string field) => Enter(FieldRef.Parse(field), "(empty)");

    [When(@"^I type ""(.*)"" into (.+)$")]
    public void WhenITypeInto(string text, string field)
    {
        var target = FieldRef.Parse(field);
        web.Type(TestId(target), text);
    }

    [When(@"^I tap (.+? (?:reps|weight|duration|distance|incline|level|calories|pace|floors|rate)(?: (?:hours|minutes|seconds))?)$")]
    public void WhenITapAField(string field) => web.Tap(TestId(FieldRef.Parse(field)));

    [When(@"^I tap Add set on (.+)$")]
    public void WhenITapAddSet(string name) => workout.AddSet(name);

    [Given(@"^I have tapped Add set on (.+)$")]
    public void GivenIHaveTappedAddSet(string name)
    {
        workout.AddSet(name);
        web.Settle();
        native.HideKeyboard();
    }

    [When(@"^I remove (?!every exercise$)(.+?)((?: set \d+)?)$")]
    public void WhenIRemove(string name, string set) => Remove(name, set);

    [Given(@"^I have removed (.+?)((?: set \d+)?)$")]
    public void GivenIHaveRemoved(string name, string set)
    {
        Remove(name, set);
        native.HideKeyboard();
    }

    [When(@"^I tap the summary of (.+) set (\d+)$")]
    public void WhenITapTheSummary(string name, int set) => workout.OpenSummary(name, set);

    [Then(@"^" + NotAnExercise + @"(.+?) shows ""(.*)""$")]
    public void ThenShows(string subject, string text)
    {
        if (FieldRef.TryParse(subject, out var field))
        {
            web.Eventually(() => ReadField(field!), Is.EqualTo(text), subject);
            return;
        }

        var (name, set) = Subject(subject);
        web.Eventually(() => Messages(name, set), Does.Contain(text), $"messages on {subject}");
    }

    [Then(@"^" + NotAnExercise + @"(.+?) shows:$")]
    public void ThenShowsAll(string subject, DataTable messages)
    {
        var (name, set) = Subject(subject);
        var expected = new[] { messages.Header.Single() }.Concat(messages.Rows.Select(r => r[0])).ToList();
        web.Eventually(() => Messages(name, set), Is.EqualTo(expected), $"messages on {subject}");
    }

    [Then(@"^" + NotAnExercise + @"(.+?) shows only ""(.*)""$")]
    public void ThenShowsOnly(string subject, string text)
    {
        var (name, set) = Subject(subject);
        web.Eventually(() => Messages(name, set), Is.EqualTo(new[] { text }), $"messages on {subject}");
    }

    [Then(@"^" + NotAnExercise + @"(.+?) shows no errors$")]
    public void ThenShowsNoErrors(string subject)
    {
        var (name, set) = Subject(subject);
        web.Eventually(() => Messages(name, set), Is.Empty, $"messages on {subject}");
    }

    [Then(@"^(.+) shows no exercise error$")]
    public void ThenShowsNoExerciseError(string name) => web.Eventually(() => workout.ExerciseError(name), Is.Null, name);

    [Then(@"^" + NotAnExercise + @"(.+?) (?:still )?shows (?!no |only |exactly |"")(.+)$")]
    public void ThenShowsValues(string subject, string values)
    {
        var (name, set) = Subject(subject);
        var expected = EntryValues.Parse(values).Values;
        web.Eventually(() => workout.ReadAll(name, set, expected.Keys), Is.EquivalentTo(expected), subject);
    }

    [Then(@"^set (\d+) shows exactly the fields: (.+)$")]
    public void ThenSetShowsTheFields(int set, string labels) =>
        web.Eventually(() => workout.Labels(LastAdded, set), Is.EqualTo(labels.Split(", ")));

    [Then(@"^the entry shows exactly the fields: (.+)$")]
    public void ThenTheEntryShowsTheFields(string labels) =>
        web.Eventually(() => workout.Labels(LastAdded, null), Is.EqualTo(labels.Split(", ")));

    [Then(@"^(.+) has (\d+) sets?((?: and it is empty)?)$")]
    public void ThenHasSets(string name, int count, string empty)
    {
        web.Eventually(() => workout.SetCount(name), Is.EqualTo(count), $"{name} sets");
        if (empty.Length > 0)
        {
            Assert.That(workout.ReadAll(name, 1, Exercises.Keys(Exercises.Named(name))).Values, Is.All.Empty, $"{name} set 1");
        }
    }

    [Then(@"^(.+) has its entry and it is empty$")]
    public void ThenHasItsEntryEmpty(string name) =>
        web.Eventually(() => workout.ReadAll(name, null, Exercises.Keys(Exercises.Named(name))).Values, Is.All.Empty, name);

    [Then(@"^(.+) sets are: (.+)$")]
    public void ThenSetsAre(string name, string sets)
    {
        var expected = EntryValues.ParseSets(sets);
        web.Eventually(() => workout.SetCount(name), Is.EqualTo(expected.Count), $"{name} sets");
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.That(workout.ReadAll(name, i + 1, expected[i].Values.Keys), Is.EquivalentTo(expected[i].Values), $"{name} set {i + 1}");
        }
    }

    [Then(@"^(.+) set (\d+) is open with labels: (.+)$")]
    public void ThenSetIsOpenWithLabels(string name, int set, string labels)
    {
        ThenSetIsOpen(name, set);
        Assert.That(workout.Labels(name, set), Is.EqualTo(labels.Split(", ")));
    }

    [Then(@"^(.+) set (\d+) is (?:still )?open$")]
    public void ThenSetIsOpen(string name, int set) =>
        web.Eventually(() => web.Exists($"{workout.Card(name)}-set-{set}") && !workout.IsSummary(name, set), Is.True, $"{name} set {set} open");

    [Then(@"^(.+) set (\d+) is still open and shows ""(.*)""$")]
    public void ThenSetIsStillOpenAndShows(string name, int set, string text)
    {
        web.Settle();
        ThenSetIsOpen(name, set);
        web.Eventually(() => workout.Errors(name, set), Does.Contain(text));
    }

    [Then(@"^(.+) set (\d+) is a summary ""(.*)""$")]
    public void ThenSetIsASummary(string name, int set, string text)
    {
        web.Eventually(() => workout.IsSummary(name, set), Is.True, $"{name} set {set} is a summary");
        Assert.That(workout.SummaryText(name, set), Is.EqualTo(text));
    }

    [Then(@"^every (.+) set is a summary$")]
    public void ThenEverySetIsASummary(string name)
    {
        web.Settle();
        var count = workout.SetCount(name);
        Assert.That(count, Is.GreaterThan(0), $"{name} sets");
        Assert.That(Enumerable.Range(1, count).Select(set => workout.IsSummary(name, set)), Is.All.True, $"{name} sets are summaries");
    }

    [Then(@"^(.+) has no Add set and its entry can't be removed on its own$")]
    public void ThenCardioHasOneEntry(string name) => Assert.Multiple(() =>
    {
        Assert.That(workout.HasAddSet(name), Is.False, "Add set");
        Assert.That(web.FindAll($"[data-testid^=\"{workout.Card(name)}-set-\"]"), Is.Empty, "sets");
        Assert.That(web.FindAll($"[data-testid=\"{workout.Card(name)}\"] [data-testid$=\"-remove\"]:not([data-testid=\"{workout.Card(name)}-remove\"])"), Is.Empty, "a remove button on the entry");
    });

    [Then(@"^(.+) can be removed as an exercise$")]
    public void ThenCanBeRemoved(string name) => Assert.That(workout.HasRemove(name), Is.True);

    [Then(@"^focus is on (.+)$")]
    public void ThenFocusIsOn(string target) => web.Eventually(web.Focused, Is.EqualTo(FocusTarget(target)), "focused element");

    [Then(@"^the focused field is in view$")]
    public void ThenTheFocusedFieldIsInView() => web.Eventually(web.FocusedInView, Is.True, "focused field in view and uncovered");

    private string LastAdded => state.LastAdded ?? throw new InvalidOperationException("No exercise was added.");

    private void Enter(FieldRef field, string text)
    {
        if (field.Box is not null)
        {
            throw new ArgumentException($"Enter a whole field, not its {field.Box} box; type into a box instead.", nameof(field));
        }

        workout.Enter(field.Name, field.Set, field.Key, text);
        if (text != "(empty)")
        {
            state.RecordLogged(field.Name, field.Set ?? 1, new Dictionary<string, string> { [field.Key] = text });
        }
    }

    private void Remove(string name, string set)
    {
        if (set.Length > 0)
        {
            var number = int.Parse(set[" set ".Length..], CultureInfo.InvariantCulture);
            var count = workout.SetCount(name);
            workout.RemoveSet(name, number);
            web.Eventually(() => workout.SetCount(name), Is.EqualTo(count - 1), $"{name} sets");
        }
        else
        {
            workout.Remove(name);
            web.WaitUntil(() => !workout.Has(name), $"{name} wasn't removed");
        }
    }

    /// <summary>The field's text: one box when <see cref="FieldRef.Box"/> is given, otherwise as <see cref="WorkoutScreen.Read"/>.</summary>
    private string ReadField(FieldRef field) =>
        field.Box is null ? workout.Read(field.Name, field.Set, field.Key) : web.Value(TestId(field));

    /// <summary>The field's input, or its first box for a duration or pace.</summary>
    private string TestId(FieldRef field)
    {
        var testId = workout.Field(field.Name, field.Set, field.Key);
        var box = field.Box ?? field.Key switch
        {
            "duration" => "h",
            "pace" => "m",
            _ => null,
        };

        return box is null ? testId : $"{testId}-{box}";
    }

    /// <summary>Messages on a set, or on an exercise: its exercise error and its cardio entry's messages.</summary>
    private IReadOnlyList<string> Messages(string name, int? set) =>
        set is not null
            ? workout.Errors(name, set)
            : [.. workout.ExerciseError(name) is { } error ? [error] : Array.Empty<string>(), .. workout.Errors(name, null)];

    private (string Name, int? Set) Subject(string subject)
    {
        var match = SubjectPattern().Match(subject);
        var name = match.Groups["name"].Value;
        int? set = match.Groups["set"].Success ? int.Parse(match.Groups["set"].Value, CultureInfo.InvariantCulture) : null;
        return (name.Length == 0 ? LastAdded : name, set);
    }

    private string FocusTarget(string target) => target switch
    {
        "Add exercise" => "add-exercise",
        "Cancel" => "confirm-dialog-cancel",
        _ when target.EndsWith(" Add set", StringComparison.Ordinal) => $"{workout.Card(target[..^" Add set".Length])}-add-set",
        _ => TestId(FieldRef.Parse(target)),
    };

    [GeneratedRegex(@"^(?:set (?<set>\d+)|(?<name>.+?)(?: set (?<set>\d+))?)$")]
    private static partial Regex SubjectPattern();
}
