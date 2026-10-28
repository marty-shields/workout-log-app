using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ExercisesPageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Curl = 132;
    private const int Run = 311;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly LoggedExercisesResult Logged = new(
    [
        new LoggedExercise(Run, Now.AddDays(-1), new ExerciseBest(null, null, null, 5.2m)),
        new LoggedExercise(Squat, Now.AddDays(-8), new ExerciseBest(70m, 3, null, null)),
        new LoggedExercise(Curl, Now.AddYears(-1), null),
    ], Failed: false);

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        LoggedExercises.Result = Logged;
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerOnly()
    {
        LoggedExercises.Hold = true;

        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
        });
    }

    [Test]
    public void Loaded_ListsTheExercisesInOrderWithTheirLastSessionAndKind()
    {
        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercises-row-311", "exercises-row-6", "exercises-row-132" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .detail").Select(d => d.TextContent),
                Is.EqualTo(new[] { "Last Tue 27 Oct · 5.2km", "Last Tue 20 Oct · 70kg × 3", "Last Tue 28 Oct 2025" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .aside").Select(a => a.TextContent), Is.EqualTo(new[] { "Cardio", "Strength", "Strength" }));
            Assert.That(cut.FindAll("button[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercises-row-6", "exercises-row-132" }));
        });
    }

    [Test]
    public void TappingALift_RaisesOnOpenWithItsId()
    {
        var opened = new List<int>();
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnOpen, (int id) => opened.Add(id)));

        cut.Find("[data-testid=exercises-row-132]").Click();

        Assert.That(opened, Is.EqualTo(new[] { Curl }));
    }

    [Test]
    public void NothingLogged_ShowsTheMessage()
    {
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: false);

        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-empty]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
        });
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainLoadsAgain()
    {
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: true);
        var cut = Context.Render<ExercisesPage>();
        Assume.That(cut.Find("[data-testid=exercises-error]").TextContent, Is.EqualTo("Couldn't load your exercises"));
        LoggedExercises.Result = Logged;

        cut.Find("[data-testid=exercises-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=exercises-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Search_RanksTheMatchesAmongTheLoggedExercises()
    {
        var cut = Context.Render<ExercisesPage>();

        cut.Find("[data-testid=exercises-search]").Input("barbell");

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-6" }));
    }

    [Test]
    public void Search_NoMatch_ShowsNoExercisesMatch()
    {
        var cut = Context.Render<ExercisesPage>();

        cut.Find("[data-testid=exercises-search]").Input("zumba");

        Assert.That(cut.Find("[data-testid=exercises-none]").TextContent, Is.EqualTo("No exercises match"));
    }

    [TestCase("exercises-filter-strength", null, new[] { "exercises-row-6", "exercises-row-132" })]
    [TestCase("exercises-filter-cardio", null, new[] { "exercises-row-311" })]
    [TestCase("exercises-filter-strength", "curl", new[] { "exercises-row-132" })]
    [TestCase("exercises-filter-cardio", "curl", new string[0])]
    public void Chip_FiltersWithAndWithoutSearch(string chip, string? query, string[] expected)
    {
        var cut = Context.Render<ExercisesPage>();
        if (query is not null)
        {
            cut.Find("[data-testid=exercises-search]").Input(query);
        }

        cut.Find($"[data-testid={chip}]").Change(true);

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(expected));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var backs = 0;
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnBack, () => backs++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(backs, Is.EqualTo(1));
    }
}
