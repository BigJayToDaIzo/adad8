---
name: tdd-smallest-step
description: Always pick the smallest possible red test — user will challenge if you skip ahead to downstream effects
type: feedback
---

When writing red tests, always target the smallest observable unit. Don't jump to testing downstream effects (e.g. register values) when you could test the immediate output of the unit under change.

**Why:** User challenged multiple test suggestions this session for testing too far downstream. The right test targets the thing you're building, not its side effects three layers down.

**How to apply:** Before writing a test, ask: "what is the smallest thing this change should produce that I can observe?" If it's internal state that isn't exposed, then the smallest downstream effect is acceptable — but justify why you can't test closer to the source.
