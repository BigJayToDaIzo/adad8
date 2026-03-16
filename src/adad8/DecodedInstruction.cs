namespace adad8
{
  public record DecodedInstruction
  {
    public Operation Operation { get; init; }
    public bool Direction { get; init; }
    public bool Word { get; init; }
    public int ByteLength { get; init; }
    public Register? Source { get; init; }
    public Register? Destination { get; init; }
    public ushort? Immediate { get; init; }
    public MemoryOperand? MemoryOperand { get; init; }
  }
}
