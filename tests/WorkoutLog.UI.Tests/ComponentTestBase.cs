using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests;

/// <summary>Creates a fresh bUnit context for each test, with a <see cref="BackButton"/> and the exercise catalog.</summary>
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
        Context.Services.AddSingleton<IBackButton>(BackButton);
        Context.Services.AddSingleton(Catalog);
    }

    [TearDown]
    public void DisposeContext() => Context.Dispose();
}
