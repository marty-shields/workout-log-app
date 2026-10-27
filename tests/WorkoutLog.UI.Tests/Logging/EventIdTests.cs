using System.Text.RegularExpressions;
using WorkoutLog.UI.Logging;

namespace WorkoutLog.UI.Tests.Logging;

/// <summary>
/// Event ids are named values: <c>UiEvent</c> 2000–4999 here, <c>AppEvent</c> 5000+ (the App can't be referenced, so its file is read),
/// <c>CoreEvent</c> in Core.Tests. Values are unique and fixed. No <c>[LoggerMessage]</c> in the source uses a bare number.
/// </summary>
public partial class EventIdTests
{
    private static IEnumerable<TestCaseData> SourceFiles() =>
        RepoFiles.Find("*.cs", "src")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetRelativePath(RepoFiles.Root, f)));

    [Test]
    public void UiEvent_ValuesAreUnique() =>
        Assert.That(Enum.GetValues<UiEvent>().Select(e => (int)e), Is.Unique);

    [Test]
    public void UiEvent_ValuesAreInTheUisRange() =>
        Assert.That(Enum.GetValues<UiEvent>().Select(e => (int)e), Is.All.InRange(2000, 4999));

    [Test]
    public void UiEvent_ValuesAreFixed() =>
        Assert.That(Enum.GetValues<UiEvent>().ToDictionary(e => e, e => (int)e), Is.EqualTo(new Dictionary<UiEvent, int>
        {
            [UiEvent.StartupFinished] = 2001,
            [UiEvent.StartupFailed] = 2002,
            [UiEvent.ScreenError] = 2101,
            [UiEvent.BackHandled] = 2201,
            [UiEvent.BackNotHandled] = 2202,
            [UiEvent.BackHandlerFailed] = 2203,
            [UiEvent.ExerciseFocusFailed] = 2301,
            [UiEvent.ErrorFocusFailed] = 2302,
            [UiEvent.BlockedSetFocusFailed] = 2401,
            [UiEvent.SetCheckExerciseError] = 2501,
            [UiEvent.SetCheckFieldUnworded] = 2502,
            [UiEvent.SaveCheckExerciseError] = 2601,
            [UiEvent.NewWorkoutDiscarded] = 2701,
            [UiEvent.EditWorkoutOpenFailed] = 2901,
            [UiEvent.EditsDiscarded] = 2902,
            [UiEvent.AppStartHandlerFailed] = 3201,
            [UiEvent.LoadFailedFocusFailed] = 3301,
            [UiEvent.ShareFailed] = 3401,
            [UiEvent.SettingsPickFailed] = 3402,
            [UiEvent.ExportFocusFailed] = 3501,
            [UiEvent.SettingsImportFocusFailed] = 3502,
            [UiEvent.ReviewImportFocusFailed] = 3601,
            [UiEvent.ReviewPickFailed] = 3701,
            [UiEvent.ImportFileTooLarge] = 3801,
        }));

    [Test]
    public void UiEvent_RetiredValuesAreNotReused() =>
        Assert.That(Enum.GetValues<UiEvent>().Select(e => (int)e), Has.None.AnyOf(2801, 3001));

    [Test]
    public void AppEvent_ValuesAreUniqueAndInTheAppsRange()
    {
        var values = AppEvents().Values;

        Assert.Multiple(() =>
        {
            Assert.That(values, Is.Not.Empty);
            Assert.That(values, Is.Unique);
            Assert.That(values, Is.All.GreaterThanOrEqualTo(5000));
        });
    }

    [Test]
    public void AppEvent_ValuesAreFixed() =>
        Assert.That(AppEvents(), Is.EqualTo(new Dictionary<string, int>
        {
            ["ProcessStarted"] = 5001,
            ["PreviousExit"] = 5002,
            ["PreviousExitAbnormal"] = 5003,
            ["NoPreviousExit"] = 5004,
            ["ActivityCreated"] = 5011,
            ["ActivityStopped"] = 5012,
            ["ActivityDestroyed"] = 5013,
            ["ActivityStarted"] = 5014,
            ["TrimMemory"] = 5021,
            ["UnhandledException"] = 5031,
            ["UnobservedTaskException"] = 5033,
        }));

    [TestCaseSource(nameof(SourceFiles))]
    public void LoggerMessage_UsesANamedEventId(string file) =>
        Assert.That(BareNumber().IsMatch(File.ReadAllText(file)), Is.False);

    /// <summary>Each <c>AppEvent</c> member's name and value, read from its source file.</summary>
    private static Dictionary<string, int> AppEvents() =>
        Member().Matches(RepoFiles.Read("src", "WorkoutLog.App", "Logging", "AppEvent.cs"))
            .ToDictionary(m => m.Groups[1].Value, m => int.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture));

    [GeneratedRegex(@"(\w+)\s*=\s*(\d+)")]
    private static partial Regex Member();

    [GeneratedRegex(@"\[LoggerMessage\(\s*(EventId\s*=\s*)?\d")]
    private static partial Regex BareNumber();
}
