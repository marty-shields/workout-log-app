using Microsoft.AspNetCore.Components;

namespace WorkoutLog.App.Components;

/// <summary>In-app navigation.</summary>
public static class NavigationManagerExtensions
{
    /// <summary>
    /// Navigates without adding a WebView back-stack entry. The BlazorWebView takes back while the WebView
    /// can go back, so the app keeps no back stack and back reaches <c>MainActivity</c> (D119).
    /// </summary>
    public static void GoTo(this NavigationManager navigation, string uri) =>
        navigation.NavigateTo(uri, replace: true);
}
