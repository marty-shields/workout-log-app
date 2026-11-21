using Microsoft.AspNetCore.Components;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;
using WorkoutLog.App.Lifecycle;
using WorkoutLog.App.Navigation;

namespace WorkoutLog.App.Components;

/// <summary>
/// Hands the BlazorWebView's scoped <see cref="IBackButton"/> and <see cref="IAppLifecycle"/> to the Android activity's wiring,
/// which lives outside that scope and so cannot resolve them. Renders nothing. Put it first in the root component,
/// so it connects (which clears the earlier handlers) before any page registers.
/// </summary>
public sealed class ActivityBridge : ComponentBase, IDisposable
{
    [Inject]
    private IBackButton BackButton { get; set; } = null!;

    [Inject]
    private IAppLifecycle Lifecycle { get; set; } = null!;

    [Inject]
    private AndroidBackButton AndroidBackButton { get; set; } = null!;

    [Inject]
    private AndroidAppLifecycle AndroidLifecycle { get; set; } = null!;

    public void Dispose()
    {
        AndroidBackButton.Disconnect(BackButton);
        AndroidLifecycle.Disconnect(Lifecycle);
    }

    protected override void OnInitialized()
    {
        AndroidBackButton.Connect(BackButton);
        AndroidLifecycle.Connect(Lifecycle);
    }
}
