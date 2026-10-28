using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class BodyweightRecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly BodyweightRecordsRule rule = new(new LiftRecordsRule());

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new BodyweightRecords(null, null, null, null)));

    [Test]
    public void Records_TakeTheMostRepsInASetAndASessionAndTheLiftRecords()
    {
        var records = rule.Records(
        [
            Session(Start.AddDays(-14), 25, 50, Top(10m, 8), 12.67m),
            Session(Start, 22, 63, Top(16m, 8), 20.27m),
        ]);

        Assert.That(records, Is.EqualTo(new BodyweightRecords(
            new ProgressRecord<int>(25, Start.AddDays(-14)),
            new ProgressRecord<int>(63, Start),
            new ProgressRecord<ExerciseBest>(Top(16m, 8), Start),
            new ProgressRecord<decimal>(20.27m, Start))));
    }

    [Test]
    public void Records_TiedAcrossSessions_TakeTheEarliest() =>
        Assert.That(rule.Records(
            [
                Session(Start, 25, 60, Top(16m, 8), 20.27m),
                Session(Start.AddDays(-7), 25, 60, Top(16m, 8), 20.27m),
                Session(Start.AddDays(-3), 25, 60, Top(16m, 8), 20.27m),
            ]),
            Is.EqualTo(new BodyweightRecords(
                new ProgressRecord<int>(25, Start.AddDays(-7)),
                new ProgressRecord<int>(60, Start.AddDays(-7)),
                new ProgressRecord<ExerciseBest>(Top(16m, 8), Start.AddDays(-7)),
                new ProgressRecord<decimal>(20.27m, Start.AddDays(-7)))));

    [Test]
    public void Records_SessionsWithoutValues_AreNone() =>
        Assert.That(rule.Records([Session(Start, null, null, null, null)]), Is.EqualTo(new BodyweightRecords(null, null, null, null)));

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);

    private static BodyweightSessionProgress Session(DateTimeOffset startedAt, int? bestSet, int? total, ExerciseBest? topSet, decimal? e1rm) =>
        new(new StrengthSession(1, startedAt, []), bestSet, total, topSet, e1rm);
}
