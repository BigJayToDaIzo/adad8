namespace adad8
{
  public class Memory
  {
    public byte[] Mem { get; } = new byte[1_048_576];

    public byte ReadByte(int address)
    {
      return Mem[address & 0xFFFFF];
    }
    public void WriteByte(int address, byte value)
    {
      Mem[address & 0xFFFFF] = value;
    }
  }
}
