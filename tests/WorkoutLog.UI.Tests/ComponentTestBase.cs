using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// Creates a fresh bUnit context for each test with the UI's services (<see cref="UiServiceCollectionExtensions.AddUi"/>),
/// a <see cref="BackButton"/>, a <see cref="FakeTimeProvider"/> that never moves, the exercise catalog, a <see cref="SampleWorkoutService"/>, a <see cref="SampleWeightHistoryService"/>, a <see cref="FakeLoggedExercisesService"/>, a <see cref="FakeLastSessionService"/> and a <see cref="FakeHaptics"/>.
/// Tests may register their own clock, workout and weight history services.
/// </summary>
public abstract class ComponentTestBase
{
    /// <summary>The real exercise list, loaded once for all tests.</summary>
    protected static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    protected BunitContext Context { get; private set; } = null!;

    /// <summary>The back button the rendered components register with.</summary>
    protected BackButton BackButton { get; private set; } = null!;

    /// <summary>The logged exercises service the rendered components read; nothing logged unless a test sets its result.</summary>
    protected FakeLoggedExercisesService LoggedExercises { get; private set; } = null!;

    /// <summary>The last session service the rendered components read; no earlier workout unless a test sets one.</summary>
    protected FakeLastSessionService LastSessions { get; private set; } = null!;

    /// <summary>The haptics the rendered components play; records each cue.</summary>
    protected FakeHaptics Haptics { get; private set; } = null!;

    [SetUp]
    public void CreateContext()
    {
        Context = new BunitContext();
        BackButton = new BackButton(NullLogger<BackButton>.Instance);
        LoggedExercises = new FakeLoggedExercisesService();
        LastSessions = new FakeLastSessionService();
        Haptics = new FakeHaptics();
        Context.Services.AddUi();
        Context.Services.AddSingleton<IBackButton>(BackButton);
        Context.Services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        Context.Services.AddSingleton(Catalog);
        Context.Services.AddSingleton<IWorkoutService>(new SampleWorkoutService(new FakeTimeProvider(), Catalog));
        Context.Services.AddSingleton<IWeightHistoryService>(new SampleWeightHistoryService());
        Context.Services.AddSingleton<ILoggedExercisesService>(LoggedExercises);
        Context.Services.AddSingleton<ILastSessionService>(LastSessions);
        Context.Services.AddSingleton<IHaptics>(Haptics);
    }

    [TearDown]
    public void DisposeContext() => Context.Dispose();

    /// <summary>
    /// Waits until the renderer has run the work queued so far, such as a component continuing after a fake call completes or
    /// a timer ticks. bUnit's synchronous events (<c>Click()</c>, <c>Input()</c>) are queued, not run, while the renderer is busy.
    /// </summary>
    protected Task WaitForRendererAsync() => Context.Renderer.Dispatcher.InvokeAsync(() => { });
}
