using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Screens;

/// <summary>New and Edit Workout: the exercise cards, their sets or entry, and the top and bottom bars.</summary>
public sealed partial class WorkoutScreen(WebView web, Native native)
{
    private static readonly string[] DurationBoxes = ["h", "mm", "ss"];

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

    /// <summary>Sets on a strength card: rows, open sets and summaries. A done card is opened first.</summary>
    public int SetCount(string name)
    {
        Open(name);
        return web.FindAll($"[data-testid^=\"{Card(name)}-set-\"]").Count(e => SetId().IsMatch(e.GetDomAttribute("data-testid") ?? ""));
    }

    /// <summary>Whether the set is shown as a summary line. A done card is opened first.</summary>
    public bool IsSummary(string name, int set)
    {
        Open(name);
        return web.Exists($"{Card(name)}-set-{set}-open");
    }

    public string SummaryText(string name, int set) =>
        web.Find($"{Card(name)}-set-{set}-open").FindElement(OpenQA.Selenium.By.CssSelector(".text")).Text.Trim();

    /// <summary>Whether the card is the one-line done row: it has the pencil and no tick.</summary>
    public bool IsDone(string name) => web.Exists($"{Card(name)}-edit");

    /// <summary>The done row holds the name, the pencil and the trash, and no sets, entry or Add set.</summary>
    public bool IsOneLine(string name) =>
        IsDone(name) && web.Exists($"{Card(name)}-remove") && !web.Exists($"{Card(name)}-done") && !web.Exists($"{Card(name)}-add-set")
        && web.FindAll($"[data-testid=\"{Card(name)}\"] input, [data-testid=\"{Card(name)}\"] [data-testid*=\"-set-\"]").Count == 0;

    /// <summary>Taps the tick; the card shows as one line if it passes the checks, otherwise it stays open.</summary>
    public void Done(string name) => TapButton($"{Card(name)}-done");

    /// <summary>Taps the pencil of a done card.</summary>
    public void Edit(string name) => TapButton($"{Card(name)}-edit");

    /// <summary>As a user does before changing a done card, e.g. on Edit Workout: taps its pencil. Does nothing for an open card.</summary>
    public void Open(string name)
    {
        if (!IsDone(name))
        {
            return;
        }

        Edit(name);
        web.WaitUntil(() => !IsDone(name), $"{name} didn't open");
    }

    /// <summary>The <c>data-testid</c> of a field: a set's (<paramref name="set"/> given) or a cardio entry's.</summary>
    public string Field(string name, int? set, string key) => set is { } number ? $"{Card(name)}-set-{number}-{key}" : $"{Card(name)}-{key}";

    /// <summary>Messages under a set (<paramref name="set"/> given) or a cardio entry.</summary>
    public IReadOnlyList<string> Errors(string name, int? set)
    {
        var testId = set is { } number ? $"{Card(name)}-set-{number}-error" : $"{Card(name)}-entry-error";
        return web.FindAll($"[data-testid=\"{testId}\"] .error").Select(e => e.Text.Trim()).ToList();
    }

    /// <summary>The card's last and PB line as its text content, e.g. "Last 60kg · PB 70kg", or null when it has none.</summary>
    public string? Weights(string name) =>
        web.Exists($"{Card(name)}-weights") ? web.Find($"{Card(name)}-weights").GetDomProperty("textContent")?.Trim() : null;

    /// <summary>
    /// The text of last time's set (<paramref name="set"/> given) or cardio entry without the "Last time" label, e.g. 60×8; null when it shows none.
    /// </summary>
    public string? LastTime(string name, int? set)
    {
        var testId = set is { } number ? $"{Card(name)}-set-{number}-last" : $"{Card(name)}-last";
        var text = web.Exists(testId) ? web.Find(testId).GetDomProperty("textContent")?.Trim() : null;
        return text is not null && text.StartsWith("Last time ", StringComparison.Ordinal) ? text["Last time ".Length..] : text;
    }

    /// <summary>Whether the card shows last time's text on any set or above its entry.</summary>
    public bool HasLastTime(string name) => web.FindAll($"[data-testid^=\"{Card(name)}-\"][data-testid$=\"-last\"]").Count > 0;

