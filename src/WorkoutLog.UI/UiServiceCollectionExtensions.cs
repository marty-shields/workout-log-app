using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Theme;

namespace WorkoutLog.UI;

public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the UI's services. The host must register Core's services, logging and an <see cref="IImportFilePicker"/>.</summary>
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
            .AddSingleton<LastTimeLoader>()
            .AddSingleton<ThemeService>()
            .AddTransient<WorkoutEditor>();
}
