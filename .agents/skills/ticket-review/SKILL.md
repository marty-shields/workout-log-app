---
name: ticket-review
description: Runs three independent reviews (requirements, code and tests, security) of the current branch against a GitHub issue, one after another, each with no context but the issue. Collates the findings, fixes the dead certs and asks the user about the rest. Use only when the user explicitly asks for a ticket review or a code review of a story; never start it on your own.
disable-model-invocation: true
---

# Ticket review

Run only when the user asks for a ticket review or a code review of a story. Never start it yourself.

## Inputs
- **Issue:** a GitHub issue link or number. Required.
- **Extra notes or ACs:** optional, from the user, added to the issue text.

Fetch the issue with `gh issue view <n> --repo marty-shields/workout-log-app --json title,body,url` ([08](../../../docs/08-dev-setup-build.md#source-control) has the `gh` sign-in). If it can't be fetched, stop and say so.

## What is reviewed
The current branch against `main`: `git diff main` (branch commits and uncommitted changes) plus untracked files from `git status --short`.

## Steps
1. **Fetch the issue** and note the repo path and branch.
2. **Run the three reviews in this order, one at a time:** requirements, code and tests, security. They share the working tree, so never run them in parallel.
   - Run each in a fresh-context sub-agent: a new agent with no conversation history, not a fork.
   - If the agent can't start one, run its own CLI headless in a new process with only the reviewer prompt (e.g. `claude -p`, `codex exec`, `copilot -p`).
   - If neither is possible, stop and tell the user. Never review inline with this session's context.
   - Give each reviewer only its filled-in prompt from [Reviewer prompt](#reviewer-prompt). Nothing about the implementation, earlier discussion or the other reviews.
3. **Check each finding against the code yourself.** Reviewers can be wrong. Note what you found.
4. **Classify each finding:**
   - **Dead cert:** all of these hold:
     - its evidence (file:line) checks out, and it has one obvious fix;
     - the fix changes no behaviour, public API or story scope;
     - no other reviewer contradicts it or suggests a different fix.
   - **Not acting on:** the reviewer suggests no change (info only). Give the reason.
   - **Needs your call:** everything else, including findings your check refuted (say why).
5. **Show the collated table** ([Collated table](#collated-table)).
6. **Fix the dead certs.**
7. **Ask about the rest in question rounds.**
   - Use the agent's structured question tool if it has one: multi-select, up to 4 questions a round, related findings grouped into one question. A finding whose fix needs a design choice gets its own question with the options.
   - Otherwise, give a numbered list and ask the user to reply with the IDs to fix.
   - Repeat until every finding has an answer. Never assume a finding isn't worth fixing.
8. **Fix the confirmed findings.** AGENTS.md rules still apply: ask about any ambiguity, and get approval for new packages.
9. **Verify and stop.** Run the build and the relevant tests from AGENTS.md and report the results. Then stop for the user's local review; don't commit.

## Reviewer prompt
Fill in the `{…}` parts, keep the shared part, and add the reviewer's focus.

```
You are reviewing a change in the git repo at {repo path} (branch `{branch}`). The change is everything in
`git diff main` plus the untracked files from `git status --short`.

It implements this GitHub issue ({issue url}):
---
{issue title}
{issue body}
{extra notes or ACs from the user, if any}
---

YOUR FOCUS: {focus}

Read AGENTS.md and the docs it links before reviewing.

Rules:
- Read-only. Don't edit, create or delete files, commit, install packages or write to GitHub.
- You may run `dotnet build WorkoutLog.slnx`, `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`. Never run the E2E tests.
- Ignore bin/, obj/, .vs/ and TestResults/.
- Report only findings you can back with file:line evidence. Say what you checked and found nothing in.

Reply with one fenced JSON block and nothing else:
{
  "reviewer": "requirements | code | security",
  "verification": ["each command you ran and its result"],
  "ac_table": [{"ac": "...", "verdict": "met | partly met | not met", "evidence": "file:line"}],
  "findings": [{
    "id": "REQ-1 | CODE-1 | SEC-1 …",
    "severity": "blocking | should-fix | nit | info",
    "category": "short kebab-case, e.g. correctness, test-quality, scope-gap, data-access",
    "file": "repo-relative path",
    "line": 1,
    "problem": "one sentence",
    "evidence": "what in the code shows it",
    "fix": "concrete suggested fix, or null for info",
    "introduced_by_change": true
  }],
  "checked_clean": ["areas checked with nothing found"]
}
`ac_table` is for the requirements reviewer only; the others return [].
```

### Focus: requirements
Does the change resolve the issue fully, without adding or removing scope? Give each AC, and the story statement, a verdict in `ac_table` with evidence. Check the Definition of Done in docs/11-product-goal.md and the AGENTS.md rules: decisions log, docs kept in step, logging. Report as findings anything beyond the issue's scope (`scope-addition`) and anything in scope that is missing (`scope-gap`). Search src/ and tests/, including the App, UI, E2E and UI test projects, for places that still depend on what changed.

### Focus: code and tests
A general code and test review. Check:
- correctness, readability, and consistency with the surrounding code and repo conventions (docs/05-architecture.md, docs/07-testing.md);
- test quality: are the tests meaningful, could they pass for the wrong reason, are they flaky on timing, are cases missing, do the names fit;
- comment and doc accuracy;
- anything dead or redundant.

### Focus: security
A full security review. The app is a personal, offline, single-user Android app (.NET MAUI Blazor Hybrid, SQLite through sqlite-net). Check the change and the code paths it touches for:
- ways to reach data or the database that skip the intended path (DI registrations, `IEnumerable<T>` resolution, visibility, internals access);
- concurrency and data-integrity risks: deadlocks, re-entrancy, misuse of locks or semaphores, disposal;
- DI lifetime problems;
- exception handling that leaks data or leaves inconsistent state;
- workout contents or other sensitive data in logs;
- SQL injection;
- committed secrets or environment-specific values;
- new dependencies.

Set `introduced_by_change` to false for problems that existed before this change.

## Collated table
One table of every finding, grouped by status: **Dead cert (fixing)**, **Needs your call**, **Not acting on**. Merge findings that several reviewers raised into one row and list all their IDs.

| Status | IDs | Reviewers | Severity | Where | Summary | Check |
|---|---|---|---|---|---|---|
| Dead cert | CODE-1 | code | nit | Tests.cs:63 | Test passes for the wrong reason | Confirmed |

Below the table, list each reviewer's verification runs and the requirements AC table.
