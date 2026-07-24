---
name: fix-bug
description: Reproduce, locate, fix, verify, and commit one OrderHub bug. Use only when the user explicitly asks to fix a bug, not for analysis-only requests.
---

# OrderHub bug-fix workflow

1. Restate the reported symptom and obtain concrete reproduction evidence: route, input, observed value, and expected value. If the user has not reproduced it, inspect the likely path and state what still needs UI or deployment verification.
2. Trace the data flow from Controller to Core service, repository, EF Core, and UI mapping. Identify the first incorrect state transition or calculation, not merely the final error.
3. Explain the root cause with exact file path, method, line number, and affected behavior. Wait for explicit user authorization before modifying code.
4. Make the smallest change that fixes only that root cause. Preserve OrderHub conventions: Controllers stay thin, business logic belongs in Core services, repositories own DbContext access, and money uses decimal.
5. Add one regression test that fails before the fix and proves the corrected behavior. Do not rewrite unrelated tests.
6. Use the `code-reviewer` agent to review the diff. Resolve relevant findings before verification.
7. Use the `test-runner` agent to run the full suite. Tell the user which UI flow to re-check manually.
8. After all verification passes, create one focused commit whose message states symptom, root cause, and fix. Do not push without a separate user confirmation.
