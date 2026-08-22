<!--
Style:
- Write for a consumer who wants to accomplish a task, not understand
  the machinery underneath. No reflection, DI wiring, or "how it's
  resolved at runtime" — that belongs in a concept doc; link it instead.
- Every code sample must be copy-pasteable and verified against the
  actual current API before writing it down — a stale sample is worse
  than no sample, since nothing flags it as wrong.
- State decisions as rules ("use X when Y"), not as narrated internals
  ("this works because the framework does Z internally").
- Keep it short. If a section needs the depth of a concept doc, that's
  a sign it belongs in one instead.
-->

# Title

## What this is for

One or two sentences: what you can do once you know this pattern, and
why you'd reach for it.

## How to do it

A worked example — real, current code, minimal narration. If a step
requires a decision (e.g. "do I need a factory here or not"), state
the rule directly rather than explaining the mechanism that produces it.

## Common mistakes

The two or three things people actually get wrong doing this.

## Learn more

Link the concept doc(s) that explain the "why" underneath, for readers
who want to go deeper than this guide goes.
