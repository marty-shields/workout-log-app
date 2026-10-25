# 09 — Product Goal & Definition of Done

## Product goal
> Replace my paper notebook: log every gym session on my phone, and look back at past workouts.

### Done when
- [ ] The signed release APK is installed on my phone.
- [ ] Every gym session for 4 weeks is logged in the app, without the notebook.
- [ ] All requirements in [01](01-requirements.md) are met.

## Definition of Done
A story in the [backlog](https://github.com/users/marty-shields/projects/2) is Done when all of these are true:

| Area | Criteria |
|---|---|
| Code | Builds with no errors or warnings (warnings are errors in `Directory.Build.props`). No new packages without approval. |
| Tests | New behaviour has unit or bUnit tests. All tests pass. |
| Logging | Errors and unexpected events in new code are logged as in [04](04-architecture.md#logging), with tests. No workout contents in logs. |
| Verified on phone | Where the story changes the app, its acceptance criteria have been checked by hand on the real phone. |
| Docs + PR | Affected docs are updated and `README.md` still matches the app (D222). The reviewer approved the change locally before the commit, and it reached `main` through a PR as in [07](07-dev-setup-build.md#source-control). |
