using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class MusclesPageTests : ComponentTestBase
{
    private static readonly MuscleSetsResult Loaded = new(
    [
        new MuscleGroupCount(MuscleGroup.Chest, 12),
        new MuscleGroupCount(MuscleGroup.Back, 0),
        new MuscleGroupCount(MuscleGroup.Shoulders, 0),
        new MuscleGroupCount(MuscleGroup.Arms, 1),
        new MuscleGroupCount(MuscleGroup.Legs, 9),
        new MuscleGroupCount(MuscleGroup.Core, 0),
    ], Failed: false);

    private FakeMuscleSetsService muscleSets = null!;

    [SetUp]
    public void SetUp()
    {
        muscleSets = new FakeMuscleSetsService { Result = Loaded };
        Context.Services.AddSingleton<IMuscleSetsService>(muscleSets);
    }

    [Test]
    public async Task WhileLoading_ShowsTheSpinnerOnly()
    {
        muscleSets.Hold = true;
        var cut = Context.Render<MusclesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=muscles-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=muscles-chest]"), Is.Empty);
        });

        muscleSets.Complete();
        await WaitForRendererAsync();
        Assert.That(cut.FindAll("[data-testid=muscles-loading]"), Is.Empty);
    }

    [Test]
    public void Loaded_ShowsEachGroupsSetsInOrder()
    {
        var cut = Context.Render<MusclesPage>();

        Assert.That(Rows(cut), Is.EqualTo(new[]
        {
            ("muscles-chest", "12 sets"),
            ("muscles-back", "Not trained"),
            ("muscles-shoulders", "Not trained"),
            ("muscles-arms", "1 set"),
            ("muscles-legs", "9 sets"),
            ("muscles-core", "Not trained"),
        }));
    }

    [Test]
    public void NothingInTheLast7Days_EveryGroupIsNotTrained()
    {
        muscleSets.Result = new MuscleSetsResult(Enum.GetValues<MuscleGroup>().Select(g => new MuscleGroupCount(g, 0)).ToList(), Failed: false);

        var cut = Context.Render<MusclesPage>();

        Assert.That(Rows(cut).Select(r => r.Sets), Is.EqualTo(Enumerable.Repeat("Not trained", 6)));
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainLoadsAgain()
    {
        muscleSets.Result = new MuscleSetsResult([], Failed: true);
        var cut = Context.Render<MusclesPage>();
        Assume.That(cut.Find("[data-testid=muscles-error]").TextContent, Is.EqualTo("Couldn't load your muscles"));
        muscleSets.Result = Loaded;

        cut.Find("[data-testid=muscles-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(muscleSets.Loads, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=muscles-error]"), Is.Empty);
            Assert.That(Rows(cut), Has.Count.EqualTo(6));
        });
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = 0;
        var cut = Context.Render<MusclesPage>(p => p.Add(x => x.OnBack, () => back++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.EqualTo(1));
    }

    private static List<(string TestId, string Sets)> Rows(IRenderedComponent<MusclesPage> cut) =>
        cut.FindAll("li[data-testid^=muscles-]")
            .Select(r => (r.GetAttribute("data-testid")!, r.QuerySelector(".sets")!.TextContent))
            .ToList();
}
