using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Logging;
using WorkoutLog.App.Logging;

namespace WorkoutLog.App;

public partial class MainPage : ContentPage
{
    private readonly ILogger<MainPage> logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<MainPage>>();

    public MainPage()
    {
        InitializeComponent();
    }

    /// <summary>The app has no external links. The default for one is to open it in the browser; cancel it instead.</summary>
    private void OnUrlLoading(object? sender, UrlLoadingEventArgs e)
    {
        if (e.UrlLoadingStrategy == UrlLoadingStrategy.OpenInWebView)
        {
            return;
        }

        e.UrlLoadingStrategy = UrlLoadingStrategy.CancelLoad;
        Log.UrlBlocked(logger, e.Url.Scheme, e.Url.Host);
    }

    private static partial class Log
    {
        [LoggerMessage((int)AppEvent.UrlBlocked, LogLevel.Warning, "Blocked a link to {Scheme}://{Host}")]
        public static partial void UrlBlocked(ILogger logger, string scheme, string host);
    }
}
