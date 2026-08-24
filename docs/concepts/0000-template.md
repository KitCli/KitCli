<!--
Style, from real review feedback on earlier concept docs:
- Omit needless words. Every word should tell. Cut "actually", "just",
  "simply", "entirely", "in practice", and any qualifier that adds no
  fact — they weaken a sentence and never strengthen it.
- Prefer the active voice, and put the emphatic word last.
- One idea per paragraph. A paragraph doing two things splits into two.
- Prefer a list or table over a dense sentence chaining branches with
  arrows or semicolons.
- Keep to 2-3 inline-code names per sentence. Pick the ones that matter
  and drop or defer the rest.
- Define or avoid jargon on first use. Where a reader would ask "what?",
  rephrase rather than assume the term lands.
- In a Q&A entry, check the answer resolves the literal question asked,
  not adjacent context.
- Mentioning a bug or gap, say plainly whether an issue tracks it, or
  whether nothing does. Never leave it reading as narrative color.
-->

# Title

## Premise

What this subsystem is and why it exists — the context a reader needs
before "Problem" makes sense.

## Problem

The specific challenge this part of KitCli solves. Concrete, not abstract.

## Solution

Progressive disclosure of how it works: real class and method names, real
signatures, worked examples using code in the repo today. Verify every
name and signature against source before writing it down, and never
describe intended behavior as current.

## Constraints & tradeoffs

Design decisions and the alternatives that lost, in brief — the one place
a concept doc overlaps an ADR. Where a decision has, or deserves, its own
ADR, link it rather than re-explaining it.

## Questions & answers

The "how do I..." and "why does it..." questions a consumer would ask,
answered directly.

## Related concepts

A concept doc covers **one** concept. Don't let a subsystem's neighbors
creep in because they're used together. List the other concept docs this
one connects to, a line each: which doc, and why a reader might need it.
Linking out rather than re-explaining is what keeps single-topic docs
sustainable instead of moving the sprawl around.
