# Report format

Every factory agent ends with one report as its final message. No files. Keep it under 40 lines. Never paste whole files or diffs; use `path:line` references.

```
STATUS: pass | fixed | blocked
CHANGED: <files touched, or "none">
FINDINGS:
- [high|med|low] path:line - what is wrong - suggested fix
EVIDENCE: <tests run and result, screenshots taken and what they show, commands and exit codes>
QUESTIONS: <ambiguities you resolved by assuming, and what you assumed; open points the user should answer. "none" if none>
```

- `pass`: nothing to fix. `fixed`: you changed code to resolve findings. `blocked`: you could not finish; say why under FINDINGS.
- Reviewers report only real problems. No praise, no restating the diff, no style nits that the repo's formatter or CI already enforces.
- Put an issue under QUESTIONS only when a person must decide. If you can decide, decide, state the assumption, and carry on.
