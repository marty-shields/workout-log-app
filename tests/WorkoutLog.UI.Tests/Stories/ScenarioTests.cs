using System.Reflection;
using WorkoutLog.UI.Stories.Stories.Prototype;

namespace WorkoutLog.UI.Tests.Stories;

public class ScenarioTests : ComponentTestBase
{
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
        var cut = Context.Render<PrototypeApp>(p => p.Add(x => x.Empty, scenario.Empty));

        for (var i = 0; i < scenario.Steps.Count; i++)
        {
            var step = scenario.Steps[i];
            check(cut, $"Before step {i + 1} \"{step.Caption}\"");
            var targets = cut.FindAll($"[data-testid=\"{step.TestId}\"]");
            Assert.That(targets, Is.Not.Empty, $"Step {i + 1} \"{step.Caption}\": nothing with data-testid {step.TestId}");

            switch (step.Action)
            {
                case StepAction.Click:
                    targets[0].Click();
                    break;
                case StepAction.Enter:
                    targets[0].Input(step.Text);
                    break;
            }
        }

        check(cut, "After the last step");
    }
}
