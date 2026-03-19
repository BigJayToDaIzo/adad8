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

        static Decoder()
        {
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
            bool isOpCode = false;
            MemoryOperand? memOp = null;
            while (!isOpCode)
            {
                // we have to set MemoryOperand.Segment here
                // we don't have a MemoryOperand yet.
                if (instructions[opCodeIdx] == 0x26)
                {
                    memOp = new { Segment = Register.ES };
                    opCodeIdx++;
                }
                else if (instructions[opCodeIdx] == 0x2E)
                {
                    opCodeIdx++;
                }
                else if (instructions[opCodeIdx] == 0x36)
                {
                    opCodeIdx++;
                }
                else if (instructions[opCodeIdx] == 0x3E)
                {
                    opCodeIdx++;
                }
                isOpCode = true;
            }
            var word = (instructions[opCodeIdx] & 1) == 1;
            var direction = ((instructions[opCodeIdx] >> 1) & 1) == 1;
            var operation = _transOperation[instructions[opCodeIdx]];

            Register? reg;
            Register? rm;
            Register? source = null;
            Register? destination = null;
            ushort? immediateValue = null;

            if (_transFormat[instructions[0]] == EncodingFormat.ModRM) // has second byte!
            {
                (reg, rm, memOp) = ParseModRMByte(instructions, word);
                source = !direction ? reg : rm;
                destination = direction ? reg : rm;
            }
            else if (_transFormat[instructions[0]] == EncodingFormat.ImmediateToAccumulator)
            {
                destination = word ? Register.AX : Register.AL;
                immediateValue = word
                    ? (ushort)(instructions[1] | (instructions[2] << 8))
                    : instructions[1];
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
            byte[] instructions,
            bool word
        )
        {
            var regEnumBank = word ? 8 : 0;
            var rm = instructions[1] & 0b111;
            var reg = (instructions[1] >> 3) & 0b111;
            var mode = (instructions[1] >> 6) & 0b11;
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
                        Displacement = (sbyte)instructions[2],
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
                        Displacement = (short)(instructions[2] | (instructions[3] << 8)),
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
                        Displacement = (short)(instructions[2] | (instructions[3] << 8)),
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