    /// <summary>The cardio entry's worked-out pace or speed line, e.g. Pace 5:46/km; null when the card has none.</summary>
    public string? WorkedOut(string name) => web.Exists($"{Card(name)}-worked-out") ? web.Text($"{Card(name)}-worked-out") : null;

    /// <summary>The note under Add set when the exercise has 10 sets; null when there is none.</summary>
    public string? SetsFull(string name) => web.Exists($"{Card(name)}-sets-full") ? web.Text($"{Card(name)}-sets-full") : null;

    public bool AddSetEnabled(string name) => web.Find($"{Card(name)}-add-set").Enabled;

    public bool AddExerciseEnabled() => web.Find("add-exercise").Enabled;

    /// <summary>The note under the exercises when the workout has 10; null when there is none.</summary>
    public string? ExercisesFull() => web.Exists("exercises-full") ? web.Text("exercises-full") : null;

    public string? ExerciseError(string name) => web.Exists($"{Card(name)}-error") ? web.Text($"{Card(name)}-error") : null;

    /// <summary>Field labels of a set (<paramref name="set"/> given) or a cardio entry, in order.</summary>
    public IReadOnlyList<string> Labels(string name, int? set)
    {
        Open(name);
        var root = set is { } number ? $"{Card(name)}-set-{number}" : Card(name);
        var script = $$"""
            const root = document.querySelector('[data-testid="{{root}}"]');
            return [...root.querySelectorAll('label.field, div.field[role=group]')].map(f =>
                f.getAttribute('role') === 'group' ? f.getAttribute('aria-label') : f.querySelector('.label').childNodes[0].textContent.trim());
            """;
        return web.Strings(script);
    }

    /// <summary>The field's text as the user sees it: h:mm:ss for a duration, empty when empty. A done card is opened first.</summary>
    public string Read(string name, int? set, string key)
    {
        Open(name);
        var field = Field(name, set, key);
        if (key == "duration")
        {
            var parts = DurationBoxes.Select(p => web.Value($"{field}-{p}")).ToArray();
            return parts.All(string.IsNullOrEmpty) ? "" : string.Join(':', parts);
        }

        return web.Value(field);
    }

    /// <summary>Values of every field given, read as <see cref="Read"/> does.</summary>
    public Dictionary<string, string> ReadAll(string name, int? set, IEnumerable<string> keys) => keys.ToDictionary(k => k, k => Read(name, set, k));

    /// <summary>
    /// Types into a field. A duration is written h:mm:ss, each part typed into its box and an empty part left
    /// empty, e.g. "1::" or ":5:"; "(empty)" or "" empties every box.
    /// </summary>
    public void Enter(string name, int? set, string key, string text)
    {
        Open(name);
        var field = Field(name, set, key);
        if (key == "duration")
        {
            var boxes = DurationBoxes;
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

    public void AddSet(string name)
    {
        Open(name);
        TapButton($"{Card(name)}-add-set");
    }

    public bool HasAddSet(string name) => web.Exists($"{Card(name)}-add-set");

    public void RemoveSet(string name, int set)
    {
        Open(name);
        TapButton($"{Card(name)}-set-{set}-remove");
    }

    public void OpenSummary(string name, int set)
    {
        Open(name);
        TapButton($"{Card(name)}-set-{set}-open");
    }

    /// <summary>Taps the trash, open or done; the "Delete {name}?" prompt opens.</summary>
    public void AskRemove(string name) => TapButton($"{Card(name)}-remove");

    /// <summary>Taps the trash and confirms the "Delete {name}?" prompt.</summary>
    public void Remove(string name)
    {
        AskRemove(name);
        web.WaitUntil(() => web.Exists("confirm-dialog-confirm"), $"The delete prompt for {name} didn't show");
        web.Tap("confirm-dialog-confirm");
    }

    public bool HasRemove(string name) => web.Exists($"{Card(name)}-remove");

    public void OpenAddExercise() => TapButton("add-exercise");

    public void Save() => TapButton("save-workout");

    public void Back() => web.Tap("back");

    public void Delete() => web.Tap("delete-workout");

    /// <summary>The field's <c>inputmode</c>: the keypad it asks Android for.</summary>
    public string InputMode(string testId) => web.Attribute(testId, "inputmode");

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
