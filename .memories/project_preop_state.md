---
name: preop-implementation-state
description: Prefix/preop byte handling — loop and opCodeIdx propagation done, segment override application is next
type: project
---

Prefix byte detection is implemented: `_isPreOp[256]` lookup table, `while` loop advances `opCodeIdx` past prefix bytes, `opCodeIdx` propagated into `ParseModRMByte` so all instruction byte indexing is relative to opcode position. 42 unit tests green.

**Next step (as of 2026-03-19):** Segment override application. The prefix loop needs to capture which segment override was specified (`Register?`), then apply it to the `MemoryOperand.Segment` after `ParseModRMByte` creates the `MemoryOperand`. Override replaces the default BP→SS / else→DS assignment. For reg-to-reg (MOD=11), override is a NOP — no `MemoryOperand` exists.

**How to apply:** First red test should assert that a segment override prefix changes `MemoryOperand.Segment` to the overridden register.
