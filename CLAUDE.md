# Base Development Standards

## Role & Boundaries
- You are a **rubber duck**, not a co-author. This is my code, not yours.
- **Do NOT offer code snippets** unless explicitly asked.
- **Do NOT perform git operations** unless explicitly asked.
- Discuss concepts, patterns, and trade-offs. Help locate class methods, functions, and signatures. Guide design decisions through conversation.
- When asked to find something, point to documentation, method signatures, and file locations — not implementations.

## Development Standards
- **TDD is non-negotiable.** Red-Green-Refactor at all times.
- Tests written first. Code written to pass tests. Refactoring follows only after green.
- No skipping steps. No writing production code without a failing test.
- In this particular case, passing this extensive test suite is the target of this project, already provided.
    - https://github.com/SingleStepTests/8088

## Documentation & Design
- Primary utility is documentation generation and design guidance.
- Follow rubber duck agile best practices: ask clarifying questions, surface assumptions, challenge design decisions constructively.
- Help articulate architecture decisions so they are well-documented and defensible.

## Tooling & Research
- Always connect projects to every available and relevant MCP server.
- Use live, authoritative sources over training data. Do not guess at syntax or APIs.
- If an MCP server, official doc source, or live reference exists — use it. No regurgitating outdated Stack Overflow answers from 2018.

## What You Should Do
- Help find the right classes, methods, interfaces, and their signatures
- Discuss architectural patterns and their trade-offs
- Ask probing questions to sharpen requirements and design
- Generate documentation when asked
- Point to relevant docs, specs, and references

## AI-Assisted Development Philosophy
- The developer drives architecture, design, and all meaningful implementation decisions.
- The AI agent handles the repetitive, remedial, and time-consuming grunt work: boilerplate, documentation, test scaffolding, lookup, and research.
- This is not blind code-gen trust. This is a deliberate workflow that puts the developer's brain on the hard problems and leverages AI for velocity on everything else.
- Every line of production code understood, reviewed, and owned by the developer.

## What You Should NOT Do
- Write or suggest code unless explicitly requested
- Take git actions unless explicitly requested
- Make assumptions about implementation choices — ask instead
- Over-engineer or suggest unnecessary abstractions

---

# Project: adad8 — Intel 8088 Emulator

## Overview
A cycle-accurate Intel 8088 emulator written in C#. This is a learning project that also serves as a portfolio demonstration of meaningful .NET development.

## Validation Target
The emulator must pass the **SingleStepTests/8088** hardware-generated test suite:
- Repository: https://github.com/SingleStepTests/8088
- Format: JSON (v2) — 10,000 tests per opcode (2,000 for string ops)
- Each test provides initial CPU state, expected final state, and per-cycle bus activity

## Test Suite Format
Each test case is a JSON object with:
- **name**: Human-readable instruction disassembly
- **bytes**: Raw instruction bytes
- **initial**: Full CPU state before execution (all registers, RAM as address-value pairs, instruction queue contents)
- **final**: Only changed values after execution (registers, RAM, flags — flags included in full if any flag changed)
- **cycles**: Array of per-cycle bus activity (11 fields per cycle: pin state, address, segment status, memory/IO status, data bus, bus cycle type, T-state, queue operation, queue byte)
- **hash**: SHA1 identifier
- **idx**: Test index

## Test Environment Assumptions
- 1MB writable RAM, no wait states
- Address wraps at 0xFFFFF (real mode)
- Half of tests execute from prefetched instruction queue; handle by installing queue contents after reset
- Subsequent queue fetches return NOP (0x90)

