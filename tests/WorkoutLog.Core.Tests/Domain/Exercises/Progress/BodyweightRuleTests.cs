using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Domain.Exercises.Progress;

public class BodyweightRuleTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 20, 17, 0, 0, TimeSpan.Zero);

    private readonly BodyweightRule rule = new(new TopSetRule(), new E1rmRule());

    [Test]
    public void Progress_Unweighted_HasBestSetAndTotalOnly()
    {
        var session = Session(Set(25, null), Set(20, null), Set(18, null));

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, 25, 63, null, null)));
    }

    [Test]
    public void Progress_ZeroKg_IsUnweighted()
    {
        var session = Session(Set(20, 0m), Set(15, 0m));

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, 20, 35, null, null)));
    }

    [Test]
    public void Progress_Weighted_HasTopSetAndE1rmOnly()
    {
        var session = Session(Set(10, 14m), Set(8, 16m));

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, null, null, new ExerciseBest(16m, 8, null, null), 16m * (1 + 8 / 30m))));
    }

    [Test]
    public void Progress_Mixed_SplitsTheSetsByWeight()
    {
        var session = Session(Set(20, null), Set(30, 0m), Set(8, 16m), Set(12, 10m));

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, 30, 50, new ExerciseBest(16m, 8, null, null), 16m * (1 + 8 / 30m))));
    }

    [Test]
    public void Progress_WeightedOver12Reps_HasATopSetButNoE1rm()
    {
        var session = Session(Set(15, 10m));

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, null, null, new ExerciseBest(10m, 15, null, null), null)));
    }

    [Test]
    public void Progress_NoSets_HasNoValues()
    {
        var session = Session();

        Assert.That(rule.Progress(session), Is.EqualTo(new BodyweightSessionProgress(session, null, null, null, null)));
    }

    private static StrengthSession Session(params StrengthSet[] sets) => new(1, Start, sets);

    private static StrengthSet Set(int? reps, decimal? weightKg) => new(reps, weightKg, null, null);
}
