using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Tests.Domain.Exercises;

public class MuscleGroupMapTests
{
    private readonly MuscleGroupMap map = new();

    private static IEnumerable<TestCaseData> Strength() =>
        new ExerciseCatalog().All
            .Where(e => e.Type == ExerciseType.Strength)
            .Select(e => new TestCaseData(e).SetArgDisplayNames($"{e.Id} {e.Name}"));

    [TestCase(Muscle.Chest, MuscleGroup.Chest)]
    [TestCase(Muscle.Lats, MuscleGroup.Back)]
    [TestCase(Muscle.MiddleBack, MuscleGroup.Back)]
    [TestCase(Muscle.LowerBack, MuscleGroup.Back)]
    [TestCase(Muscle.Traps, MuscleGroup.Back)]
    [TestCase(Muscle.Neck, MuscleGroup.Back)]
    [TestCase(Muscle.Shoulders, MuscleGroup.Shoulders)]
    [TestCase(Muscle.Biceps, MuscleGroup.Arms)]
    [TestCase(Muscle.Triceps, MuscleGroup.Arms)]
    [TestCase(Muscle.Forearms, MuscleGroup.Arms)]
    [TestCase(Muscle.Quadriceps, MuscleGroup.Legs)]
    [TestCase(Muscle.Hamstrings, MuscleGroup.Legs)]
    [TestCase(Muscle.Glutes, MuscleGroup.Legs)]
    [TestCase(Muscle.Calves, MuscleGroup.Legs)]
    [TestCase(Muscle.Abductors, MuscleGroup.Legs)]
    [TestCase(Muscle.Adductors, MuscleGroup.Legs)]
    [TestCase(Muscle.Abdominals, MuscleGroup.Core)]
    public void GroupOf_ReturnsTheMusclesGroup(Muscle muscle, MuscleGroup group) =>
        Assert.That(map.GroupOf(muscle), Is.EqualTo(group));

    [Test]
    public void GroupOf_UnmappedMuscle_ReturnsNull() =>
        Assert.That(map.GroupOf((Muscle)999), Is.Null);

    [TestCaseSource(nameof(Strength))]
    public void StrengthExercise_PrimaryMuscleIsMapped(ExerciseDefinition exercise) =>
        Assert.That(exercise.PrimaryMuscle is { } muscle ? map.GroupOf(muscle) : null, Is.Not.Null,
            $"{exercise.Name}: primary muscle {exercise.PrimaryMuscle} has no muscle group");
}
