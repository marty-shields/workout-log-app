using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.App.Components;

public static class NavigationManagerExtensions
{
    /// <summary>
    /// Navigates without adding a WebView back-stack entry. The BlazorWebView takes back while the WebView
    /// can go back, so the app keeps no back stack and back reaches <c>MainActivity</c>.
    /// </summary>
    public static void GoTo(this NavigationManager navigation, string uri) =>
        navigation.NavigateTo(uri, replace: true);

    /// <summary>Opens a top-level screen, as <see cref="GoTo(NavigationManager, string)"/> does.</summary>
    public static void GoTo(this NavigationManager navigation, NavDestination destination) =>
        navigation.GoTo(NavDestinations.Of(destination).Route);
}
