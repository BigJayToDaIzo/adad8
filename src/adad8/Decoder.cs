namespace adad8
{
  public static class Decoder
  {
    static readonly Operation[] _transOperation = new Operation[256];
    static readonly EncodingFormat[] _transFormat = new EncodingFormat[256];

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
      var direction = ((instructions[0] >> 1) & 1) == 1;
      var word = (instructions[0] & 1) == 1;

      var operation = _transOperation[instructions[0]];
      var hasModRM = _transFormat[instructions[0]] == EncodingFormat.ModRM;
      var isImmediateToAcc = _transFormat[instructions[0]] == EncodingFormat.ImmediateToAccumulator;

      Register? reg;
      Register? rm;
      Register? source = null;
      Register? destination = null;
      ushort? ImmediateValue = null;

      if (hasModRM)
      {
        // this is where our logic currently breaks, we're assuming mod = 11 here
        (reg, rm) = ParseModRMByte(instructions[1], word);
        source = !direction ? reg : rm;
        destination = direction ? reg : rm;
      }
      else if (isImmediateToAcc)
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
      };
    }

    public static (Register? reg, Register? rm) ParseModRMByte(byte instruction, bool word)
    {
      var reg = (instruction >> 3) & 0b111;
      var rm = instruction & 0b111;
      var offset = word ? 8 : 0;
      return ((Register)(reg + offset), (Register)(rm + offset));
    }
  }
}
