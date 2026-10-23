using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// Creates a fresh bUnit context for each test with the UI's services (<see cref="UiServiceCollectionExtensions.AddUi"/>),
/// a <see cref="BackButton"/>, the exercise catalog and a <see cref="SampleWorkoutService"/>. Tests may register their own workout service.
/// </summary>
public abstract class ComponentTestBase
{
    /// <summary>The real exercise list, loaded once for all tests.</summary>
    protected static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    protected BunitContext Context { get; private set; } = null!;

    /// <summary>The back button the rendered components register with.</summary>
    protected BackButton BackButton { get; private set; } = null!;

    [SetUp]
    public void CreateContext()
    {
        Context = new BunitContext();
        BackButton = new BackButton(NullLogger<BackButton>.Instance);
        Context.Services.AddUi();
        Context.Services.AddSingleton<IBackButton>(BackButton);
        Context.Services.AddSingleton(Catalog);
        Context.Services.AddSingleton<IWorkoutService>(new SampleWorkoutService(new FakeTimeProvider(), Catalog));
    }

    [TearDown]
    public void DisposeContext() => Context.Dispose();
}
