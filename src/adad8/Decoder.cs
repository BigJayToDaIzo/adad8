namespace adad8
{
  public static class Decoder
  {
    public static DecodedInstruction Decode(byte[] instructions)
    {
      var opcode = instructions[0] >> 2;
      var direction = ((instructions[0] >> 1) & 1) == 1;
      var word = (instructions[0] & 1) == 1;

      var operation = opcode switch
      {
        0 => Operation.Add,
        10 => Operation.Sub,
        _ => throw new Exception("Invalid Opcode"),
      };

      return new DecodedInstruction
      {
        Operation = operation,
        Direction = direction,
        Word = word,
      };
    }
  }
}
