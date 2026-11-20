using Microsoft.Extensions.DependencyInjection;
using Shields.UI;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI;

public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the UI's services and the kit's (<see cref="ShieldsUiServiceCollectionExtensions.AddShieldsUi"/>, which gives the theme and system-bar services). The host must register Core's services, logging, an <see cref="IImportFilePicker"/> and an <see cref="Haptics.IHaptics"/>.</summary>
    public static IServiceCollection AddUi(this IServiceCollection services) =>
        services
            .AddShieldsUi()
            .AddSingleton<ImportSession>()
            .AddSingleton<IImportFileOpener, ImportFileOpener>()
            .AddSingleton<IBackButton, BackButton>()
            .AddSingleton<AppLifecycle>()
            .AddSingleton<IAppStart>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<IAppLifecycle>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<ISetChecker, SetChecker>()
            .AddSingleton<IWorkoutChecker, WorkoutChecker>()
            .AddSingleton<LastTimeLoader>()
            .AddTransient<WorkoutEditor>();
}
