namespace adad8
{
  public static class Decoder
  {
    static readonly bool[] _hasModRM = new bool[256];

    static Decoder()
    {
      _hasModRM[0x00] = true;
    }

    public static DecodedInstruction Decode(byte[] instructions)
    {
      var opcode = instructions[0] >> 2;
      var direction = ((instructions[0] >> 1) & 1) == 1;
      var word = (instructions[0] & 1) == 1;
      var hasModRM = _hasModRM[instructions[0]];

      var operation = opcode switch
      {
        0 => Operation.Add,
        _ => throw new Exception("Invalid Opcode"),
      };

      Register? reg;
      Register? rm;
      Register? source = null;
      Register? destination = null;

      if (hasModRM)
      {
        (reg, rm) = ParseModRMByte(instructions[1], word);
        // gotta do dir swap in this bih right hurr now since we pulled the d out
        // directionHandleYo
        source = !direction ? reg : rm;
        destination = direction ? reg : rm;
      }

      return new DecodedInstruction
      {
        Operation = operation,
        Direction = direction,
        Word = word,
        Source = source,
        Destination = destination,
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
