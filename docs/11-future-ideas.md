# 11 — Future Ideas

These are not in v1. Nothing here is committed.

| Idea | Notes |
|---|---|
| Show last time's numbers | While logging an exercise, show the sets from its last session |
| Per-exercise history & progress | List or chart over time, personal bests |
| Templates / repeat a workout | Start a new workout pre-filled from a routine or a past workout |
| Rest timer | Countdown between sets |
| Export (CSV/JSON) | Manual copy of the data |
| Backup / sync | See below |
| Log a past session | Enter start and finish by hand for a session that wasn't logged live |
| Page or virtualise History | If History gets slow with many workouts: load in pages, or render only the visible rows (D183) |
| Filter History by date | E.g. by month or a date range |
| Share code between New and Edit Workout | `EditWorkoutPage` repeats `NewWorkoutPage`'s picker, remove, save and back/discard code. Decide between composition and removing the duplication (US-17) |

## Backup / sync — things to consider
These matter mainly when changing phone.
- **Android Auto Backup** can back up the app's data folder to Google Drive with no backend. Check whether it suits a sideloaded app and the SQLite file.
- **Manual export/import** of a file (ties in with Export above).
- **Cloud sync** needs a backend, hosting and auth. That is a big step up for one user.
- The same signing keystore and package id are needed to install updates without losing data (see [08](08-dev-setup-build.md)).
