using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class RecordsRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly RecordsRule rule = new();

    [Test]
    public void Records_NoSessions_AreNone() =>
        Assert.That(rule.Records([]), Is.EqualTo(new LiftRecords(null, null)));

    [Test]
    public void Records_TakeTheHeaviestTopSetAndTheBestE1rm()
    {
        var records = rule.Records(
        [
            Session(Start.AddDays(-14), Top(65m, 8), 82.33m),
            Session(Start, Top(70m, 3), 77m),
        ]);

        Assert.That(records, Is.EqualTo(new LiftRecords(
            new ProgressRecord<ExerciseBest>(Top(70m, 3), Start),
            new ProgressRecord<decimal>(82.33m, Start.AddDays(-14)))));
    }

    [Test]
    public void HeaviestWeight_TiedWeight_TakesTheMostReps() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), Top(70m, 3), null), Session(Start, Top(70m, 5), null)]).HeaviestWeight,
            Is.EqualTo(new ProgressRecord<ExerciseBest>(Top(70m, 5), Start)));

    [Test]
    public void Records_TiedAcrossSessions_TakeTheEarliest() =>
        Assert.That(rule.Records(
            [
                Session(Start, Top(70m, 3), 77m),
                Session(Start.AddDays(-7), Top(70m, 3), 77m),
                Session(Start.AddDays(-3), Top(70m, 3), 77m),
            ]),
            Is.EqualTo(new LiftRecords(
                new ProgressRecord<ExerciseBest>(Top(70m, 3), Start.AddDays(-7)),
                new ProgressRecord<decimal>(77m, Start.AddDays(-7)))));

    [Test]
    public void Records_ComparesE1rmUnrounded() =>
        Assert.That(rule.Records([Session(Start.AddDays(-7), null, 82.34m), Session(Start, null, 82.341m)]).BestE1rmKg,
            Is.EqualTo(new ProgressRecord<decimal>(82.341m, Start)));

    [Test]
    public void Records_SessionsWithoutValues_AreNone() =>
        Assert.That(rule.Records([Session(Start, null, null)]), Is.EqualTo(new LiftRecords(null, null)));

    private static ExerciseBest Top(decimal kg, int reps) => new(kg, reps, null, null);

    private static LiftSessionProgress Session(DateTimeOffset startedAt, ExerciseBest? topSet, decimal? e1rm) =>
        new(new LiftSession(1, startedAt, []), topSet, e1rm);
}
