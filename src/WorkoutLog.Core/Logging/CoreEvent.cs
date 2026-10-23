namespace WorkoutLog.Core.Logging;

/// <summary>Event ids of Core's log messages: 1000–1999, a block of 100 per type. Values never change once logged.</summary>
internal enum CoreEvent
{
    /// <summary><see cref="Data.DatabaseInitializer"/>: stored and current schema versions.</summary>
    SchemaVersion = 1001,

    /// <summary><see cref="Data.DatabaseInitializer"/>: new database created.</summary>
    DatabaseCreated = 1002,

    /// <summary><see cref="Data.DatabaseInitializer"/>: a schema upgrade step ran.</summary>
    SchemaStepApplied = 1003,

    /// <summary><see cref="Data.ExerciseSeeder"/>: exercises added and updated.</summary>
    ExercisesSeeded = 1101,

    /// <summary><see cref="Services.WorkoutService"/>: new workout saved.</summary>
    WorkoutAdded = 1201,

    /// <summary><see cref="Services.WorkoutService"/>: edited workout saved.</summary>
    WorkoutUpdated = 1202,

    /// <summary><see cref="Services.WorkoutService"/>: saving a new workout failed.</summary>
    WorkoutAddFailed = 1203,

    /// <summary><see cref="Services.WorkoutService"/>: saving an edited workout failed.</summary>
    WorkoutUpdateFailed = 1204,

    /// <summary><see cref="Services.WorkoutService"/>: the clock is before the workout start on first save.</summary>
    ClockBeforeStart = 1205,

    /// <summary><see cref="Services.WorkoutService"/>: new workout started.</summary>
    WorkoutStarted = 1206,

    /// <summary><see cref="Services.WorkoutService"/>: no workout has the id.</summary>
    WorkoutNotFound = 1207,

    /// <summary><see cref="Services.WorkoutService"/>: reading a workout failed.</summary>
    WorkoutLoadFailed = 1208,

    /// <summary><see cref="Services.WorkoutService"/>: workout read.</summary>
    WorkoutLoaded = 1209,

    /// <summary><see cref="Services.HistoryService"/>: history read.</summary>
    HistoryLoaded = 1301,

    /// <summary><see cref="Services.HistoryService"/>: reading the history failed.</summary>
    HistoryLoadFailed = 1302,
}
