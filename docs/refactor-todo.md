# Decoder Refactor TODO

## Now

- [x] Pull direction logic out of `ParseModRMByte` — return (reg, r/m) operands, let `Decode` assign source/destination based on D bit
- [x] Rename `parseModRMByte` to PascalCase
- [x] Fix PascalCase local variables in `Decode` (`Direction`, `Word`, `Op`, `Source`, `Destination`) — should be camelCase
- [x] Rename `_byteOneLookAhead` to `_hasModRM`
- [ ] Fix `Decode_0x03` test — passes `[0x03]` (1 byte) but 0x03 has a ModR/M byte; will throw once registered in lookup table
- [ ] Consider `[Theory]`/`[MemberData]` for Opcode00Tests instead of monolithic loop — gives per-case discovery and parallelism

## Soon

- [ ] `DecodedInstruction` operand representation — will need immediate and memory operand support beyond `Register?`
- [ ] `Register.cs` — add namespace declaration (only file without one)
- [ ] `Memory.cs` — switch to file-scoped namespace to match rest of codebase
