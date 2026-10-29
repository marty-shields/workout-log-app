using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.Charts;
using WorkoutLog.UI.Components.Lists;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Tests.Models.Exercises;

public class AssistedProgressItemsTests
{
    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly AssistedSessionProgress Latest = Session(31, Now.AddDays(-8), [Set(8, 5m), Set(5, null), Set(4, 0m)], Best(0m, 5));

    private static readonly AssistedSessionProgress Earlier = Session(30, Now.AddDays(-15), [Set(8, 10m), Set(6, 5m)], Best(5m, 6));

    [Test]
    public void Points_AreTheLeastAssistLabelledWithReps() =>
        Assert.That(AssistedProgressItems.Points(Progress(Earlier, Latest), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("13 Oct", 5m, "×6"), new ChartPoint("20 Oct", 0m, "×5") }));

    [Test]
    public void Points_SessionWithoutSets_HasNoValue() =>
        Assert.That(AssistedProgressItems.Points(Progress(Session(1, Now, [], null), Latest), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new ChartPoint("28 Oct", null), new ChartPoint("20 Oct", 0m, "×5") }));




    [Test]
    public void Records_ShowTheLeastAssistWithItsDate() =>
        Assert.That(AssistedProgressItems.Records(Progress(new AssistedRecords(new ProgressRecord<ExerciseBest>(Best(0m, 5), Latest.Session.StartedAt))), TimeZoneInfo.Utc, Now),
            Is.EqualTo(new[] { new RecordRow("Least assist", "0kg × 5", "20 Oct") }));

    [Test]
    public void Records_None_ShowNoValue() =>
        Assert.That(AssistedProgressItems.Records(Progress(), TimeZoneInfo.Utc, Now), Is.EqualTo(new[] { new RecordRow("Least assist", null, null) }));

    [Test]
    public void Sessions_AreNewestFirstWithEverySetsAssistBlankAsZero() =>
        Assert.That(AssistedProgressItems.Sessions(Progress(Earlier, Latest), TimeZoneInfo.Utc, Now), Is.EqualTo(new[]
        {
            new ExerciseSession(31, "Tue 20 Oct", "0kg × 5", "5kg×8 · 0kg×5 · 0kg×4"),
            new ExerciseSession(30, "Tue 13 Oct", "5kg × 6", "10kg×8 · 5kg×6"),
        }));

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);

    private static ExerciseBest Best(decimal kg, int reps) => new(kg, reps, null, null);

    private static AssistedSessionProgress Session(int id, DateTimeOffset startedAt, StrengthSet[] sets, ExerciseBest? leastAssist) =>
        new(new StrengthSession(id, startedAt, sets), leastAssist);

    private static AssistedProgress Progress(params AssistedSessionProgress[] sessions) => new(sessions, new AssistedRecords(null));

    private static AssistedProgress Progress(AssistedRecords records) => new([], records);
}