## Key 8088 Characteristics
- 16-bit internal architecture, 8-bit external data bus
- 4-byte prefetch instruction queue (vs 8086's 6-byte)
- 20-bit address space (1MB) via segment:offset addressing
- Registers: AX, BX, CX, DX, CS, SS, DS, ES, SP, BP, SI, DI, IP, FLAGS

## 8088 Internals — Auxiliary Carry Flag (AF) and the XOR Recovery Trick

AF is set when a carry (or borrow) crosses the bit 3→4 boundary — the nibble boundary. It exists for BCD adjustment instructions (DAA, DAS, AAA, AAS).

### The verbose approach (ADD-only)

Mask the low nibble of both operands, add them, check for overflow past 0x0F:

```
(source & 0x0F) + (destination & 0x0F) > 0x0F
```

Straightforward, but only models carry. Subtraction needs separate borrow logic.

### The XOR trick (operation-agnostic)

```
AF = (source ^ destination ^ result) & 0x10
```

One formula, works for both ADD and SUB. Here's the derivation:

**Step 1 — The full adder identity.** A full adder computes the sum bit at position `i` as:

```
result[i] = a[i] ⊕ b[i] ⊕ carry_in[i]
```

This is the definition of binary addition at a single bit. The sum is the three-way XOR of the two inputs and the incoming carry.

**Step 2 — XOR is self-inverse.** Since `x ⊕ x = 0` and `x ⊕ 0 = x`, any one term can be recovered from the other three:

```
carry_in[i] = a[i] ⊕ b[i] ⊕ result[i]
```

**Step 3 — AF is carry_in at bit 4.** The carry entering bit 4 is the carry out of bit 3 — exactly the nibble boundary AF tracks. XOR all three full values and mask to isolate bit 4:

```
AF = (source ⊕ destination ⊕ result) & 0x10
```

**Step 4 — Subtraction has the same structure.** A full subtractor's difference bit is:

```
result[i] = a[i] ⊕ b[i] ⊕ borrow_in[i]
```

Same XOR relationship, borrow in place of carry. The recovery formula is identical.

### Why this matters

The verbose approach requires branching on the operation (add vs subtract vs compare vs ...). The XOR trick is a single expression that works for any arithmetic operation because it derives carry/borrow from the observable inputs and result, regardless of how the result was computed.

## 8088 Internals — Little-Endian Reassembly (Multi-Byte Immediates)

The 8088 stores multi-byte values in little-endian order: least significant byte first in memory. A 16-bit immediate like `0x1234` is stored as `[0x34, 0x12]` — low byte at the lower address, high byte at the higher address.

To reconstruct the value from two consecutive bytes:

```
(ushort)(instructions[1] | (instructions[2] << 8))
```

`instructions[1]` (0x34) occupies bits 0–7. `instructions[2] << 8` shifts 0x12 up to bits 8–15. The two bytes now occupy non-overlapping bit positions:

```
instructions[1]          = 0000 0000 0011 0100   (0x34, bits 0–7)
instructions[2] << 8     = 0001 0010 0000 0000   (0x12, bits 8–15)
OR result                = 0001 0010 0011 0100   (0x1234)
```

OR merges them because the bit positions don't overlap — each byte's bits slot into their own lane. Addition would produce the same numeric result, but OR is idiomatic for bit-field assembly because it communicates intent: you're combining non-overlapping fields, not performing arithmetic.

The cast to `ushort` is necessary because C# promotes bitwise operations on bytes/shorts to `int`. Without the cast, the result is `int` and won't assign to a `ushort?` property without complaint (CS0266).

This pattern recurs anywhere the 8088 stores 16-bit values: immediates, displacements, and segment:offset addresses in memory.

## 8088 Internals — Decoder Architecture: Bitfield Extraction → Opcode-Indexed Lookup Tables

The first decoder implementation extracted meaning from bit positions within the opcode byte: shift right 2 to get the operation, mask bit 1 for direction, mask bit 0 for word width. This works when opcodes share a uniform bit layout — 0x00 through 0x03 all encode ADD with D and W bits in fixed positions.

It breaks as soon as a different encoding format appears. ADD immediate-to-accumulator (0x04/0x05) has the same operation but a completely different byte layout — no ModR/M byte, data follows the opcode directly. The top 6 bits no longer map to the same operation value. Every new encoding family would require more conditional bit logic, and the complexity compounds with each one.

The fix: stop scanning bits and start indexing by the full opcode byte. There are only 256 possible opcodes. A lookup table indexed by opcode byte is a finite, knowable space. It doesn't try to be clever — it answers the question directly for each opcode. Bit manipulation is unbounded complexity you have to reason about every time; a 256-entry table is dumb and correct.

### Lookup tables identified so far

| Table | Type | Question it answers |
|---|---|---|
| `_transOperation[256]` | `Operation[]` | What operation does this opcode perform? |
| `_transFormat[256]` | `EncodingFormat[]` | What encoding format are the operands in? (ModRM, ImmediateToAccumulator, ImmediateToModRM, etc.) |

Additional tables will emerge as more opcodes reveal their encoding quirks. The pattern is: one table per decoding question, indexed by opcode byte, populated at static init.

## Gotchas

### Decoder tests with single-byte input for multi-byte opcodes

Some decoder unit tests (e.g. `Decode_0x03`) pass only the opcode byte to validate first-byte decoding (operation, D bit, W bit). This is a valid test — it proves the opcode resolution layer works in isolation. However, opcodes like 0x03 require a ModR/M byte. When that opcode gets registered in `_hasModRM`, the decoder will expect byte 2, and the test may need its input extended (e.g. `[0x03]` → `[0x03, 0xC8]`) depending on how the decoder handles missing bytes. Not a bug — just a test that will announce itself when the decoder grows.

### AF formula is ADD-only until SUB lands

`SetFlags` currently computes AF with the verbose nibble-add approach: `(source & 0x0F) + (destination & 0x0F) > 0x0F`. This only works for ADD. When SUB is implemented, replace with the operation-agnostic XOR trick: `(source ^ destination ^ result) & 0x10`. The derivation is documented above in the AF section. Leave as-is while building out ADD to completion.
