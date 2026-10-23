# 01 — Vision & Scope

## Goal
> Replace my paper notebook: log every gym session on my phone, and look back at past workouts.

See [12 Product goal](12-product-goal.md).

## User
One person, the owner. There are no accounts, no sharing and no Play Store listing. The app is sideloaded onto one phone.

## In scope (v1)
- Log a workout: add exercises from a fixed list, then record sets (strength) or one entry (cardio).
- Each exercise declares which fields (metrics) it records. For example, Bench Press records reps + weight, Plank records duration, Stair Climber records floors + steps/min.
- Start and finish times are captured automatically, with a live timer while logging.
- Workout history: list, open, edit, delete.
- Dashboard: New Workout, last workout, counts for this week and this month, link to History.
- All data stays on the phone.

## Out of scope (v1)
- Backup, export and sync (see [11 Future ideas](11-future-ideas.md)).
- Adding, editing or deleting exercises in the app. The list changes only through an app update.
- Progress charts, personal bests, templates, rest timer, "last time" numbers.
- Logging a past session. Start and finish are always captured automatically.
- iOS, Windows, other users.

## Notebook → app mapping

| Notebook field | App |
|---|---|
| Date | Kept: taken from the start time |
| Day (S M T W TH F S) | Kept: taken from the start time |
| Start | Kept: set when the New Workout page opens |
| Finish | Kept: set on first save |
| Total time | Kept: finish − start (a live timer shows while logging) |
| Type of exercise | **Dropped** |
| Exercises 1–9 | Kept: any number, picked from the fixed list |
| Sets 1–6 (reps + weight) | Kept: any number of sets; the fields depend on the exercise (reps, weight, duration) |
| Cardio row (duration, level, calories) | Kept: one entry per cardio exercise; the fields depend on the exercise (duration, distance, floors, level, calories, rate) |
| Notes | **Dropped** |
| Rate workout | **Dropped** |
| Body fat | **Dropped** |
| Weight (body) | **Dropped** |
