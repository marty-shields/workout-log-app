using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Summary;

namespace WorkoutLog.UI.Tests.Models.Summary;

public class SummaryItemsTests
{
    private const int Squat = 6;
    private const int PushUp = 264;
    private const int AssistedPullUp = 218;
    private const int Plank = 258;
    private const int FarmersWalk = 187;
    private const int Run = 311;

    private static readonly ExerciseCatalog Catalog = new();

    [Test]
    public void Summary_CarriesTheTotals()
    {
        var summary = SummaryItems.Summary(Saved(true), Catalog);

        Assert.That((summary.Duration, summary.Exercises, summary.Sets), Is.EqualTo((TimeSpan.FromMinutes(50), 3, 9)));
    }

    [Test]
    public void Summary_RecordsAreTheRecordExercisesWithTheirBestInOrder()
    {
        var summary = SummaryItems.Summary(Saved(true,
            Exercise(Squat, new ExerciseBest(100m, 5, null, null), true, BestChangeKind.Up, new ExerciseBest(5m, null, null, null)),
            Exercise(PushUp, new ExerciseBest(null, 20, null, null), false, BestChangeKind.Equal),
            Exercise(Run, new ExerciseBest(null, null, null, 5.2m), true, BestChangeKind.Up, new ExerciseBest(null, null, null, 0.2m))), Catalog);

        Assert.That(summary.Records.Select(r => (r.ExerciseId, r.Best)), Is.EqualTo(new[] { (Squat, "100kg × 5"), (Run, "5.2km") }));
    }

    [Test]
    public void Summary_NoEarlierWorkout_HasNoChanges()
    {
        var summary = SummaryItems.Summary(Saved(false, Exercise(Squat, new ExerciseBest(100m, 5, null, null), false, BestChangeKind.FirstTime)), Catalog);

        Assert.Multiple(() =>
        {
            Assert.That(summary.Changes, Is.Empty);
            Assert.That(summary.IsFirstWorkout, Is.True);
        });
    }

    [Test]
    public void Summary_EachKindOfChange_ShowsItsDifferenceAsText()
    {
        var summary = SummaryItems.Summary(Saved(true,
            Exercise(Squat, new ExerciseBest(102.5m, 5, null, null), false, BestChangeKind.Up, new ExerciseBest(2.5m, null, null, null)),
            Exercise(PushUp, new ExerciseBest(null, 18, null, null), false, BestChangeKind.Down, new ExerciseBest(null, 1, null, null)),
            Exercise(AssistedPullUp, new ExerciseBest(20m, 8, null, null), false, BestChangeKind.Up, new ExerciseBest(5m, null, null, null)),
            Exercise(Plank, new ExerciseBest(null, null, TimeSpan.FromSeconds(75), null), false, BestChangeKind.Up, new ExerciseBest(null, null, TimeSpan.FromSeconds(15), null)),
            Exercise(FarmersWalk, new ExerciseBest(32m, null, null, 30m), false, BestChangeKind.Down, new ExerciseBest(null, null, null, 10m)),
            Exercise(Run, new ExerciseBest(null, null, null, 5.2m), false, BestChangeKind.Equal)), Catalog);

        Assert.That(summary.Changes.Select(c => (c.Since.Kind, c.Since.Difference)), Is.EqualTo(new (ChangeKind, string?)[]
        {
            (ChangeKind.Up, "2.5kg"),
            (ChangeKind.Down, "1 rep"),
            (ChangeKind.Up, "5kg"),
            (ChangeKind.Up, "0:15"),
            (ChangeKind.Down, "10m"),
            (ChangeKind.Equal, null),
        }));
    }

    [Test]
    public void Summary_FirstTimeExercise_IsFirstTime()
    {
        var summary = SummaryItems.Summary(Saved(true, Exercise(Squat, new ExerciseBest(100m, 5, null, null), false, BestChangeKind.FirstTime)), Catalog);

        Assert.That(summary.Changes.Single().Since, Is.EqualTo(SinceLastTime.FirstTime));
    }

    private static SummaryExercise Exercise(int id, ExerciseBest best, bool record, BestChangeKind kind, ExerciseBest? difference = null) =>
        new(id, best, record, new BestChange(kind, difference));

    private static SavedWorkoutSummary Saved(bool earlier, params SummaryExercise[] exercises) =>
        new(TimeSpan.FromMinutes(50), 3, 9, earlier, exercises);
}
