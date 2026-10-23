using AngleSharp.Dom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Components.Overlays;
using WorkoutLog.UI.Components.Workout;
using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>A screen state: its stories section and story name, and how to render it.</summary>
/// <param name="Section">Stories title, e.g. "Screens/New Workout".</param>
/// <param name="Name">Story name.</param>
/// <param name="Render">Renders the state with every callback set and returns its elements.</param>
public sealed record ScreenState(string Section, string Name, Func<BunitContext, IReadOnlyList<IElement>> Render)
{
    public override string ToString() => $"{Section}/{Name}";
}

/// <summary>Every screen state that has a story, in story order. Renders mirror the stories, with every callback set.</summary>
public static class ScreenStates
{
    public static IReadOnlyList<ScreenState> All { get; } =
    [
        new("Screens/Startup", "Loading", c => c.Render<StartupView>().FindAll("*")),
        new("Screens/Startup", "Error", c => c.Render<StartupView>(p => p.Add(x => x.Failed, true)).FindAll("*")),

        new("Screens/Dashboard", "Empty", c => Dashboard(c, new DashboardSummary(null, 0, 0))),
        new("Screens/Dashboard", "With workouts", c => Dashboard(c, new DashboardSummary(SampleData.Started, 2, 9))),

        new("Screens/New Workout", "Empty", c => Workout(c, WorkoutMode.New, [])),
        new("Screens/New Workout", "Live timer", NewWorkout),
        new("Screens/New Workout", "Strength sets", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)))),
        new("Screens/New Workout", "Carry sets", c => Workout(c, WorkoutMode.New, SampleData.CarryWorkout(Catalog(c)))),
        new("Screens/New Workout", "Cardio entries", c => Workout(c, WorkoutMode.New, SampleData.CardioWorkout(Catalog(c)))),
        new("Screens/New Workout", "Save errors", c => Workout(c, WorkoutMode.New, SampleData.WorkoutWithErrors(Catalog(c), Checker(c)))),
        new("Screens/New Workout", "Save with no exercises", c => Workout(c, WorkoutMode.New, [], error: "Add at least one exercise")),
        new("Screens/New Workout", "Discard prompt", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), dialog: ("Discard this workout?", "Discard"))),

        new("Screens/Edit Workout", "Saved", c => Workout(c, WorkoutMode.Edit, SampleData.StrengthWorkout(Catalog(c)))),
        new("Screens/Edit Workout", "Carry sets closed", c => Workout(c, WorkoutMode.Edit, SampleData.CarryWorkout(Catalog(c)))),
        new("Screens/Edit Workout", "Save errors", c => Workout(c, WorkoutMode.Edit, SampleData.EditWorkoutWithErrors(Catalog(c), Checker(c)))),
        new("Screens/Edit Workout", "Discard prompt", c => Workout(c, WorkoutMode.Edit, SampleData.StrengthWorkout(Catalog(c)), dialog: ("Discard changes?", "Discard"))),
        new("Screens/Edit Workout", "Delete prompt", c => Workout(c, WorkoutMode.Edit, SampleData.StrengthWorkout(Catalog(c)), dialog: ("Delete this workout?", "Delete"))),

        new("Screens/Exercise Picker", "No search text", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "")),
        new("Screens/Exercise Picker", "Results", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "dumbbell")),
        new("Screens/Exercise Picker", "No match", c => Workout(c, WorkoutMode.New, SampleData.StrengthWorkout(Catalog(c)), pickerQuery: "zumba")),

        new("Screens/History", "Empty", c => History(c, [], tappable: true)),
        new("Screens/History", "With workouts", c => History(c, SampleData.HistoryRows, tappable: true)),
        new("Screens/History", "Rows not tappable", c => History(c, SampleData.HistoryRows, tappable: false)),

        new("Screens/Error", "Rendering error", c => c.Render<ErrorView>(p => p.Add(x => x.OnRestart, () => { })).FindAll("*")),
    ];

    private static IExerciseCatalog Catalog(BunitContext context) => context.Services.GetRequiredService<IExerciseCatalog>();

    private static ISetChecker Checker(BunitContext context) => context.Services.GetRequiredService<ISetChecker>();

    private static IReadOnlyList<IElement> Dashboard(BunitContext context, DashboardSummary summary) =>
        context.Render<DashboardView>(p => p
            .Add(x => x.Summary, summary)
            .Add(x => x.OnNewWorkout, () => { })
            .Add(x => x.OnHistory, () => { }))
            .FindAll("*");

    private static IReadOnlyList<IElement> NewWorkout(BunitContext context)
    {
        var clock = new FakeTimeProvider();
        context.Services.AddSingleton<TimeProvider>(clock);
        return context.Render<NewWorkoutPage>(p => p.Add(x => x.OnBack, () => { })).FindAll("*");
    }

    private static IReadOnlyList<IElement> Workout(
        BunitContext context,
        WorkoutMode mode,
        List<ExerciseState> exercises,
        string? error = null,
        (string Title, string ConfirmText)? dialog = null,
        string? pickerQuery = null) =>
        context.Render<WorkoutView>(p =>
        {
            p.Add(x => x.Mode, mode)
                .Add(x => x.StartedAt, SampleData.Started)
                .Add(x => x.Time, new TimeSpan(0, 34, 12))
                .Add(x => x.Exercises, exercises)
                .Add(x => x.Error, error)
                .Add(x => x.ValidateSet, Checker(context).Check)
                .Add(x => x.OnAddExercise, () => { })
                .Add(x => x.OnRemoveExercise, (ExerciseState _) => { })
                .Add(x => x.OnSave, () => { })
                .Add(x => x.OnBack, () => { });
            if (mode == WorkoutMode.Edit)
            {
                p.Add(x => x.OnDelete, () => { });
            }

            if (dialog is { } d)
            {
                p.Add<ConfirmDialog>(x => x.Overlay, o => o
                    .Add(x => x.Title, d.Title)
                    .Add(x => x.ConfirmText, d.ConfirmText)
                    .Add(x => x.Danger, true)
                    .Add(x => x.OnConfirm, () => { })
                    .Add(x => x.OnCancel, () => { }));
            }

            if (pickerQuery is not null)
            {
                p.Add<ExercisePicker>(x => x.Overlay, o => o
                    .Add(x => x.Query, pickerQuery)
                    .Add(x => x.Items, SampleData.PickerItems(Catalog(context), pickerQuery))
                    .Add(x => x.QueryChanged, (string _) => { })
                    .Add(x => x.OnPick, (PickerItem _) => { })
                    .Add(x => x.OnClose, () => { }));
            }
        }).FindAll("*");

    private static IReadOnlyList<IElement> History(BunitContext context, IReadOnlyList<HistoryRow> rows, bool tappable) =>
        context.Render<HistoryView>(p =>
        {
            p.Add(x => x.Rows, rows).Add(x => x.OnBack, () => { });
            if (tappable)
            {
                p.Add(x => x.OnOpen, (HistoryRow _) => { });
            }
        }).FindAll("*");
}
