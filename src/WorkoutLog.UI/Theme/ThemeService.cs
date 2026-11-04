using Microsoft.JSInterop;
using WorkoutLog.UI.Models.Settings;

namespace WorkoutLog.UI.Theme;

/// <summary>
/// Keeps the chosen <see cref="ThemePreference"/> in the web view's local storage and applies it to the document as <c>data-theme</c>.
/// <c>theme.js</c>, loaded in the page head, applies the saved choice before the first paint.
/// </summary>
public sealed class ThemeService(IJSRuntime js)
{
    /// <summary>The saved choice. <see cref="ThemePreference.System"/> when nothing is saved or storage is unavailable.</summary>
    public async Task<ThemePreference> GetAsync() =>
        Enum.TryParse<ThemePreference>(await js.InvokeAsync<string>("workoutLogTheme.get"), ignoreCase: true, out var preference)
            ? preference
            : ThemePreference.System;

    /// <summary>Applies the choice to the document and saves it. False when storage refused the save: the choice still applies until the app closes.</summary>
    public async Task<bool> SetAsync(ThemePreference preference) =>
        await js.InvokeAsync<bool>("workoutLogTheme.set", preference.ToString().ToLowerInvariant());
}
