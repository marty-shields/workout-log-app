using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using SQLite;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;

namespace WorkoutLog.Core.Tests.Services;

public class WorkoutServiceTests
{
    private const int Press = 1;
    private const int Bike = 2;
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly WorkoutErrors NoErrors = new(null, []);
    private static readonly WorkoutErrors Errors = new(null, [new ExerciseErrors(ExerciseError.NoSets, [])]);
    private static readonly WorkoutErrors SaveFailed = new(WorkoutError.SaveFailed, []);

    private FakeTimeProvider time = null!;
    private Mock<IWorkoutRepository> repository = null!;
    private Mock<IWorkoutValidator> validator = null!;
    private FakeLogger<WorkoutService> logger = null!;
    private WorkoutService service = null!;

    [SetUp]
    public void SetUp()
    {
        time = new FakeTimeProvider(Start);
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        validator = new Mock<IWorkoutValidator>();
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(NoErrors);
        logger = new FakeLogger<WorkoutService>();
        service = new WorkoutService(repository.Object, validator.Object, time, logger);
    }

    [Test]
    public void StartNew_StampsTheCurrentTime()
    {
        Assert.That(service.StartNew().StartedAt, Is.EqualTo(Start));
    }

    [Test]
    public void StartNew_LogsNewWorkoutStarted()
    {
        service.StartNew();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutStarted, LogLevel.Information, "New workout started") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task SaveNewAsync_TwoWorkoutsOnTheSameDay_BothSaved()
    {
        repository.SetupSequence(r => r.AddAsync(It.IsAny<NewWorkout>())).ReturnsAsync(7).ReturnsAsync(8);
        var first = service.StartNew();
        time.Advance(TimeSpan.FromHours(1));
        var second = service.StartNew();

        var results = new[] { await service.SaveNewAsync(first, Exercises()), await service.SaveNewAsync(second, Exercises()) };

        Assert.That(results.Select(r => r.WorkoutId), Is.EqualTo(new int?[] { 7, 8 }));
        repository.Verify(r => r.AddAsync(It.IsAny<NewWorkout>()), Times.Exactly(2));
    }

    [Test]
    public void Validate_ReturnsTheValidatorsErrorsForACopy()
    {
        var exercises = Exercises();
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(Errors);

        Assert.That(service.Validate(exercises), Is.SameAs(Errors));
        validator.Verify(v => v.Validate(It.Is<IReadOnlyList<WorkoutExercise>>(e => e != exercises && Same(e, exercises))), Times.Once);
    }

    [Test]
    public void ValidateSet_ChecksTheSetAsAStrengthExercise()
    {
        var set = new StrengthSet(10, null, null, null);
        var errors = new ExerciseErrors(null, [new EntryErrors(new Dictionary<Metric, FieldError>())]);
        validator.Setup(v => v.Validate(It.IsAny<WorkoutExercise>())).Returns(errors);

        Assert.That(service.ValidateSet(Press, set), Is.SameAs(errors));
        validator.Verify(v => v.Validate(It.Is<WorkoutExercise>(e =>
            e is StrengthExercise && e.ExerciseId == Press && ((StrengthExercise)e).Sets.Single() == set)), Times.Once);
    }

    [Test]
    public void ValidateEntry_ChecksTheEntryAsACardioExercise()
    {
        var entry = new CardioEntry(TimeSpan.FromMinutes(5), null, null, null, null, null, null, null);
        var errors = new ExerciseErrors(null, [new EntryErrors(new Dictionary<Metric, FieldError>())]);
        validator.Setup(v => v.Validate(It.IsAny<WorkoutExercise>())).Returns(errors);

        Assert.That(service.ValidateEntry(Bike, entry), Is.SameAs(errors));
        validator.Verify(v => v.Validate(new CardioExercise(Bike, entry)), Times.Once);
    }

    [Test]
    public async Task SaveNewAsync_Valid_SavesWithTheCurrentTimeAsFinish()
    {
        var draft = service.StartNew();
        time.Advance(TimeSpan.FromMinutes(62));
        NewWorkout? saved = null;
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).Callback<NewWorkout>(w => saved = w).ReturnsAsync(7);

