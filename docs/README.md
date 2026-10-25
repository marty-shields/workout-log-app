# Workout Log — Docs

A personal Android app that replaces a paper workout notebook.

## Documents

| Doc | Purpose |
|---|---|
| [01 Vision & scope](01-vision-scope.md) | What the app is for, what's in v1 and what's out |
| [02 Requirements](02-requirements.md) | Functional and non-functional requirements with acceptance criteria |
| [03 Data model](03-data-model.md) | Entities, metrics, validation rules |
| [04 Exercise list](04-exercise-list.md) | How the exercise list was built, and metrics per activity type. Full list: [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| [05 Architecture](05-architecture.md) | Solution layout, layers, tech stack |
| [06 Screens & UX](06-screens-ux.md) | Screen inventory and navigation |
| [07 Testing](07-testing.md) | Test strategy and manual checklist |
| [08 Dev setup & build](08-dev-setup-build.md) | Tooling, running on the phone, release APK, source control, CI |
| [09 Decisions log](09-decisions-log.md) | Every decision made so far |
| [10 Future ideas](10-future-ideas.md) | Features deferred beyond v1 |
| [11 Product goal](11-product-goal.md) | Product goal and Definition of Done |
| [12 Design system](12-design-system.md) | Where the design system lives (the stories site) |

## Ways of working
- **Backlog:** the [Workout Log project](https://github.com/users/marty-shields/projects/2) over the repo's [issues](https://github.com/marty-shields/workout-log-app/issues) (D216).
- **Milestones:** M0…M6 are [GitHub milestones](https://github.com/marty-shields/workout-log-app/milestones); each holds its scope and exit criteria. A milestone is closed when all its epics are Done.
- **Epics:** issues titled `EP-xx Name`, in their milestone. An epic's stories are its sub-issues.
- **Stories:** issues titled `US-xx Title`, in their epic's milestone. IDs never change; a new story takes the next number. "As the user, I want…, so that…", or "As the developer…" for technical stories.
- **Acceptance criteria:** the linked FR-xx in [02](02-requirements.md), plus any extra ACs in the issue.
- **Size:** the project's Size field: XS, S, M, L, XL. Relative effort only.
- **Status:** the project's Status field: To do, In progress, Blocked, Done.
- **Issue numbers:** EP-01…EP-09 are #1…#9 and US-01…US-38 are #10…#47. Later stories and PRs take the next free number.
- **Delivery:** one branch per story, `type/us-xx-slug`. After local review, the commit is pushed and a DRAFT PR opened that closes the story's issue. The PR build must pass; the reviewer marks the PR ready and squash-merges it, which closes the issue ([08](08-dev-setup-build.md#source-control), [11](11-product-goal.md)).
