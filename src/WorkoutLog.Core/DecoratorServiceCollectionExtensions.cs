using Microsoft.Extensions.DependencyInjection;

namespace WorkoutLog.Core;

/// <summary>Registers decorated services.</summary>
internal static class DecoratorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton <typeparamref name="TDecorator"/> wrapping the instance <paramref name="createInner"/> returns.
    /// Only <typeparamref name="TService"/> is registered, so the inner instance can't be resolved on its own.
    /// </summary>
    public static IServiceCollection AddDecoratedSingleton<TService, TDecorator>(
        this IServiceCollection services, Func<IServiceProvider, TService> createInner)
        where TService : class
        where TDecorator : class, TService =>
        services.AddSingleton<TService>(provider => ActivatorUtilities.CreateInstance<TDecorator>(provider, createInner(provider)));
}
