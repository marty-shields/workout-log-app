using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI;

/// <summary>Registers the UI's services.</summary>
public static class UiServiceCollectionExtensions
{
    /// <summary>Registers the back button and the set and workout checkers as singletons. The host must register Core's services and logging.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddUi(this IServiceCollection services) =>
        services
            .AddSingleton<IBackButton, BackButton>()
            .AddSingleton<ISetChecker, SetChecker>()
            .AddSingleton<IWorkoutChecker, WorkoutChecker>();
}
