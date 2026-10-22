# Workout Log — Docs

A personal Android app that replaces a paper workout notebook.

**Status:** Planning. The solution has the UI and Stories projects (US-29).

## Documents

| Doc | Purpose |
|---|---|
| [01 Vision & scope](01-vision-scope.md) | What the app is for, what's in v1 and what's out |
| [02 Requirements](02-requirements.md) | Functional and non-functional requirements with acceptance criteria |
| [03 Data model](03-data-model.md) | Entities, metrics, validation rules |
| [04 Exercise list](04-exercise-list.md) | How the exercise list was built, and metrics per activity type. Full list: [exercises.json](exercises.json) |
| [05 Architecture](05-architecture.md) | Solution layout, layers, tech stack |
| [06 Screens & UX](06-screens-ux.md) | Screen inventory and navigation |
| [07 Testing](07-testing.md) | Test strategy and manual checklist |
| [08 Dev setup & build](08-dev-setup-build.md) | Tooling, running on the phone, release APK |
| [09 Roadmap](09-roadmap.md) | Milestones |
| [10 Decisions log](10-decisions-log.md) | Every decision made so far |
| [11 Future ideas](11-future-ideas.md) | Features deferred beyond v1 |
| [12 Product goal](12-product-goal.md) | Product goal and Definition of Done |
| [13 Backlog](13-backlog.md) | Epics and user stories |
| [14 Design system](14-design-system.md) | Where the design system lives (the stories site) |

## Open items

| # | Item | Where | Backlog |
|---|---|---|---|
| 1 | ~~Review the draft exercise list and metrics~~ **Done** (D79–D94) | [04](04-exercise-list.md) | [US-05](13-backlog.md#us-05-review-the-exercise-list) |
| 2 | ~~Review the draft roadmap~~ **Done** (D73) | [09](09-roadmap.md) | [US-01](13-backlog.md#us-01-review-the-draft-roadmap) |
| 3 | ~~Exact package versions~~ **Done** (D75) | [05](05-architecture.md), [07](07-testing.md) | [US-02](13-backlog.md#us-02-confirm-package-versions) |
| 4 | Where the keystore backup lives (before the first release APK) | [08](08-dev-setup-build.md) | [US-25](13-backlog.md#us-25-decide-the-keystore-backup-location) |
| 5 | ~~Rate entry format for pace units (e.g. min/km as `5.5` or `5:30`)~~ **Done** (D95) | [03](03-data-model.md) | [US-06](13-backlog.md#us-06-decide-the-pace-entry-format) |
| 6 | ~~Review the draft backlog~~ **Done** (D74) | [13](13-backlog.md) | — |
