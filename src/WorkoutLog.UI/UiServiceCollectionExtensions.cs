using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI;

/// <summary>Registers the UI's services.</summary>
public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the back button, the app lifecycle (as <see cref="IAppStart"/> and <see cref="IAppLifecycle"/>, one instance) and the set and workout checkers as singletons. The host must register Core's services and logging.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddUi(this IServiceCollection services) =>
        services
            .AddSingleton<IBackButton, BackButton>()
            .AddSingleton<AppLifecycle>()
            .AddSingleton<IAppStart>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<IAppLifecycle>(s => s.GetRequiredService<AppLifecycle>())
            .AddSingleton<ISetChecker, SetChecker>()
            .AddSingleton<IWorkoutChecker, WorkoutChecker>();
}
