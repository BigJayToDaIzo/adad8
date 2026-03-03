namespace adad8
{
  public static class Decoder
  {
    static readonly bool[] _byteOneLookAhead = new bool[256];
    static Decoder()
    {
      _byteOneLookAhead[0x00] = true;
    }
    public static DecodedInstruction Decode(byte[] instructions)
    {
      var opcode = instructions[0] >> 2;
      var Direction = ((instructions[0] >> 1) & 1) == 1;
      var Word = (instructions[0] & 1) == 1;
      var HasModRM = _byteOneLookAhead[instructions[0]];

      var Op = opcode switch
      {
        0 => Operation.Add,
        _ => throw new Exception("Invalid Opcode"),
      };

      Register? Source = null;
      Register? Destination = null;

      if (HasModRM)
      {
        (Source, Destination) = parseModRMByte(instructions[1], Word, Direction);

      }
      return new DecodedInstruction
      {
        Operation = Op,
        Direction = Direction,
        Word = Word,
        Source = Source,
        Destination = Destination,
      };
    }
    public static (Register? Source, Register? Destination) parseModRMByte(byte instruction, bool word, bool direction)
    {
      var mod = instruction >> 6;
      var reg = (instruction >> 3) & 0b111;
      var rm = instruction & 0b111;
      var Offset = word ? 8 : 0; // may be necessary if additonal instruction bytes follow
      Register? Source = null;
      Register? Destination = null;
      // if (mod == 0b00) {}
      // elseif (mod == 0b01) { // 8 bit displacement }
      // elseif (mod == 0b01) { // 16 bit displacement }
      if (mod == 0b11)
      {
        // no mem access, both are registers
        if (direction)
        {
          Source = (Register)(rm + Offset);
          Destination = (Register)(reg + Offset);
        }
        else
        {
          Source = (Register)(reg + Offset);
          Destination = (Register)(rm + Offset);
        }
      }
      return (Source, Destination);
    }
  }
}
