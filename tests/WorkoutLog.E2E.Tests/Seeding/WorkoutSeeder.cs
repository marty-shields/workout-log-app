using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SQLite;
using WorkoutLog.Core;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.E2E.Tests.Device;

namespace WorkoutLog.E2E.Tests.Seeding;

/// <summary>
/// Builds a database on the PC with Core's own services, each workout saved at its start and finish time, then copies it into
/// the Debug app over adb and starts the app. Ids are 1…n in the order given.
/// </summary>
public sealed class WorkoutSeeder(Native native)
{
    private const string DeviceTemp = "/data/local/tmp/workoutlog-seed.db3";
    private const string AppDatabase = "files/workoutlog.db3";

    public async Task SeedAsync(IReadOnlyList<SeedWorkout> workouts)
    {
        var folder = Directory.CreateTempSubdirectory("workoutlog-seed-");
        try
        {
            var path = Path.Combine(folder.FullName, "workoutlog.db3");
            await BuildAsync(path, workouts);
            Install(path);
        }
        finally
        {
            try
            {
                folder.Delete(recursive: true);
            }
            catch (IOException e)
            {
                TestContext.Progress.WriteLine($"Couldn't remove the seed folder: {e.Message}");
            }
        }

        native.Launch();
    }

    private static async Task BuildAsync(string path, IReadOnlyList<SeedWorkout> workouts)
    {
        var clock = new SeedClock();
        await using var services = new ServiceCollection()
            .AddCore(path)
            .AddSingleton<TimeProvider>(clock)
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .BuildServiceProvider();
        try
        {
            await services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var workoutService = services.GetRequiredService<IWorkoutService>();
            for (var i = 0; i < workouts.Count; i++)
            {
                var workout = workouts[i];
                clock.Now = workout.StartedAt;
                var draft = workoutService.StartNew();
                clock.Now = workout.FinishedAt;
                var result = await workoutService.SaveNewAsync(draft, workout.Exercises);
                if (result.WorkoutId != i + 1)
                {
                    throw new InvalidOperationException($"Seeding workout {i + 1} failed: {result.Errors.Error}, {result.Errors.Exercises.Count} exercise errors.");
                }
            }
        }
        finally
        {
            SQLiteAsyncConnection.ResetPool();
        }
    }

    /// <summary>Stops the app and replaces its database with the file at <paramref name="path"/>. The app need not have run yet.</summary>
    private static void Install(string path)
    {
        var adb = AppiumSetup.Adb;
        AppiumSetup.Driver.TerminateApp(AppiumSetup.Package);
        adb.Run("push", path, DeviceTemp);
        try
        {
            adb.Shell("chmod", "644", DeviceTemp);
            adb.Shell("run-as", AppiumSetup.Package, "mkdir", "-p", "files");
            adb.Shell("run-as", AppiumSetup.Package, "rm", "-f", $"{AppDatabase}-journal", $"{AppDatabase}-wal", $"{AppDatabase}-shm");
            adb.Shell("run-as", AppiumSetup.Package, "cp", DeviceTemp, AppDatabase);
        }
        finally
        {
            if (!adb.TryRun(out var output, "shell", "rm", "-f", DeviceTemp))
            {
                TestContext.Progress.WriteLine($"Couldn't remove the seed file from the device: {output}");
            }
        }
    }

    /// <summary>A clock set by hand, forwards or back.</summary>
    private sealed class SeedClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }

        public override DateTimeOffset GetUtcNow() => Now.ToUniversalTime();
    }
}

/// <summary>A workout to seed: its start and finish, and its exercises in order.</summary>
public sealed record SeedWorkout(DateTimeOffset StartedAt, DateTimeOffset FinishedAt, IReadOnlyList<WorkoutExercise> Exercises);
