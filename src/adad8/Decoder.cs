namespace adad8
{
  public static class Decoder
  {
    public static DecodedInstruction Decode(byte instruction)
    {
      var opcode = instruction >> 2;
      var direction = ((instruction >> 1) & 1) == 1;
      var word = (instruction & 1) == 1;

      var operation = opcode switch
      {
        0 => Operation.Add,
        10 => Operation.Sub,
        _ => throw new Exception("Invalid Opcode"),
      };

      return new DecodedInstruction { Operation = operation, Direction = direction, Word = word };
    }
  }
}
