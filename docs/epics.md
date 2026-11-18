# Epics

How a feature made of several tasks is built and reviewed. The person reviews one PR per epic. The tasks inside it are checked by CI and a verifier.

## Shape
- An **epic issue** (label `epic`) holds the goal, the design and the screenshots. Each task is a **sub-issue** of it, with its scope and acceptance criteria.
- The epic has one branch, `epic/<name>`, cut from `main`, and one **epic PR** from it into `main`. Open it as a draft so only `pr-checks` runs, and mark it ready when every sub-issue has merged, so all CI runs.
- Each task has its own branch, `task/<epic>-<id>` (not `epic/<name>/...`: git cannot hold `epic/a` and `epic/a/b` together), and a PR into the epic branch.

## A task PR
1. Rebase on the epic branch before opening and again before merging, because other tasks merge into it in the meantime.
2. Title is a Conventional Commit and the PR carries the epic PR's `semver:` label.
3. The PR description lists what changed, the evidence for each acceptance criterion, screenshots for a UI change, and an **Open questions** section.
4. A verifier that did not write the code runs each acceptance criterion on the issue and reports pass or fail for each.
5. The author squash-merges into the epic branch when the checks are green, every criterion passed and every review finding is fixed. No human review is needed.
6. On merging, copy the PR's open questions into the epic PR description under `### From <task id>`. An open question does not block the merge.

## What CI runs
| PR | Runs |
| --- | --- |
| Into `epic/**` | `pr-checks`, the dev pipeline (`dev / build-and-test`, `dev / backup-manifest`) and `story-layout`. Not the emulator E2E. |
| Epic PR into `main` | Everything that runs for any PR to `main`, once it is marked ready. |

A task that adds or changes E2E scenarios runs them locally with `.github/scripts/e2e-local.sh` and says so in its PR. Exit code 2 means they did not run; say that instead of claiming a pass.

## The epic PR
- Its description holds the summary, a checklist of the sub-issues, the screenshots and the collected open questions. The person answers them at the end.
- The person reviews it and squash-merges it. Squash is required: `next-version.sh` expects every commit on `main` to be the merge of exactly one PR.
- Epics that depend on each other go in order. Cut the next epic's branch from `main` after the previous epic's PR has merged.
