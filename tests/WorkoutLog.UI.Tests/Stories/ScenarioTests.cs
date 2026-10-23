using System.Reflection;
using Bunit.Extensions.WaitForHelpers;
using WorkoutLog.UI.Stories.Stories.Prototype;

namespace WorkoutLog.UI.Tests.Stories;

public class ScenarioTests : ComponentTestBase
{
    /// <summary>How long a step waits for its target, as the scenario player does.</summary>
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(3);

    private static IEnumerable<TestCaseData> AllScenarios() =>
        typeof(Scenarios).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => (Scenario)f.GetValue(null)!)
            .Select(s => new TestCaseData(s).SetArgDisplayNames(s.Name));

    [TestCaseSource(nameof(AllScenarios))]
    public void Scenario_EveryStepFindsItsTarget(Scenario scenario)
    {
        Play(scenario, (_, _) => { });
    }

    [TestCaseSource(nameof(AllScenarios))]
    public void Scenario_InteractiveElementsHaveTestIdsAtEveryStep(Scenario scenario)
    {
        Play(scenario, (cut, when) => Assert.That(TestIdTests.WithoutTestId(cut.FindAll("*")), Is.Empty, when));
    }

    /// <summary>Plays every step, calling <paramref name="check"/> before each step and after the last.</summary>
    private void Play(Scenario scenario, Action<IRenderedComponent<PrototypeApp>, string> check)
    {
        var cut = Context.Render<PrototypeApp>(p => p
            .Add(x => x.Empty, scenario.Empty)
            .Add(x => x.StartupDelay, scenario.StartupDelay)
            .Add(x => x.StartupFails, scenario.StartupFails));

        for (var i = 0; i < scenario.Steps.Count; i++)
        {
            var step = scenario.Steps[i];
            var selector = $"[data-testid=\"{step.TestId}\"]";
            try
            {
                cut.WaitForState(() => cut.FindAll(selector).Count > 0, StepTimeout);
            }
            catch (WaitForFailedException)
            {
            }

            var targets = cut.FindAll(selector);
            Assert.That(targets, Is.Not.Empty, $"Step {i + 1} \"{step.Caption}\": nothing with data-testid {step.TestId}");
            foreach (var expected in step.Expected ?? [])
            {
                Assert.That(TestText.Normalise(targets[0].TextContent), Does.Contain(expected), $"Step {i + 1} \"{step.Caption}\"");
            }

            check(cut, $"Before step {i + 1} \"{step.Caption}\"");

            switch (step.Action)
            {
                case StepAction.Click:
                    targets[0].Click();
                    break;
                case StepAction.Enter:
                    Type(cut, selector, step.Text);
                    break;
            }
        }

        check(cut, "After the last step");
    }

    /// <summary>Clears the input, then types one character at a time, as ScenarioPlayer.razor.js does, so each partial value is checked.</summary>
    private static void Type(IRenderedComponent<PrototypeApp> cut, string selector, string? text)
    {
        cut.Find(selector).Input("");
        var typed = "";
        foreach (var character in text ?? "")
        {
            typed += character;
            cut.Find(selector).Input(typed);
        }
    }
}
