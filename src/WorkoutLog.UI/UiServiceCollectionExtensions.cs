using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI;

/// <summary>Registers the UI's services.</summary>
public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the UI's services. The host must register Core's services, logging and an <see cref="IImportFilePicker"/>.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddUi(this IServiceCollection services) =>
        services
            .AddSingleton<ImportSession>()
            .AddSingleton<IImportFileOpener, ImportFileOpener>()
            .AddSingleton<IBackButton, BackButton>()
            .AddSingleton<AppLifecycle>()
            .AddSingleton<IAppStart>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<IAppLifecycle>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<ISetChecker, SetChecker>()
            .AddSingleton<IWorkoutChecker, WorkoutChecker>()
            .AddTransient<WorkoutEditor>();
}