        var result = await service.SaveNewAsync(draft, Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(7, NoErrors)));
        Assert.That(saved!.StartedAt, Is.EqualTo(Start));
        Assert.That(saved.FinishedAt, Is.EqualTo(Start.AddMinutes(62)));
        Assert.That(Same(saved.Exercises, Exercises()), Is.True);
    }

    [Test]
    public async Task SaveNewAsync_ClockBeforeTheStart_FinishesAtTheStart()
    {
        var startedAt = Start.AddMinutes(30);
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).ReturnsAsync(7);

        await service.SaveNewAsync(new WorkoutDraft(startedAt), Exercises());

        repository.Verify(r => r.AddAsync(It.Is<NewWorkout>(w => w.StartedAt == startedAt && w.FinishedAt == startedAt)), Times.Once);
    }

    [Test]
    public async Task SaveNewAsync_Invalid_ReturnsTheErrorsAndSavesNothing()
    {
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(Errors);

        var result = await service.SaveNewAsync(service.StartNew(), Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(null, Errors)));
        repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SaveNewAsync_CallerChangesTheListDuringTheSave_SavesWhatWasValidated()
    {
        var sets = new List<StrengthSet> { new(10, 20m, null, null) };
        var exercises = new List<WorkoutExercise> { new StrengthExercise(Press, sets) };
        var write = new TaskCompletionSource<int>();
        NewWorkout? saved = null;
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).Callback<NewWorkout>(w => saved = w).Returns(write.Task);

        var pending = service.SaveNewAsync(service.StartNew(), exercises);
        sets.Add(new StrengthSet(-5, -1m, null, null));
        exercises.Add(new StrengthExercise(99, []));
        write.SetResult(7);
        await pending;

        Assert.That(saved!.Exercises, Has.Count.EqualTo(1));
        Assert.That(((StrengthExercise)saved.Exercises[0]).Sets, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SaveNewAsync_RepositoryThrows_ReturnsSaveFailed()
    {
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).ThrowsAsync(SQLiteException.New(SQLite3.Result.Full, "disk full"));

        var result = await service.SaveNewAsync(service.StartNew(), Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(null, SaveFailed)));
    }

    [Test]
    public async Task SaveNewAsync_StartAlreadySaved_ReturnsSaveFailedAndKeepsTheFirst()
    {
        var path = TempDatabase.NewPath();
        using var provider = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddCore(path)
            .AddSingleton<TimeProvider>(time)
            .BuildServiceProvider();
        try
        {
            await provider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            var saving = provider.GetRequiredService<IWorkoutService>();
            WorkoutExercise[] exercises = [new StrengthExercise(Press, [new StrengthSet(10, 20m, null, null)])];
            var first = await saving.SaveNewAsync(saving.StartNew(), exercises);

            var second = await saving.SaveNewAsync(saving.StartNew(), exercises);

            Assert.That(first.WorkoutId, Is.Not.Null);
            Assert.That(second, Is.EqualTo(new SaveResult(null, SaveFailed)));
            Assert.That(await provider.GetRequiredService<IWorkoutRepository>().ListAsync(),
                Is.EqualTo(new[] { new WorkoutSummary(first.WorkoutId!.Value, Start) }));
        }
        finally
        {
            SQLiteAsyncConnection.ResetPool();
            File.Delete(path);
        }
    }

    [Test]
    public async Task SaveAsync_Valid_ReplacesTheExercisesOnly()
    {
        IReadOnlyList<WorkoutExercise>? saved = null;
        repository.Setup(r => r.ReplaceExercisesAsync(7, It.IsAny<IReadOnlyList<WorkoutExercise>>()))
            .Callback<int, IReadOnlyList<WorkoutExercise>>((_, e) => saved = e).Returns(Task.CompletedTask);

        var result = await service.SaveAsync(7, Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(7, NoErrors)));
        Assert.That(Same(saved!, Exercises()), Is.True);
        repository.Verify(r => r.ReplaceExercisesAsync(7, It.IsAny<IReadOnlyList<WorkoutExercise>>()), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SaveAsync_Invalid_ReturnsTheErrorsAndSavesNothing()
    {
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(Errors);

        var result = await service.SaveAsync(7, Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(null, Errors)));
        repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SaveAsync_MissingWorkout_ReturnsSaveFailed()
    {
        repository.Setup(r => r.ReplaceExercisesAsync(7, It.IsAny<IReadOnlyList<WorkoutExercise>>())).ThrowsAsync(new KeyNotFoundException());

        var result = await service.SaveAsync(7, Exercises());

        Assert.That(result, Is.EqualTo(new SaveResult(null, SaveFailed)));
    }

    [Test]
    public async Task SaveNewAsync_Saved_LogsTheIdAndExerciseCount()
    {
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).ReturnsAsync(7);

        await service.SaveNewAsync(new WorkoutDraft(Start), Exercises());

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.WorkoutAdded, LogLevel.Information, "Workout 7 added with 2 exercises"),
        }));
    }

    [Test]
    public async Task SaveNewAsync_ClockBeforeTheStart_LogsAWarning()
    {
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).ReturnsAsync(7);

        await service.SaveNewAsync(new WorkoutDraft(Start.AddMinutes(30)), Exercises());

        Assert.That(LoggedEvents.Of(logger)[0], Is.EqualTo(
            (CoreEvent.ClockBeforeStart, LogLevel.Warning, "The clock is 00:30:00 behind the workout start; the finish is set to the start")));
    }

    [Test]
    public async Task SaveNewAsync_RepositoryThrows_LogsTheException()
    {
        var exception = SQLiteException.New(SQLite3.Result.Full, "disk full");
        repository.Setup(r => r.AddAsync(It.IsAny<NewWorkout>())).ThrowsAsync(exception);

        await service.SaveNewAsync(new WorkoutDraft(Start), Exercises());

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutAddFailed, LogLevel.Error, "Adding a workout failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task SaveAsync_Saved_LogsTheIdAndExerciseCount()
    {
        repository.Setup(r => r.ReplaceExercisesAsync(7, It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(Task.CompletedTask);

        await service.SaveAsync(7, Exercises());

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.WorkoutUpdated, LogLevel.Information, "Workout 7 updated with 2 exercises"),
        }));
    }

    [Test]
    public async Task SaveAsync_RepositoryThrows_LogsTheIdAndException()
    {
        var exception = new KeyNotFoundException();
        repository.Setup(r => r.ReplaceExercisesAsync(7, It.IsAny<IReadOnlyList<WorkoutExercise>>())).ThrowsAsync(exception);

        await service.SaveAsync(7, Exercises());

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutUpdateFailed, LogLevel.Error, "Updating workout 7 failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task GetAsync_Found_ReturnsTheWorkoutAndLogsTheIdAndExerciseCount()
    {
        var workout = new Workout(7, Start, Start.AddHours(1), Exercises());
        repository.Setup(r => r.GetAsync(7)).ReturnsAsync(workout);

        var result = await service.GetAsync(7);

        Assert.That(result, Is.EqualTo(new WorkoutResult(workout, WorkoutLoad.Loaded)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutLoaded, LogLevel.Debug, "Workout 7 loaded with 2 exercises") }));
    }

    [Test]
    public async Task GetAsync_Missing_ReturnsNotFoundAndLogsAWarning()
    {
        repository.Setup(r => r.GetAsync(7)).ReturnsAsync((Workout?)null);

        var result = await service.GetAsync(7);

        Assert.That(result, Is.EqualTo(new WorkoutResult(null, WorkoutLoad.NotFound)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutNotFound, LogLevel.Warning, "Workout 7 not found") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task GetAsync_RepositoryThrows_ReturnsFailedAndLogsTheIdAndException()
    {
        var exception = SQLiteException.New(SQLite3.Result.IOError, "disk I/O error");
        repository.Setup(r => r.GetAsync(7)).ThrowsAsync(exception);

        var result = await service.GetAsync(7);

        Assert.That(result, Is.EqualTo(new WorkoutResult(null, WorkoutLoad.Failed)));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutLoadFailed, LogLevel.Error, "Loading workout 7 failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task DeleteAsync_Deleted_ReturnsDeletedAndLogsTheId()
    {
        repository.Setup(r => r.DeleteAsync(7)).Returns(Task.CompletedTask);

        var result = await service.DeleteAsync(7);

        Assert.That(result, Is.EqualTo(WorkoutDelete.Deleted));
        repository.Verify(r => r.DeleteAsync(7), Times.Once);
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutDeleted, LogLevel.Information, "Workout 7 deleted") }));
        Assert.That(logger.LatestRecord.Exception, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_RepositoryThrows_ReturnsFailedAndLogsTheIdAndException()
    {
        var exception = SQLiteException.New(SQLite3.Result.IOError, "disk I/O error");
        repository.Setup(r => r.DeleteAsync(7)).ThrowsAsync(exception);

        var result = await service.DeleteAsync(7);

        Assert.That(result, Is.EqualTo(WorkoutDelete.Failed));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutDeleteFailed, LogLevel.Error, "Deleting workout 7 failed") }));
        Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
    }

    [Test]
    public async Task DeleteAsync_MissingWorkout_ReturnsFailed()
    {
        repository.Setup(r => r.DeleteAsync(7)).ThrowsAsync(new KeyNotFoundException());

        var result = await service.DeleteAsync(7);

        Assert.That(result, Is.EqualTo(WorkoutDelete.Failed));
        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.WorkoutDeleteFailed, LogLevel.Error, "Deleting workout 7 failed") }));
    }

    [Test]
    public async Task Invalid_LogsNothing()
    {
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>())).Returns(Errors);

        await service.SaveNewAsync(new WorkoutDraft(Start), Exercises());
        await service.SaveAsync(7, Exercises());

        Assert.That(LoggedEvents.Of(logger), Is.Empty);
    }

    [Test]
    public void NullInput_ThrowsArgumentNullException()
    {
        var draft = service.StartNew();

        Assert.Multiple(() =>
        {
            Assert.That(() => service.Validate(null!), Throws.ArgumentNullException);
            Assert.That(() => service.Validate([null!]), Throws.ArgumentNullException);
            Assert.That(() => service.Validate([new StrengthExercise(Press, null!)]), Throws.ArgumentNullException);
            Assert.That(() => service.Validate([new StrengthExercise(Press, [null!])]), Throws.ArgumentNullException);
            Assert.That(() => service.Validate([new CardioExercise(Bike, null!)]), Throws.ArgumentNullException);
            Assert.That(() => service.ValidateSet(Press, null!), Throws.ArgumentNullException);
            Assert.That(() => service.ValidateEntry(Bike, null!), Throws.ArgumentNullException);
            Assert.That(() => service.SaveNewAsync(null!, Exercises()), Throws.ArgumentNullException);
            Assert.That(() => service.SaveNewAsync(draft, null!), Throws.ArgumentNullException);
            Assert.That(() => service.SaveAsync(7, [null!]), Throws.ArgumentNullException);
        });
        repository.VerifyNoOtherCalls();
    }

    private static WorkoutExercise[] Exercises() =>
    [
        new StrengthExercise(Press, [new StrengthSet(10, 20m, null, null)]),
        new CardioExercise(Bike, new CardioEntry(TimeSpan.FromMinutes(20), null, null, null, null, null, null, null)),
    ];

    private static bool Same(IReadOnlyList<WorkoutExercise> actual, IReadOnlyList<WorkoutExercise> expected) =>
        actual.Count == expected.Count && actual.Zip(expected).All(p => p switch
        {
            (StrengthExercise a, StrengthExercise e) => a.ExerciseId == e.ExerciseId && a.Sets.SequenceEqual(e.Sets),
            _ => Equals(p.First, p.Second),
        });
}
