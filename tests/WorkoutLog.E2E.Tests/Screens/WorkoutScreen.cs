using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>New and Edit Workout: the exercise cards, their sets or entry, and the top and bottom bars.</summary>
public sealed partial class WorkoutScreen(WebView web, Native native)
{
    public string Title() => web.Text("screen-title");

    public string Subtitle() => web.Text("screen-subtitle");

    public bool IsShown() => web.Exists("save-workout");

    public bool IsEdit() => web.Exists("delete-workout");

    public string? Error() => web.Exists("workout-error") ? web.Text("workout-error") : null;

    public bool ShowsNoExercises() => web.Exists("no-exercises");

    /// <summary>Exercise names on the cards, in order.</summary>
    public IReadOnlyList<string> ExerciseNames() => Cards().Select(c => c.Name).ToList();

    /// <summary>The <c>data-testid</c> of the card for <paramref name="name"/>, e.g. exercise-2.</summary>
    public string Card(string name)
    {
        string? card = null;
        web.WaitUntil(() => (card = Cards().FirstOrDefault(c => c.Name == name).TestId) is not null, $"{name} isn't in the workout");
        return card!;
    }

    public bool Has(string name) => Cards().Any(c => c.Name == name);

    /// <summary>Sets on a strength card: rows, open sets and summaries.</summary>
    public int SetCount(string name) =>
        web.FindAll($"[data-testid^=\"{Card(name)}-set-\"]").Count(e => SetId().IsMatch(e.GetDomAttribute("data-testid") ?? ""));

    /// <summary>Whether the set is shown as a summary line.</summary>
    public bool IsSummary(string name, int set) => web.Exists($"{Card(name)}-set-{set}-open");

    public string SummaryText(string name, int set) =>
        web.Find($"{Card(name)}-set-{set}-open").FindElement(OpenQA.Selenium.By.CssSelector(".text")).Text.Trim();

    /// <summary>The <c>data-testid</c> of a field: a set's (<paramref name="set"/> given) or a cardio entry's.</summary>
    public string Field(string name, int? set, string key) => set is { } number ? $"{Card(name)}-set-{number}-{key}" : $"{Card(name)}-{key}";

    /// <summary>Messages under a set (<paramref name="set"/> given) or a cardio entry.</summary>
    public IReadOnlyList<string> Errors(string name, int? set)
    {
        var testId = set is { } number ? $"{Card(name)}-set-{number}-error" : $"{Card(name)}-entry-error";
        return web.FindAll($"[data-testid=\"{testId}\"] .error").Select(e => e.Text.Trim()).ToList();
    }

    public string? ExerciseError(string name) => web.Exists($"{Card(name)}-error") ? web.Text($"{Card(name)}-error") : null;

    /// <summary>Field labels of a set (<paramref name="set"/> given) or a cardio entry, in order.</summary>
    public IReadOnlyList<string> Labels(string name, int? set)
    {
        var root = set is { } number ? $"{Card(name)}-set-{number}" : Card(name);
        var script = $$"""
            const root = document.querySelector('[data-testid="{{root}}"]');
            return [...root.querySelectorAll('label.field, div.field[role=group]')].map(f =>
                f.getAttribute('role') === 'group' ? f.getAttribute('aria-label') : f.querySelector('.label').childNodes[0].textContent.trim());
            """;
        return web.Strings(script);
    }

    /// <summary>The field's text as the user sees it: h:mm:ss or m:ss for a duration or pace, empty when empty.</summary>
    public string Read(string name, int? set, string key)
    {
        var field = Field(name, set, key);
        if (key is "duration" or "pace")
        {
            var parts = (key == "duration" ? ["h", "mm", "ss"] : new[] { "m", "ss" }).Select(p => web.Value($"{field}-{p}")).ToArray();
            return parts.All(string.IsNullOrEmpty) ? "" : string.Join(':', parts);
        }

        return web.Value(field);
    }

    /// <summary>Values of every field given, read as <see cref="Read"/> does.</summary>
    public Dictionary<string, string> ReadAll(string name, int? set, IEnumerable<string> keys) => keys.ToDictionary(k => k, k => Read(name, set, k));

    /// <summary>
    /// Types into a field. A duration or pace is written h:mm:ss or m:ss, each part typed into its box and an empty part left
    /// empty, e.g. "1::" or ":5:"; "(empty)" or "" empties every box.
    /// </summary>
    public void Enter(string name, int? set, string key, string text)
    {
        var field = Field(name, set, key);
        if (key is "duration" or "pace")
        {
            var boxes = key == "duration" ? new[] { "h", "mm", "ss" } : ["m", "ss"];
            var parts = text is "(empty)" or "" ? new string[boxes.Length] : text.Split(':');
            for (var i = 0; i < boxes.Length; i++)
            {
                var part = i < parts.Length ? parts[i] ?? "" : "";
                if (part.Length == 0)
                {
                    web.Clear($"{field}-{boxes[i]}");
                }
                else
                {
                    web.Type($"{field}-{boxes[i]}", part);
                }
            }

            return;
        }

        if (text is "(empty)" or "")
        {
            web.Clear(field);
        }
        else
        {
            web.Type(field, text);
        }
    }

    /// <summary>Types each value given into a set or entry.</summary>
    public void EnterAll(string name, int? set, IReadOnlyDictionary<string, string> values)
    {
        foreach (var (key, value) in values)
        {
            Enter(name, set, key, value);
        }
    }

    public void AddSet(string name) => TapButton($"{Card(name)}-add-set");

    public bool HasAddSet(string name) => web.Exists($"{Card(name)}-add-set");

    public void RemoveSet(string name, int set) => TapButton($"{Card(name)}-set-{set}-remove");

    public void OpenSummary(string name, int set) => TapButton($"{Card(name)}-set-{set}-open");

    public void Remove(string name) => TapButton($"{Card(name)}-remove");

    public bool HasRemove(string name) => web.Exists($"{Card(name)}-remove");

    public void OpenPicker() => TapButton("add-exercise");

    public void Save() => TapButton("save-workout");

    public void Back() => web.Tap("back");

    public void Delete() => web.Tap("delete-workout");

    /// <summary>The field's <c>inputmode</c>: the keypad it asks Android for.</summary>
    public string InputMode(string testId) => web.Attribute(testId, "inputmode");

    /// <summary>Whether this exercise is cardio.</summary>
    public static bool IsCardio(string name) => Exercises.Named(name).Type == ExerciseType.Cardio;

    /// <summary>Hides the soft keyboard, then taps the button.</summary>
    private void TapButton(string testId)
    {
        native.HideKeyboard();
        web.Tap(testId);
    }

    private IEnumerable<(string TestId, string Name)> Cards()
    {
        const string script = """
            return [...document.querySelectorAll('section.card[data-testid^="exercise-"]')]
                .map(s => [s.getAttribute('data-testid'), s.querySelector('.name').textContent.trim()]);
            """;
        var cards = web.List(script);
        return cards.Cast<ReadOnlyCollection<object>>().Select(c => ((string)c[0], (string)c[1]));
    }

    [GeneratedRegex(@"-set-\d+$")]
    private static partial Regex SetId();
}
