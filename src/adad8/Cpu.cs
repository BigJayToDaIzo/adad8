namespace adad8;

public class Cpu
{
  public ushort AX { get; set; }
  public ushort BX { get; set; }
  public ushort CX { get; set; }
  public ushort DX { get; set; }
  public ushort CS { get; set; }
  public ushort SS { get; set; }
  public ushort DS { get; set; }
  public ushort ES { get; set; }
  public ushort SP { get; set; }
  public ushort BP { get; set; }
  public ushort SI { get; set; }
  public ushort DI { get; set; }
  public ushort IP { get; set; }
  public ushort Flags { get; set; }

  public byte[] Memory { get; } = new byte[1_048_576];
  public byte[] Queue { get; set; } = [];

  public void Execute() { }
}
