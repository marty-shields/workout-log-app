using System.Globalization;
using Android.Views;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using View = Android.Views.View;

namespace WorkoutLog.App.Components;

/// <summary>
/// Sets <c>--inset-top</c> and <c>--inset-bottom</c> on the page to the space the status bar, camera cutout and navigation bar
/// cover, in CSS pixels, and again whenever the window lays out with other values, such as after a rotation.
/// MAUI 11 draws the page edge to edge, and the WebView doesn't report these insets through <c>env(safe-area-inset-*)</c>
/// (Android System WebView 133 on the API 36 emulator gives 0). Where a WebView does, the values set here are the same
/// insets and replace the <c>env()</c> ones rather than adding to them.
/// </summary>
public sealed class SystemBarInsets : ComponentBase, IDisposable
{
    private View? decor;
    private ViewTreeObserver? observer;
    private LayoutListener? listener;
    private (float Top, float Bottom)? applied;

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    public void Dispose()
    {
        if (listener is null)
        {
            return;
        }

        if (observer is { IsAlive: true })
        {
            observer.RemoveOnGlobalLayoutListener(listener);
        }

        listener.Dispose();
        listener = null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || Platform.CurrentActivity?.Window?.DecorView is not { } view)
        {
            return;
        }

        decor = view;
        listener = new LayoutListener(() => InvokeAsync(ApplyAsync));
        observer = decor.ViewTreeObserver;
        observer?.AddOnGlobalLayoutListener(listener);
        await ApplyAsync();
    }

    private async Task ApplyAsync()
    {
        var density = decor?.Resources?.DisplayMetrics?.Density ?? 0;
        if (decor?.RootWindowInsets?.GetInsets(WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout()) is not { } insets || density <= 0)
        {
            return;
        }

        (float Top, float Bottom) current = (insets.Top / density, insets.Bottom / density);
        if (current == applied)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("document.documentElement.style.setProperty", "--inset-top", Pixels(current.Top));
            await JS.InvokeVoidAsync("document.documentElement.style.setProperty", "--inset-bottom", Pixels(current.Bottom));
            applied = current;
        }
        catch (JSDisconnectedException)
        {
            // The page is gone, and its insets with it.
        }
        catch (JSException)
        {
            // Left unapplied, so the next layout tries again.
        }
    }

    private static string Pixels(float value) => value.ToString(CultureInfo.InvariantCulture) + "px";

    private sealed class LayoutListener(Func<Task> laidOut) : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        public void OnGlobalLayout() => _ = laidOut();
    }
}
