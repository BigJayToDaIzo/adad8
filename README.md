# adad8

A cycle-accurate Intel 8088 emulator in C#. Built from the ground up using strict TDD against a hardware-generated test suite.

## What Is This?

An emulator for the Intel 8088 — the CPU that powered the original IBM PC. The 8088 has a 16-bit internal architecture with an 8-bit external data bus, a 20-bit address space (1MB via segment:offset addressing), and a 4-byte prefetch instruction queue.

This project targets instruction-level accuracy first, with cycle-level accuracy as a future goal.

## Validation

The emulator is validated against [SingleStepTests/8088](https://github.com/SingleStepTests/8088), a suite of hardware-generated test cases. Each opcode has 10,000 tests (2,000 for string ops). Every test provides:

- Initial CPU state (all registers, RAM, instruction queue)
- Expected final state (changed values only)
- Per-cycle bus activity (for future cycle-accurate validation)

The test environment assumes 1MB writable RAM, no wait states, and address wrapping at 0xFFFFF.

## Project Structure

```
adad8.slnx                  # Solution file
src/adad8/
  Cpu.cs                     # CPU state, register dispatch, flag computation, execution
  Decoder.cs                 # Instruction decoder (opcode + ModR/M parsing)
  DecodedInstruction.cs      # Immutable record representing a decoded instruction
  Memory.cs                  # 1MB address space with 20-bit wrapping
  Operation.cs               # Opcode enum (Add, Sub, ...)
  Register.cs                # Register enum matching Intel's REG encoding
tests/adad8.Tests/
  CpuTests.cs                # Unit tests for register dispatch, execution, flags
  DecoderTests.cs            # Unit tests for instruction decoding
  TestHarness.cs             # Setup/verify helpers for integration tests
  TestLoader.cs              # Loads gzipped JSON test data
  TestModel.cs               # Deserialization models for SingleStepTests format
  Opcodes/
    Opcode00Tests.cs          # Integration test (10k cases for ADD)
tests/data/
  00.json.gz                  # SingleStepTests data for opcode 0x00
```

## Architecture

**Decode → Execute pipeline.** The `Decoder` produces a `DecodedInstruction` record from raw bytes. The `Cpu` executes it against its internal state. Clean separation — each layer is independently testable.

**Register enum as lookup table.** Enum values 0–7 are the 8-bit registers (AL through BH), 8–15 are the 16-bit registers (AX through DI). The ordering mirrors Intel's REG encoding table exactly, so register resolution from a ModR/M byte is just `regIndex + (word ? 8 : 0)` — no switch or dictionary needed.

**Flags register.** A 16-bit `ushort` is the source of truth. Individual flags (CF, PF, AF, ZF, SF, TF, IF, DF, OF) are computed bool properties using bit manipulation. Flag computation after arithmetic uses a wider `int` result to preserve carry information before truncation.

**Memory.** Separate class with `ReadByte`/`WriteByte` methods and 20-bit address wrapping. The seam exists for future bus cycle tracking.

## Current Status

The emulator handles ADD register-to-register instructions (byte and word width) with all six status flags (CF, PF, AF, ZF, SF, OF). Decoder supports the ModR/M byte for MOD=11 (register-to-register mode) with direction and word bit handling.

Not yet implemented: memory addressing modes, immediate operands, other opcodes, instruction fetching from memory, cycle-accurate bus activity.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [just](https://github.com/casey/just) (optional, for convenience commands)

## Build & Test

```sh
# with just
just build
just test

# or directly
dotnet build adad8.slnx
dotnet test adad8.slnx
```

## The .plan

This project keeps a [Carmack-style .plan file](.plan) at the repository root. It's a reverse-chronological work log — newest entries first — documenting what was built, key design decisions and their rationale, open questions, and resolved items. It's a log, not a todo list.

## Reference Material

- [Intel 8086 Family User's Manual](https://archive.org/details/bitsavers_intel80869lyUsersManualOct79_62967963) (October 1979) — the primary specification
- [SingleStepTests/8088](https://github.com/SingleStepTests/8088) — hardware-generated validation suite
