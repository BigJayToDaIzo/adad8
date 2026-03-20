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

## 8088 Internals — Sign Extension via Signed Cast

When the 8088 needs an 8-bit value sign-extended to 16 bits (MOD=01 displacements, relative jumps, 0x83 group sign-extended immediates), C# handles two's complement through the type system:

```
(sbyte)byteValue
```

`(sbyte)` reinterprets the unsigned byte as signed (-128 to 127). Implicit widening to `short` preserves the sign. `0xFC` → `(sbyte)` → -4 → assigns to `short` → -4. No manual bit flipping needed.

## 8088 Internals — Decoder Architecture: Bitfield Extraction → Opcode-Indexed Lookup Tables

The first decoder implementation extracted meaning from bit positions within the opcode byte: shift right 2 to get the operation, mask bit 1 for direction, mask bit 0 for word width. This works when opcodes share a uniform bit layout — 0x00 through 0x03 all encode ADD with D and W bits in fixed positions.

It breaks as soon as a different encoding format appears. ADD immediate-to-accumulator (0x04/0x05) has the same operation but a completely different byte layout — no ModR/M byte, data follows the opcode directly. The top 6 bits no longer map to the same operation value. Every new encoding family would require more conditional bit logic, and the complexity compounds with each one.

The fix: stop scanning bits and start indexing by the full opcode byte. There are only 256 possible opcodes. A lookup table indexed by opcode byte is a finite, knowable space. It doesn't try to be clever — it answers the question directly for each opcode. Bit manipulation is unbounded complexity you have to reason about every time; a 256-entry table is dumb and correct.

### Lookup tables identified so far

| Table | Type | Question it answers |
|---|---|---|
| `_transOperation[256]` | `Operation[]` | What operation does this opcode perform? |
| `_transFormat[256]` | `EncodingFormat[]` | What encoding format are the operands in? (ModRM, ImmediateToAccumulator, ImmediateToModRM, etc.) |
| `_transRM[8]` | `(Register?, Register?)[]` | What base/index register pair does this R/M field encode? (for memory addressing modes) |
| `_isPreOp[256]` | `bool[]` | Is this byte a prefix (true) or an opcode (false)? 7 entries true: `0x26`, `0x2E`, `0x36`, `0x3E`, `0xF0`, `0xF2`, `0xF3` |

Additional tables will emerge as more opcodes reveal their encoding quirks. The pattern is: one table per decoding question, indexed by opcode byte, populated at static init.

## Gotchas

### AF formula is ADD-only until SUB lands

`SetFlags` currently computes AF with the verbose nibble-add approach: `(source & 0x0F) + (destination & 0x0F) > 0x0F`. This only works for ADD. When SUB is implemented, replace with the operation-agnostic XOR trick: `(source ^ destination ^ result) & 0x10`. The derivation is documented above in the AF section. Leave as-is while building out ADD to completion.

## 8088 Internals — Execute Source Resolution: Three-Way Operand Dispatch

`Execute` needs to resolve a source operand value before it can do anything. There are exactly three places a source value can come from, and for any given instruction exactly one is populated:

