using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using WorkoutLog.App.Diagnostics;
using WorkoutLog.App.Export;
using WorkoutLog.App.Import;
using WorkoutLog.App.Logging;
using WorkoutLog.App.Navigation;
using WorkoutLog.App.Sharing;
using WorkoutLog.App.Timing;
using WorkoutLog.Core;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using WorkoutLog.UI;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Sharing;

namespace WorkoutLog.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddCore(Path.Combine(FileSystem.AppDataDirectory, "workoutlog.db3"));
        builder.Services.AddUi();
        builder.Services.AddSingleton<IExportEnvironment, MauiExportEnvironment>();
        builder.Services.AddSingleton<IElapsedClock, AndroidElapsedClock>();
        builder.Services.AddSingleton<IFileShare, MauiFileShare>();
        builder.Services.AddSingleton<IImportFilePicker, MauiImportFilePicker>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        AddLogging(builder);
        AddDiagnostics(builder);
        AddBackButton(builder);
        AddAppLifecycle(builder);

        return builder.Build();
    }

    /// <summary>
    /// Passes the Android back button to the registered page handlers through <see cref="AndroidBackButton"/>.
    /// Only <see cref="MainActivity"/> counts; MAUI's own activities, such as the file picker's, are ignored.
    /// The handlers resolve it from the app's services.
    /// </summary>
    private static void AddBackButton(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AndroidBackButton>();
        builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android
            .OnCreate((activity, _) =>
            {
                if (activity is MainActivity main)
                {
                    BackButton().Attach(main);
                }
            })
            .OnDestroy(activity => BackButton().Detach(activity))));

        static AndroidBackButton BackButton() => IPlatformApplication.Current!.Services.GetRequiredService<AndroidBackButton>();
    }

    /// <summary>
    /// Runs the pages' app start handlers through <see cref="IAppLifecycle"/> each time <see cref="MainActivity"/> starts,
    /// and removes them when it is destroyed or replaced. MAUI's own activities are ignored. The handlers resolve it from the app's services.
    /// </summary>
    private static void AddAppLifecycle(MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => events.AddAndroid(android => android
            .OnCreate((activity, _) =>
            {
                if (activity is MainActivity)
                {
                    Lifecycle().Attach(activity);
                }
            })
            .OnStart(activity =>
            {
                if (activity is MainActivity)
                {
                    _ = Lifecycle().NotifyStartedAsync();
                }
            })
            .OnDestroy(activity => Lifecycle().Detach(activity))));

        static IAppLifecycle Lifecycle() => IPlatformApplication.Current!.Services.GetRequiredService<IAppLifecycle>();
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
            .OnStart(_ => Diagnostics().ActivityStarted())
            .OnStop(_ => Diagnostics().ActivityStopped())
            .OnDestroy(_ => Diagnostics().ActivityDestroyed())
            .OnApplicationTrimMemory((_, level) => Diagnostics().TrimMemory(level))));

        static AndroidDiagnostics Diagnostics() => IPlatformApplication.Current!.Services.GetRequiredService<AndroidDiagnostics>();
    }
}
