namespace adad8
{
  public static class Decoder
  {
    static readonly Operation[] _transOperation = new Operation[256];
    static readonly EncodingFormat[] _transFormat = new EncodingFormat[256];
    static readonly bool[] _hasModRM = new bool[256];

    // obsolete code for tdd based documentation
    // a live look in a commit log how a refactor looks as complexity grows
    // var opcode = instructions[0] >> 2;
    // need Add to be able to switch on the various encoding formats
    // not to mention every instruction beyond add likely needs
    // bit level parsing as opposed to the simple nibble
    // correction, we're moving to byte level parsing, not bit.
    // a byte can only be one of 256 things, LOOKUP TABLE yo!
    static Decoder()
    {
      // this will likely need to be abstracted soon it's about to get rowdy
      // in here!
      // add op
      for (byte i = 0x00; i <= 0x05; i++)
      {
        _transOperation[i] = Operation.Add;
        if (i <= 0x03)
        {
          _transFormat[i] = EncodingFormat.ModRM;
        }
        // loop in sync with transOp bool defaults to false so I can just loop
        // on first four setting true until that pattern breaks the unit tests
        // if (i < 0x04)
        //   _hasModRM[i] = true;
        else
          _transFormat[i] = EncodingFormat.ImmediateToAccumulator;
      }
      // Sub op
      // for (byte i = 0x06; i <= 0x0B; i++) { }
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
        (reg, rm) = ParseModRMByte(instructions[1], word);
        // gotta do dir swap in this bih right hurr now since we pulled the d out
        // directionHandleYo
        source = !direction ? reg : rm;
        destination = direction ? reg : rm;
      }
      else if (isImmediateToAcc)
      {
        destination = word ? Register.AX : Register.AL;
        ImmediateValue = instructions[1];
        // perform op on immediate value and destination register,
        // writing to destination register.
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
      var offset = word ? 8 : 0; // may be necessary if additonal instruction bytes follow
      return ((Register)(reg + offset), (Register)(rm + offset));
    }
  }
}
