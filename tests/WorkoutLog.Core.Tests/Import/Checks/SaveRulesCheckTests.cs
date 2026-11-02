using Moq;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class SaveRulesCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>(), DateTimeOffset.UnixEpoch);
    private readonly SaveRulesCheck check = new(new WorkoutValidator(new ExerciseCatalog(), new NoteLengthRule()));

    [Test]
    public void Check_ValidWorkout_Passes() =>
        Assert.That(check.Check(At(0, Bench, Run), Context), Is.Empty);

    [Test]
    public void Check_PassesTheNoteAndDomainExercisesToTheValidator()
    {
        string? note = null;
        IReadOnlyList<WorkoutExercise>? validated = null;
        var validator = new Mock<IWorkoutValidator>(MockBehavior.Strict);
        validator.Setup(v => v.Validate(It.IsAny<string?>(), It.IsAny<IReadOnlyList<WorkoutExercise>>()))
            .Callback<string?, IReadOnlyList<WorkoutExercise>>((n, e) => (note, validated) = (n, e))
            .Returns(new WorkoutErrors([], []));

        new SaveRulesCheck(validator.Object).Check(At(0, Bench with { Note = "Grip" }, Run) with { Note = "Felt strong" }, Context);

        Assert.That(note, Is.EqualTo("Felt strong"));
        Assert.That(validated, Is.EqualTo(new[] { new StrengthExercise(8, [new StrengthSet(8, 60m, null, null)], "Grip"), Run.ToDomain() }).UsingPropertiesComparer());
    }

    [Test]
    public void Check_WorkoutNoteOver500Characters_FailsSaveRules() =>
        Assert.That(check.Check(At(0) with { Note = new string('n', 501) }, Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_ExerciseNoteOver500Characters_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Note = new string('n', 501) }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_NotesOf500CharactersInsideWhitespace_Pass() =>
        Assert.That(check.Check(At(0, Bench with { Note = $" {new string('n', 500)} " }) with { Note = $"\n{new string('n', 500)}" }, Context), Is.Empty);

    [Test]
    public void Check_NoExercises_FailsSaveRules() =>
        Assert.That(check.Check(At(0) with { Exercises = [] }, Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_StrengthWithNoSets_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MissingRequiredMetric_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(WeightKg: 60m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_ValueOverTheLimit_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8, 100_000m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MetricTheExerciseDoesntShow_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8, Distance: 5m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_SetsForACardioExercise_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Run with { Sets = [new ImportSet(8)], Entry = null }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MissingWeight_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MoreThanTwoDecimalPlaces_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8, 60.125m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_ElevenSets_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [.. Enumerable.Repeat(new ImportSet(8, 60m), 11)] }), Context),
            Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_ElevenExercises_FailsSaveRules() =>
        Assert.That(check.Check(At(0, [.. Enumerable.Range(1, 11).Select(id => Bench with { Exercise = new ImportExerciseInfo(id) })]), Context),
            Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [TestCase(300)]
    [TestCase(313)]
    public void Check_RateOnAnExerciseWithWorkedOutSpeed_FailsSaveRules(int exerciseId) =>
        Assert.That(check.Check(At(0, new ImportExercise(new ImportExerciseInfo(exerciseId), Entry: new ImportEntry(TimeSpan.FromMinutes(30), 5m, Rate: 10m))), Context),
            Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_RepeatedExercise_LeftToRepeatedExerciseCheck() =>
        Assert.That(check.Check(At(0, Bench, Bench), Context), Is.Empty);

    [Test]
    public void Check_UnknownExercise_LeftToUnknownExerciseCheck() =>
        Assert.That(check.Check(At(0, Bench, Bench with { Exercise = new ImportExerciseInfo(999) }), Context), Is.Empty);
}
