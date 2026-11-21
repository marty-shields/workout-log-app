using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Haptics;
using WorkoutLog.UI.Import;

namespace WorkoutLog.UI.Stories.Samples;

public static class StorySampleServices
{
    /// <summary>Registers the services the stories' screens resolve, backed by sample data.</summary>
    public static IServiceCollection AddWorkoutLogStorySamples(this IServiceCollection services) => services
        .AddSingleton(TimeProvider.System)
        .AddSingleton<IWorkoutService, SampleWorkoutService>()
        .AddSingleton<IExerciseCatalog, ExerciseCatalog>()
        .AddSingleton<IImportService, SampleImportService>()
        .AddSingleton<ILoggedExercisesService, SampleLoggedExercisesService>()
        .AddSingleton<ILastSessionService, SampleLastSessionService>()
        .AddSingleton<IImportFilePicker, SampleImportFilePicker>()
        .AddSingleton<IHaptics, SampleHaptics>();
}