| Source type | DecodedInstruction field | How to get the value |
|---|---|---|
| Immediate | `Immediate` (non-null) | Use it directly (it's already a `ushort`) |
| Memory | `MemoryOperand` (non-null), `Source` is null | `ResolveEffectiveAddress` → `Mem.ReadByte` (or two-byte read for word) |
| Register | `Source` (non-null) | `GetRegisterValue(Source)` |

This was extracted into `Cpu.DecodeSource(DecodedInstruction)` which returns a `ushort` — the resolved value regardless of where it came from. `Execute` never has to think about operand sourcing; it just gets a number.

### Address vs Value — the indirection trap

`ResolveEffectiveAddress` returns *where* the operand lives. `Mem.ReadByte` returns *what's there*. Forgetting to dereference (using the address as the value) is an easy mistake — the test catches it because the address (e.g. `0x0200`) won't match the expected result computed from the stored value (e.g. `0x05`).

## 8088 Internals — Segment:Offset Physical Address Calculation

The 8088 computes 20-bit physical addresses from a 16-bit segment and a 16-bit offset:

```
physical_address = ((segment << 4) + offset) & 0xFFFFF
```

Two wrapping boundaries matter:

1. **Offset wraps at 16 bits.** The effective address (base + index + displacement) is computed as a 16-bit value. If the sum exceeds 0xFFFF, it wraps. This must happen *before* the segment is added.

2. **Physical address wraps at 20 bits.** After adding `segment << 4`, the result is masked to 20 bits (`& 0xFFFFF`). This is the real-mode 1MB address space boundary.

### Segment selection rules

Every memory access uses a segment register. The default depends on the base register:

| Base register | Default segment | Why |
|---|---|---|
| BP | SS | BP is the stack frame pointer — stack-relative access |
| Everything else (BX, SI, DI, none) | DS | Data segment is the default |

The decoder determines the segment at decode time and stores it on `MemoryOperand.Segment`. `ResolveEffectiveAddress` reads it and computes the full physical address.

### Implementation note

`ResolveEffectiveAddress` must compute the offset first (base + index + displacement), wrap it to 16 bits, *then* add `segment << 4`, and wrap the final result to 20 bits. Getting the order wrong produces addresses that are off by 0x10000 — which reads/writes the wrong memory location entirely.

### How a wrong address silently corrupts flags

A bad EA doesn't crash — it reads a valid but *wrong* byte from memory. The failure chain:

1. **Wrong offset wrapping** → physical address lands in wrong memory region
2. **Wrong `destVal`** → `Mem.ReadByte(wrongAddr)` returns whatever byte lives there
3. **Wrong arithmetic result** → `destVal + srcVal` computes a plausible but incorrect sum
4. **Wrong flags** → `SetFlags` faithfully computes flags for the wrong result

The result and flags are internally consistent — they're just not consistent with what the hardware would produce. This makes the bug invisible to any check that doesn't compare against known-good state.

In integration testing, this typically surfaces as a **Flags mismatch** (not a RAM mismatch), because `Verify` checks registers and flags before RAM. For memory-destination instructions where no GPR changes, flags is the first computed value in the assert chain, so it's the first to fail. The symptom is a small delta in the Flags register (e.g. 4 = bit 2 = PF) with no obvious connection to addressing — you have to trace backwards from "wrong parity" through "wrong result" through "wrong read" to "wrong address."

### Two traps when refactoring `ResolveEffectiveAddress`

1. **Double shift.** If the segment is shifted `<< 4` when read from the register *and* shifted again when added to the offset, the segment contribution is `<< 8` — 16x too large. The symptom: addresses like `0x20010` instead of `0x2010`. Pick one site for the shift.

2. **Wrong mask width.** `0xFFFFF` (five F's) = 20 bits. `0xFFFFFF` (six F's) = 24 bits. Easy to miscount. `Memory.ReadByte` masks at 20 bits as a safety net, but `ResolveEffectiveAddress` should be correct on its own.

## 8088 Internals — Instruction Prefix Bytes

The 8088 has single-byte prefixes that modify the following instruction. The decoder must consume prefix bytes before extracting W/D/operation from the actual opcode. Prefixes are not opcodes — they have no W bit, no D bit, no ModR/M byte. They modify state and then the real opcode follows.

### Segment override prefixes

These override the default segment register for the instruction's memory access:

| Byte | Prefix | Effect |
|---|---|---|
| `0x26` | ES: | Memory access uses ES instead of default |
| `0x2E` | CS: | Memory access uses CS instead of default |
| `0x36` | SS: | Memory access uses SS instead of default |
| `0x3E` | DS: | Memory access uses DS instead of default |

On a register-to-register instruction (MOD=11), a segment override is a NOP — there's no memory access to redirect. But the prefix byte is still consumed, IP still advances by 1, and the test suite expects this.

### What happens without prefix handling

The decoder treats the prefix byte as the opcode. For `CS: ADD CL, BL` (bytes `[0x2E, 0x00, 0xD9]`):

1. Decoder reads `0x2E` as opcode → extracts W=0, D=1 from its bits
2. Reads `0x00` as ModR/M → MOD=00, REG=000, R/M=000 → memory operand [BX+SI]
3. Produces `ADD AL, [BX+SI]` instead of `ADD CL, BL`
4. Execute writes to AL, never touches CL
5. CX remains at its initial value — test fails on CX assertion

The symptom is a register holding its initial value when it should have been modified. The decoder silently produced a valid but wrong instruction — no crash, no exception, just wrong behavior.

### Prefix handling strategy

Prefixes must be stripped before the main decode loop. The decoder needs to:

1. Check if `instructions[offset]` is a prefix byte
2. If so, record its effect (e.g., segment override) and advance `offset`
3. Repeat until a non-prefix byte is found — that's the real opcode
4. Decode the opcode starting at `offset`, using the accumulated prefix state
5. `ByteLength` must include all prefix bytes consumed

The segment override, once captured, replaces the default segment that `ParseModRMByte` would assign. For memory operands, the prefix segment takes precedence over the BP→SS / else→DS default. For register operands, the override is ignored.

### Other prefixes (future)

Beyond segment overrides, the 8088 has:
- `0xF0` — LOCK (bus lock for multi-processor, can ignore for now)
- `0xF2` — REPNE/REPNZ (string operation repeat)
- `0xF3` — REP/REPE/REPZ (string operation repeat)

Multiple prefixes can stack. The decoder loop should handle any number of prefix bytes before the opcode.

## Current Development State

### What works (46 unit tests green, integration test partially passing)
- ADD register-to-register: byte and word, all 6 status flags, all edge cases
- ADD immediate-to-accumulator: byte (AL) and word (AX)
- ADD memory as source: byte and word (little-endian two-byte read)
- ADD memory as destination: byte and word (little-endian two-byte write)
- MOD=00 + R/M=110 direct address special case
- IP advancement by instruction byte length
- Effective address resolution with two-phase wrapping (offset at 16 bits, physical at 20 bits)
- Segment register support: CS, DS, SS, ES added to Register enum
- Decoder: segment assignment on MemoryOperand (BP→SS, else→DS)
- Decoder: opcode lookup tables, ModR/M parsing for all MOD modes, ImmediateToAccumulator format
- Decoder: prefix byte detection via `_isPreOp[256]` lookup table — loop advances `opCodeIdx` past prefix bytes to find real opcode
- Decoder: `opCodeIdx` propagated through `Decode` and into `ParseModRMByte` — all instruction byte indexing is now relative to opcode position
- Decoder: segment override prefixes (0x26→ES, 0x2E→CS, 0x36→SS, 0x3E→DS) applied to MemoryOperand via `record with` after ParseModRMByte returns; null-guarded for MOD=11 (register-to-register, no memOp)
- Opcode00 integration test running — passes prefixed instructions, fails on `add byte [ds:7D0Eh], cl`

### Current integration test failure
- Test: `add byte [ds:7D0Eh], cl` — non-prefixed, MOD=00 R/M=110 direct address
- Symptom: Flags mismatch — expected 0xF096, actual 0xF006, delta = 0x0090 (AF + SF)
- Root cause unknown — could be wrong EA (wrong-address → wrong-read → wrong-flags chain) or a real SetFlags bug. Debugger breakpoint is set; next step is stepping through ResolveEffectiveAddress to verify the physical address and read value.

### What's next
1. Debug the `add byte [ds:7D0Eh], cl` failure — step through ResolveEffectiveAddress, verify address/read/arithmetic/flags chain
2. Segment override if/else chain in Decode (lines 77-84) is a candidate for a lookup table refactor after green
3. Continue running Opcode00 integration test suite (10,000 tests) to find remaining edge cases
4. Duplicate memory read pattern in DecodeSource and Execute — candidate for ReadWord helper on Memory class
5. Operation enum default value — `Add` is `0`, so uninitialized `_transOperation` slots silently return `Add`. Consider adding a `None` sentinel as the first enum value.
