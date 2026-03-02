namespace adad8
{
  public record DecodedInstruction
  {
    public Operation Operation { get; init; }
    public bool Direction { get; init; }
    public bool Word { get; init; }
  }
}
