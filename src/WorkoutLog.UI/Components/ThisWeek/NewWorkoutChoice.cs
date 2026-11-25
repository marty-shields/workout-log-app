using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.ThisWeek;

namespace WorkoutLog.UI.Components.ThisWeek;

/// <summary>
/// What every + button goes through, on This week and Trends alike. The page renders the kit's <c>ChoiceSheet</c> from <see cref="IsOpen"/>, <see cref="Message"/>
/// and the handlers here, and calls <see cref="AskAsync"/> from its + button.
/// </summary>
public sealed class NewWorkoutChoice(IUnfinishedWorkoutService unfinishedWorkout, TimeProvider clock)
{
    public const string Title = "Workout in progress";
    public const string ResumeText = "Resume";
    public const string DiscardText = "Discard and start new";
    public const string TestId = "new-unfinished";

    private UnfinishedItem? item;
    private bool busy;

    /// <summary>Raised when there is no unfinished workout, or after Discard and start new has discarded it.</summary>
    public Func<Task> OnNew { get; set; } = () => Task.CompletedTask;

    /// <summary>Raised by Resume.</summary>
    public Func<Task> OnResume { get; set; } = () => Task.CompletedTask;

    public bool IsOpen => item is not null;

    public string? Message => item is null ? null : $"You haven't finished the workout you started. Resume it, or discard it to start a new one: {item.Lost}.";

    /// <summary>
    /// Reads the unfinished workout afresh: with none it raises <see cref="OnNew"/>, with one it opens the sheet that offers
    /// Resume or Discard and start new, so a new workout never replaces it. If the read fails nothing happens, so a stored workout is never started over; Core logs it.
    /// </summary>
    public async Task AskAsync()
    {
        if (busy)
        {
            return;
        }

        busy = true;
        try
        {
            switch (await unfinishedWorkout.GetSummaryAsync())
            {
                case NoUnfinishedWorkout:
                    await OnNew();
                    break;
                case UnfinishedSummary summary:
                    item = UnfinishedItem.From(summary, clock.LocalTimeZone, clock.GetUtcNow());
                    break;
                case ReadFailed:
                    break;
            }
        }
        finally
        {
            busy = false;
        }
    }

    public void Close() => item = null;

    public async Task ResumeAsync()
    {
        item = null;
        await OnResume();
    }

    /// <summary>Discards the unfinished workout, then raises <see cref="OnNew"/>. A failed write keeps it and starts nothing; Core logs it.</summary>
    public async Task DiscardAndStartAsync()
    {
        item = null;
        if (await unfinishedWorkout.DiscardAsync() == UnfinishedWrite.Done)
        {
            await OnNew();
        }
    }
}
