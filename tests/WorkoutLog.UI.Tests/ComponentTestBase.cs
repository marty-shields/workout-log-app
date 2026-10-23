using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests;

/// <summary>Creates a fresh bUnit context for each test, with a <see cref="BackButton"/>.</summary>
public abstract class ComponentTestBase
{
    protected BunitContext Context { get; private set; } = null!;

    /// <summary>The back button the rendered components register with.</summary>
    protected BackButton BackButton { get; private set; } = null!;

    [SetUp]
    public void CreateContext()
    {
        Context = new BunitContext();
        BackButton = new BackButton(NullLogger<BackButton>.Instance);
        Context.Services.AddSingleton<IBackButton>(BackButton);
    }

    [TearDown]
    public void DisposeContext() => Context.Dispose();
}
