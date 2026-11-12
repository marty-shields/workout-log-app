using System.Globalization;
using Microsoft.JSInterop;

namespace WorkoutLog.UI.Theme;

/// <summary>
/// Writes the space the system bars and camera cutout cover to <c>--inset-top</c> and <c>--inset-bottom</c> on the document.
/// A repeat of the values last written does nothing.
/// </summary>
public sealed class SystemBarInsetWriter(IJSRuntime js)
{
    private (float Top, float Bottom)? written;

    /// <summary>
    /// Converts the insets from device pixels to CSS pixels and writes them.
    /// Does nothing when <paramref name="density"/> is zero or less, or when the values are the ones last written.
    /// A failed write is not remembered, so the next call tries again.
    /// </summary>
    public async Task ApplyAsync(int topPixels, int bottomPixels, float density)
    {
        if (density <= 0)
        {
            return;
        }

        (float Top, float Bottom) current = (topPixels / density, bottomPixels / density);
        if (current == written)
        {
            return;
        }

        try
        {
            await js.InvokeVoidAsync("document.documentElement.style.setProperty", "--inset-top", Pixels(current.Top));
            await js.InvokeVoidAsync("document.documentElement.style.setProperty", "--inset-bottom", Pixels(current.Bottom));
            written = current;
        }
        catch (JSDisconnectedException)
        {
            // The page is gone, and its insets with it.
        }
        catch (JSException)
        {
            // Left unwritten, so the next call tries again.
        }
    }

    private static string Pixels(float value) => value.ToString(CultureInfo.InvariantCulture) + "px";
}
