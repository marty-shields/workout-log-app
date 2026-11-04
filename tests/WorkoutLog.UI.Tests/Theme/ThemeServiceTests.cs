using WorkoutLog.UI.Models.Settings;
using WorkoutLog.UI.Theme;

namespace WorkoutLog.UI.Tests.Theme;

public class ThemeServiceTests
{
    private BunitContext context = null!;
    private ThemeService service = null!;

    [SetUp]
    public void SetUp()
    {
        context = new BunitContext();
        service = new ThemeService(context.JSInterop.JSRuntime);
    }

    [TearDown]
    public void TearDown() => context.Dispose();

    [Test]
    public void Preference_DefaultsToSystem() => Assert.That(default(ThemePreference), Is.EqualTo(ThemePreference.System));

    [TestCase("system", ThemePreference.System)]
    [TestCase("light", ThemePreference.Light)]
    [TestCase("dark", ThemePreference.Dark)]
    [TestCase("sepia", ThemePreference.System)]
    [TestCase("", ThemePreference.System)]
    public async Task Get_ReadsTheSavedChoice(string saved, ThemePreference expected)
    {
        context.JSInterop.Setup<string>("workoutLogTheme.get").SetResult(saved);

        Assert.That(await service.GetAsync(), Is.EqualTo(expected));
    }

    [TestCase(ThemePreference.System, "system")]
    [TestCase(ThemePreference.Light, "light")]
    [TestCase(ThemePreference.Dark, "dark")]
    public async Task Set_AppliesAndSavesTheChoice(ThemePreference preference, string expected)
    {
        context.JSInterop.SetupVoid("workoutLogTheme.set", _ => true).SetVoidResult();

        await service.SetAsync(preference);

        Assert.That(context.JSInterop.Invocations["workoutLogTheme.set"].Single().Arguments, Is.EqualTo(new object[] { expected }));
    }
}
