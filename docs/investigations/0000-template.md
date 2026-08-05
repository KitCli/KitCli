<!--
An investigation is what a technical spike produces. It records what was
found, so the finding survives the gap between work sessions.

Lead with the verdict. A spike's deliverable is a decision, not a
report — if a reader has to reach the evidence before they know what to
do, the order is wrong. Evidence goes underneath, for the reader who
wants to check the reasoning or reuse a fact two years later.

Use the right doc type:
- Investigation — "what did we find out, and what should we do?"
- ADR           — "what did we decide, and why did the alternatives lose?"
- Concept       — "how does this work today?"

An investigation that leads to a decision doesn't become an ADR — it
justifies one. Write both and link them.

Durable facts about a dependency belong in that dependency's own
reference docs as well, not only here. Record them in both places and
say where the permanent home is.
-->

# NNNN. The question being answered

- **Status:** In Development | In Review | Complete
- **Spike:** #NN
- **Time-box:** the box agreed when the spike was estimated
- **Date:** YYYY-MM-DD

## Verdict

New complexity, or no new complexity. One paragraph, no hedging.

If no new complexity: say so plainly. The spike closes and a fresh
ticket carries the work.

If new complexity: say what it is, in the terms a breakdown needs.

## Recommendation

What should happen next, concretely enough to slice into a parent ticket
and estimable sub-tickets.

## What was established

The durable facts — the ones that stay true regardless of what gets
decided. Note where each one's permanent home is if it belongs somewhere
other than this file.

## Evidence

How each fact above was established. Real commands, real files, real
versions. Enough that a reader can re-run it rather than trust it.

## Open questions

What remains unanswered, phrased as questions. Include anything the
time-box cut short — a spike that stops at the box with questions open
is working correctly, not failing.

## Out of scope

What this deliberately didn't examine, so the next reader doesn't assume
it was checked.
