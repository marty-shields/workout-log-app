using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Logging;
using WorkoutLog.Core.Services;
using static WorkoutLog.Core.Tests.Domain.Exercises.Bests.BestRuleCases;

namespace WorkoutLog.Core.Tests.Services;

public class MuscleSetsServiceTests
{
    private const int UnknownId = 9999;

    /// <summary>Wednesday 28 Oct 2026, 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Thursday 22 Oct 2026, 00:00 UTC: 00:00 six days before <see cref="Now"/>.</summary>
    private static readonly DateTimeOffset WindowStart = new(2026, 10, 22, 0, 0, 0, TimeSpan.Zero);

    private Mock<IWorkoutRepository> repository = null!;
    private FakeLogger<MuscleSetsService> logger = null!;
    private MuscleSetsService service = null!;

    [SetUp]
    public void SetUp()
    {
        repository = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        logger = new FakeLogger<MuscleSetsService>();
        UseClock(Now);
    }

    [Test]
    public void Exercises_HaveTheNamedPrimaryMuscles() =>
        Assert.That(new[] { Squat, Curl, PushUp, Plank, Run }.Select(id => Definition(id).PrimaryMuscle),
            Is.EqualTo(new Muscle?[] { Muscle.Quadriceps, Muscle.Biceps, Muscle.Chest, Muscle.Abdominals, null }));

    [Test]
    public async Task GetAsync_AddsUpEachGroupsSetsInOrder()
    {
        SetUpRead(WindowStart, Now,
            new ExerciseSetCount(1, Squat, 3),
            new ExerciseSetCount(1, Curl, 2),
            new ExerciseSetCount(2, Squat, 4),
            new ExerciseSetCount(2, Plank, 1));

        var result = await service.GetAsync();

        Assert.That(result, Is.EqualTo(new MuscleSetsResult(
        [
            new MuscleGroupCount(MuscleGroup.Chest, 0),
            new MuscleGroupCount(MuscleGroup.Back, 0),
            new MuscleGroupCount(MuscleGroup.Shoulders, 0),
            new MuscleGroupCount(MuscleGroup.Arms, 2),
            new MuscleGroupCount(MuscleGroup.Legs, 7),
            new MuscleGroupCount(MuscleGroup.Core, 1),
        ], Failed: false)).UsingPropertiesComparer());
    }

    [Test]
    public async Task GetAsync_NothingInTheWindow_EveryGroupIsZero()
    {
        SetUpRead(WindowStart, Now);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Groups.Select(g => g.Group), Is.EqualTo(Enum.GetValues<MuscleGroup>()));
            Assert.That(result.Groups.Select(g => g.Sets), Is.All.Zero);
            Assert.That(result.Failed, Is.False);
        });
    }

    [Test]
    public async Task GetAsync_Cardio_IsNotCounted()
    {
        SetUpRead(WindowStart, Now, new ExerciseSetCount(1, Run, 2), new ExerciseSetCount(1, PushUp, 1));

        var result = await service.GetAsync();

        Assert.That(result.Groups.Select(g => g.Sets), Is.EqualTo(new[] { 1, 0, 0, 0, 0, 0 }));
    }

    [Test]
    public async Task GetAsync_UnknownExercise_IsLeftOutAndLoggedWithItsId()
    {
        SetUpRead(WindowStart, Now, new ExerciseSetCount(1, UnknownId, 5), new ExerciseSetCount(1, PushUp, 2));

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Groups.Select(g => g.Sets), Is.EqualTo(new[] { 2, 0, 0, 0, 0, 0 }));
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
            {
                (CoreEvent.MuscleSetsUnknownExercise, LogLevel.Warning, $"Exercise {UnknownId} is not in the exercise list; left out"),
                (CoreEvent.MuscleSetsLoaded, LogLevel.Information, "Loaded the sets per muscle group from 1 workouts"),
            }));
        });
    }

    [TestCase(0, 0, TestName = "GetAsync_AtMidnight_WindowStartsAt0000SixDaysAgo")]
    [TestCase(23, 59, TestName = "GetAsync_LateEvening_WindowStartsAt0000SixDaysAgo")]
    public async Task GetAsync_WindowIsTodayAndTheSixDaysBefore(int hour, int minute)
    {
        var now = new DateTimeOffset(2026, 10, 28, hour, minute, 0, TimeSpan.Zero);
        UseClock(now);
        SetUpRead(WindowStart, now);

        await service.GetAsync();

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_UsesThePhonesTimeZone()
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("UTC+10", TimeSpan.FromHours(10), "UTC+10", "UTC+10");
        var now = new DateTimeOffset(2026, 10, 28, 20, 0, 0, TimeSpan.Zero);
        UseClock(now, zone);
        SetUpRead(new DateTimeOffset(2026, 10, 23, 0, 0, 0, TimeSpan.FromHours(10)), now);

        await service.GetAsync();

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_MidnightSkipped_WindowStartsAtTheFirstRealInstant()
    {
        var thursday0030UtcMinus2 = new DateTimeOffset(2026, 10, 22, 0, 30, 0, TimeSpan.FromHours(-2));
        UseClock(Now, ClocksJumpFrom2330To0030On22Oct());
        SetUpRead(thursday0030UtcMinus2, Now);

        await service.GetAsync();

        repository.VerifyAll();
    }

    [Test]
    public async Task GetAsync_LogsTheWorkoutCountAtInformation()
    {
        SetUpRead(WindowStart, Now, new ExerciseSetCount(1, Squat, 3), new ExerciseSetCount(1, Curl, 2), new ExerciseSetCount(4, Squat, 1));

        await service.GetAsync();

        Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[]
        {
            (CoreEvent.MuscleSetsLoaded, LogLevel.Information, "Loaded the sets per muscle group from 2 workouts"),
        }));
    }

    [Test]
    public async Task GetAsync_ReadThrows_ReturnsFailedAndLogsAWarningWithTheException()
    {
        var exception = new InvalidOperationException("disk");
        repository.Setup(r => r.GetSetCountsAsync(WindowStart, Now)).ThrowsAsync(exception);

        var result = await service.GetAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new MuscleSetsResult([], Failed: true)).UsingPropertiesComparer());
            Assert.That(LoggedEvents.Of(logger), Is.EqualTo(new[] { (CoreEvent.MuscleSetsLoadFailed, LogLevel.Warning, "Loading the sets per muscle group failed") }));
            Assert.That(logger.LatestRecord.Exception, Is.SameAs(exception));
        });
    }

    /// <summary>Creates the service with a clock at <paramref name="now"/> in <paramref name="zone"/>, UTC by default.</summary>
    private void UseClock(DateTimeOffset now, TimeZoneInfo? zone = null)
    {
        var time = new FakeTimeProvider(now);
        time.SetLocalTimeZone(zone ?? TimeZoneInfo.Utc);
        service = new MuscleSetsService(repository.Object, Catalog, new MuscleGroupMap(), time, logger);
    }

    /// <summary>UTC-3, moving to UTC-2 at 23:30 on Wed 21 Oct, so Thu 22 Oct has no 00:00.</summary>
    private static TimeZoneInfo ClocksJumpFrom2330To0030On22Oct() =>
        TimeZoneInfo.CreateCustomTimeZone("Test", TimeSpan.FromHours(-3), "Test", "Test", "Test",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 23, 30, 0), 10, 21),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 2, 15)),
        ]);

    /// <summary>Sets up the read, which must be from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private void SetUpRead(DateTimeOffset from, DateTimeOffset to, params ExerciseSetCount[] counts) =>
        repository.Setup(r => r.GetSetCountsAsync(from, to)).ReturnsAsync(counts);
}
