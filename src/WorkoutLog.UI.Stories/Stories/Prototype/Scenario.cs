namespace WorkoutLog.UI.Stories.Stories.Prototype;

/// <summary>A user task played on the prototype by <see cref="ScenarioPlayer"/>.</summary>
/// <param name="Name">Scenario name.</param>
/// <param name="Summary">One line on what the scenario shows.</param>
/// <param name="Steps">Steps in order.</param>
/// <param name="Empty">Starts with no saved workouts.</param>
/// <param name="StartupDelay">How long the Startup screen shows first. None by default.</param>
/// <param name="StartupFails">Startup fails and shows its error.</param>
public sealed record Scenario(
    string Name, string Summary, IReadOnlyList<ScenarioStep> Steps, bool Empty = false, TimeSpan StartupDelay = default, bool StartupFails = false);

/// <summary>What a step does to its target element.</summary>
public enum StepAction
{
    /// <summary>Taps the element.</summary>
    Click,

    /// <summary>Clears the input, then types the text.</summary>
    Enter,

    /// <summary>Outlines the element without changing anything.</summary>
    Show,
}

/// <summary>One step of a scenario.</summary>
/// <param name="Caption">Text shown in the steps list.</param>
/// <param name="Action">What the step does.</param>
/// <param name="TestId">data-testid of the target element.</param>
/// <param name="Text">Text typed by <see cref="StepAction.Enter"/>.</param>
public sealed record ScenarioStep(string Caption, StepAction Action, string TestId, string? Text = null)
{
    /// <summary>Taps the element.</summary>
    public static ScenarioStep Click(string caption, string testId) => new(caption, StepAction.Click, testId);

    /// <summary>Types <paramref name="text"/> into the input.</summary>
    public static ScenarioStep Enter(string caption, string testId, string text) => new(caption, StepAction.Enter, testId, text);

    /// <summary>Outlines the element.</summary>
    public static ScenarioStep Show(string caption, string testId) => new(caption, StepAction.Show, testId);

    /// <summary>Presses the simulated Android back button.</summary>
    public static ScenarioStep Back(string caption) => Click(caption, "system-back");
}
