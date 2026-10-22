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
        var cut = Context.Render<PrototypeApp>(p => p.Add(x => x.Empty, scenario.Empty));

        for (var i = 0; i < scenario.Steps.Count; i++)
        {
            var step = scenario.Steps[i];
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
    }
}
