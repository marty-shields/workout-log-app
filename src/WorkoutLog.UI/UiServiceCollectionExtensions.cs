using Microsoft.Extensions.DependencyInjection;
using Shields.UI;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Import;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI;

public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the UI's services and the kit's (<see cref="ShieldsUiServiceCollectionExtensions.AddShieldsUi"/>, which gives the theme, system-bar, back button and app lifecycle services). The host must register Core's services, logging, an <see cref="IImportFilePicker"/> and an <see cref="Haptics.IHaptics"/>.</summary>
    public static IServiceCollection AddUi(this IServiceCollection services) =>
        services
            .AddShieldsUi()
            .AddSingleton<ImportSession>()
            .AddSingleton<IImportFileOpener, ImportFileOpener>()
            .AddSingleton<ISetChecker, SetChecker>()
            .AddSingleton<IWorkoutChecker, WorkoutChecker>()
            .AddSingleton<LastTimeLoader>()
            .AddSingleton<UnfinishedAutosaver>()
            .AddTransient<WorkoutEditor>()
            .AddTransient<NewWorkoutChoice>();
}
