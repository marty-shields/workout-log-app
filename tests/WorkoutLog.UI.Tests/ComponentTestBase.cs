namespace WorkoutLog.UI.Tests;

/// <summary>Creates a fresh bUnit context for each test.</summary>
public abstract class ComponentTestBase
{
    protected BunitContext Context { get; private set; } = null!;

    [SetUp]
    public void CreateContext() => Context = new BunitContext();

    [TearDown]
    public void DisposeContext() => Context.Dispose();
}
