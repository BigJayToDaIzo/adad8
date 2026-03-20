namespace adad8
{
    public static class Decoder
    {
        static readonly Operation[] _transOperation = new Operation[256];
        static readonly EncodingFormat[] _transFormat = new EncodingFormat[256];
        static readonly (Register?, Register?)[] _transRM =
        [
            (Register.BX, Register.SI),
            (Register.BX, Register.DI),
            (Register.BP, Register.SI),
            (Register.BP, Register.DI),
            (null, Register.SI),
            (null, Register.DI),
            (Register.BP, null),
            (Register.BX, null),
        ];
        static readonly bool[] _isPreOp = new bool[256];

        static Decoder()
        {
            // PreOpCodes
            _isPreOp[0x26] = true;
            _isPreOp[0x2E] = true;
            _isPreOp[0x36] = true;
            _isPreOp[0x3E] = true;
            _isPreOp[0xF0] = true;
            _isPreOp[0xF2] = true;
            _isPreOp[0xF3] = true;
            // add op
            for (byte i = 0x00; i <= 0x05; i++)
            {
                _transOperation[i] = Operation.Add;
                if (i <= 0x03)
                {
                    _transFormat[i] = EncodingFormat.ModRM;
                }
                else
                    _transFormat[i] = EncodingFormat.ImmediateToAccumulator;
            }
            // Next op
        }

        public static DecodedInstruction Decode(byte[] instructions)
        {
            // is this an opcode or a preop?
            var opCodeIdx = 0;
            while (_isPreOp[instructions[opCodeIdx]])
                opCodeIdx++;
            var word = (instructions[opCodeIdx] & 1) == 1;
            var direction = ((instructions[opCodeIdx] >> 1) & 1) == 1;
            var operation = _transOperation[instructions[opCodeIdx]];

            Register? reg;
            Register? rm;
            Register? source = null;
            Register? destination = null;
            ushort? immediateValue = null;
            MemoryOperand? memOp = null;

            if (_transFormat[instructions[opCodeIdx]] == EncodingFormat.ModRM) // has second byte!
            {
                (reg, rm, memOp) = ParseModRMByte(opCodeIdx, instructions, word);
                source = !direction ? reg : rm;
                destination = direction ? reg : rm;
            }
            else if (_transFormat[instructions[opCodeIdx]] == EncodingFormat.ImmediateToAccumulator)
            {
                destination = word ? Register.AX : Register.AL;
                immediateValue = word
                    ? (ushort)(instructions[opCodeIdx + 1] | (instructions[opCodeIdx + 2] << 8))
                    : instructions[opCodeIdx + 1];
            }

            if (opCodeIdx > 0 && memOp is not null)
            {
                if (instructions[0] == 0x26)
                    memOp = memOp with { Segment = Register.ES };
                else if (instructions[0] == 0x2E)
                    memOp = memOp with { Segment = Register.CS };
                else if (instructions[0] == 0x36)
                    memOp = memOp with { Segment = Register.SS };
                else if (instructions[0] == 0x3E)
                    memOp = memOp with { Segment = Register.DS };
            }

            return new DecodedInstruction
            {
                Operation = operation,
                Direction = direction,
                Word = word,
                Source = source,
                Destination = destination,
                ByteLength = instructions.Length,
                Immediate = immediateValue,
                MemoryOperand = memOp,
            };
        }

        public static (Register? reg, Register? rm, MemoryOperand? memOp) ParseModRMByte(
            int opCodeIdx,
            byte[] instructions,
            bool word
        )
        {
            var regEnumBank = word ? 8 : 0;
            var rm = instructions[opCodeIdx + 1] & 0b111;
            var reg = (instructions[opCodeIdx + 1] >> 3) & 0b111;
            var mode = (instructions[opCodeIdx + 1] >> 6) & 0b11;
            Register? segment;
            var (baseReg, indexReg) = _transRM[rm];
            // segment parsing
            if (baseReg == Register.BP)
                segment = Register.SS;
            else
                segment = Register.DS;

            if (mode == 0b11) // Register mode (no displacement)
                return ((Register)(reg + regEnumBank), (Register)(rm + regEnumBank), null);
            if (mode == 0b01)
                return (
                    (Register)(reg + regEnumBank),
                    null,
                    new MemoryOperand
                    {
                        Base = baseReg,
                        Index = indexReg,
                        Displacement = (sbyte)instructions[opCodeIdx + 2],
                        Segment = segment,
                    }
                );
            if (mode == 0b10)
                return (
                    (Register)(reg + regEnumBank),
                    null,
                    new MemoryOperand
                    {
                        Base = baseReg,
                        Index = indexReg,
                        Displacement = (short)(
                            instructions[opCodeIdx + 2] | (instructions[opCodeIdx + 3] << 8)
                        ),
                        Segment = segment,
                    }
                );
            // MOD=00 from here — 11, 01, 10 all returned early above
            if (rm == 0b110) // direct address: 16-bit displacement, no base/index
                return (
                    (Register)(reg + regEnumBank),
                    null,
                    new MemoryOperand
                    {
                        Displacement = (short)(
                            instructions[opCodeIdx + 2] | (instructions[opCodeIdx + 3] << 8)
                        ),
                        Segment = segment,
                    }
                );
            return (
                (Register)(reg + regEnumBank),
                null,
                new MemoryOperand
                {
                    Base = baseReg,
                    Index = indexReg,
                    Segment = segment,
                }
            );
        }
    }
}
