using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using WorkoutLog.App.Diagnostics;
using WorkoutLog.App.Logging;
using WorkoutLog.Core;
using WorkoutLog.Core.Logging;

namespace WorkoutLog.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddCore(Path.Combine(FileSystem.AppDataDirectory, "workoutlog.db3"));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        AddLogging(builder);
        AddDiagnostics(builder);

        return builder.Build();
    }

    /// <summary>
    /// Logs to logcat and to a file in the app's external files folder, or the app data folder if there is none.
    /// Debug builds log from Debug, Release from Information; <c>Microsoft.*</c> from Warning.
    /// </summary>
    private static void AddLogging(MauiAppBuilder builder)
    {
        var folder = Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null)?.AbsolutePath ?? FileSystem.AppDataDirectory, "logs");

        builder.Logging.ClearProviders();
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

        builder.Services.AddSingleton(_ => new FileLogWriter(folder, FileLogWriter.DefaultMaxBytes));
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AndroidLogProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLogProvider>());
    }

    /// <summary>
    /// Logs the process start, activity lifecycle and trim-memory requests through <see cref="AndroidDiagnostics"/>.
    /// The handlers resolve it from the app's services.
    /// </summary>
    private static void AddDiagnostics(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AndroidDiagnostics>();
        builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android
            .OnApplicationCreate(app => Diagnostics().ProcessStarted(app))
            .OnCreate((_, _) => Diagnostics().ActivityCreated())
            .OnStop(_ => Diagnostics().ActivityStopped())
            .OnDestroy(_ => Diagnostics().ActivityDestroyed())
            .OnApplicationTrimMemory((_, level) => Diagnostics().TrimMemory(level))));

        static AndroidDiagnostics Diagnostics() => IPlatformApplication.Current!.Services.GetRequiredService<AndroidDiagnostics>();
    }
}
