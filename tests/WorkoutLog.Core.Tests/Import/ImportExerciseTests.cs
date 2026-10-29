using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Import;

namespace WorkoutLog.Core.Tests.Import;

public class ImportExerciseTests
{
    private static readonly ImportExerciseInfo Info = new(8);

    [Test]
    public void IsWellFormed_SetsOrEntryAlone_IsTrue()
    {
        Assert.That(new ImportExercise(Info, []).IsWellFormed, Is.True);
        Assert.That(new ImportExercise(Info, [new ImportSet(1)]).IsWellFormed, Is.True);
        Assert.That(new ImportExercise(Info, Entry: new ImportEntry()).IsWellFormed, Is.True);
    }

    [Test]
    public void IsWellFormed_NeitherBothOrANullSet_IsFalse()
    {
        Assert.That(new ImportExercise(Info).IsWellFormed, Is.False);
        Assert.That(new ImportExercise(Info, [], new ImportEntry()).IsWellFormed, Is.False);
        Assert.That(new ImportExercise(Info, [new ImportSet(1), null!]).IsWellFormed, Is.False);
    }

    [Test]
    public void ToDomain_Sets_IsStrengthWithEveryMetric()
    {
        var exercise = new ImportExercise(Info, [new ImportSet(1, 2.5m, TimeSpan.FromSeconds(3), 4m), new ImportSet(5)]);

        Assert.That(exercise.ToDomain(), Is.EqualTo(new StrengthExercise(8,
            [new StrengthSet(1, 2.5m, TimeSpan.FromSeconds(3), 4m), new StrengthSet(5, null, null, null)])).UsingPropertiesComparer());
    }

    [Test]
    public void ToDomain_Entry_IsCardioWithEveryMetric()
    {
        var exercise = new ImportExercise(Info, Entry: new ImportEntry(TimeSpan.FromMinutes(1), 2m, 3, 4, 5m, 6, 7m));

        Assert.That(exercise.ToDomain(), Is.EqualTo(new CardioExercise(8,
            new CardioEntry(TimeSpan.FromMinutes(1), 2m, 3, 4, 5m, 6, 7m))).UsingPropertiesComparer());
    }

    [Test]
    public void ToDomain_Neither_Throws()
    {
        Assert.That(() => new ImportExercise(Info).ToDomain(), Throws.InvalidOperationException);
    }
}
