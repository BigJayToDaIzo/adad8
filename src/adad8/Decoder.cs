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
            // first byte start
            var word = (instructions[0] & 1) == 1;
            var direction = ((instructions[0] >> 1) & 1) == 1;
            var operation = _transOperation[instructions[0]];

            Register? reg;
            Register? rm;
            MemoryOperand? memOp = null;
            Register? source = null;
            Register? destination = null;
            ushort? ImmediateValue = null;

            if (_transFormat[instructions[0]] == EncodingFormat.ModRM) // has second byte!
            {
                (reg, rm, memOp) = ParseModRMByte(instructions[1], word);
                source = !direction ? reg : rm;
                destination = direction ? reg : rm;
            }
            else if (_transFormat[instructions[0]] == EncodingFormat.ImmediateToAccumulator)
            {
                destination = word ? Register.AX : Register.AL;
                ImmediateValue = word
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
                Immediate = ImmediateValue,
                MemoryOperand = memOp,
            };
        }

        public static (Register? reg, Register? rm, MemoryOperand? memOp) ParseModRMByte(
            byte instruction,
            bool word
        )
        {
            var regEnumBank = word ? 8 : 0;
            var rm = instruction & 0b111;
            var reg = (instruction >> 3) & 0b111;
            var mode = (instruction >> 6) & 0b11;
            if (mode == 0b11) // Register mode (no displacement)
                return ((Register)(reg + regEnumBank), (Register)(rm + regEnumBank), null);
            else
            {
                // mod 0b00,0b01, and 0b10 only differ in offset of memory
                // which is handled by the word ? checks throughout
                var (baseReg, indexReg) = _transRM[rm];
                // this only works for mode 0x00, the other 2 will need a Displacement
                // field in the MemoryOperand
                return (
                    (Register)(reg + regEnumBank),
                    null,
                    new MemoryOperand { Base = baseReg, Index = indexReg }
                );
            }
        }
    }
}
