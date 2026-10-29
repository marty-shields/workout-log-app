using Reqnroll;
using WorkoutLog.E2E.Tests.Device;
using WorkoutLog.E2E.Tests.Seeding;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Saved workouts a scenario starts from: seeded into the database, or saved through the app.</summary>
[Binding]
public sealed class SeedSteps(WorkoutSeeder seeder, DeviceClock clock, ScenarioState state, Journeys journeys)
{
    /// <summary>A seeded workout without a finish time lasts this long.</summary>
    private static readonly TimeSpan DefaultLength = TimeSpan.FromMinutes(45);

    [Given(@"^these saved workouts:$")]
    public async Task GivenTheseSavedWorkouts(DataTable table)
    {
        var now = clock.LocalNow();
        var workouts = table.Rows.Select(row =>
        {
            var started = SeedTime.Resolve(row["started"], now);
            var finished = row.TryGetValue("finished", out var text) ? SeedTime.Resolve(text, now) : started + DefaultLength;
            return (started, finished, row["exercises"]);
        }).ToList();

        await SeedAsync(workouts);
    }

    [Given(@"^(\d+) saved workouts on different days$")]
    public async Task GivenWorkoutsOnDifferentDays(int count)
    {
        var today = clock.LocalNow().Date;
        var workouts = Enumerable.Range(1, count)
            .Select(i => today.AddDays(i - count - 1).AddHours(10))
            .Select(started => (started, started + DefaultLength, "Push-up: 10 reps 0 kg"))
            .ToList();

        await SeedAsync(workouts);
    }

    [Given(@"^I have saved a workout with (.+)$")]
    public void GivenIHaveSavedAWorkoutWith(string exercises) => journeys.SaveWorkout(ExerciseValues.ParseWorkout(exercises));

    private async Task SeedAsync(IReadOnlyList<(DateTime Started, DateTime Finished, string Exercises)> workouts)
    {
        var exercises = workouts.Select(w => ExerciseValues.ParseWorkout(w.Exercises)).ToList();
        var seeds = workouts.Select((w, i) => new SeedWorkout(
            clock.FromLocal(w.Started),
            clock.FromLocal(w.Finished),
            exercises[i].Select(e => e.ToWorkoutExercise()).ToList())).ToList();

        await seeder.SeedAsync(seeds);
        for (var i = 0; i < workouts.Count; i++)
        {
            state.Seeded[i + 1] = workouts[i].Started;
            state.SeededFinished[i + 1] = workouts[i].Finished;
            state.SeededExercises[i + 1] = exercises[i];
        }
    }
}
