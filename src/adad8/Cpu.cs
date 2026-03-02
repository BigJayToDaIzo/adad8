namespace adad8;

public class Cpu
{
  // Accumulator register
  public ushort AX { get; set; }
  public byte AH
  {
    get => (byte)(AX >> 8);
    set => AX = (ushort)((AX & 0x00FF) | (value << 8));
  }
  public byte AL
  {
    get => (byte)(AX & 0xFF);
    set => AX = (ushort)((AX & 0xFF00) | value);
  }

  // Base register
  public ushort BX { get; set; }
  public byte BH
  {
    get => (byte)(BX >> 8);
    set => BX = (ushort)((BX & 0x00FF) | (value << 8));
  }
  public byte BL
  {
    get => (byte)(BX & 0xFF);
    set => BX = (ushort)((BX & 0xFF00) | value);
  }

  // Count register
  public ushort CX { get; set; }
  public byte CH
  {
    get => (byte)(CX >> 8);
    set => CX = (ushort)((CX & 0x00FF) | (value << 8));
  }
  public byte CL
  {
    get => (byte)(CX & 0xFF);
    set => CX = (ushort)((CX & 0xFF00) | value);
  }

  // Data register
  public ushort DX { get; set; }
  public byte DH
  {
    get => (byte)(DX >> 8);
    set => DX = (ushort)((DX & 0x00FF) | (value << 8));
  }
  public byte DL
  {
    get => (byte)(DX & 0xFF);
    set => DX = (ushort)((DX & 0xFF00) | value);
  }

  // segments code, stack, data and extra
  public ushort CS { get; set; }
  public ushort SS { get; set; }
  public ushort DS { get; set; }
  public ushort ES { get; set; }

  // pointers
  public ushort SP { get; set; }
  public ushort BP { get; set; }

  // indicies 
  public ushort SI { get; set; }
  public ushort DI { get; set; }

  // instruction pointer
  public ushort IP { get; set; }

  // flags — 16-bit source of truth, computed bool properties
  public ushort Flags { get; set; }
  public bool CF
  {
    get => (Flags & (1 << 0)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 0) : Flags & ~(1 << 0));
  }
  public bool PF
  {
    get => (Flags & (1 << 2)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 2) : Flags & ~(1 << 2));
  }
  public bool AF
  {
    get => (Flags & (1 << 4)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 4) : Flags & ~(1 << 4));
  }
  public bool ZF
  {
    get => (Flags & (1 << 6)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 6) : Flags & ~(1 << 6));
  }
  public bool SF
  {
    get => (Flags & (1 << 7)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 7) : Flags & ~(1 << 7));
  }
  public bool TF
  {
    get => (Flags & (1 << 8)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 8) : Flags & ~(1 << 8));
  }
  public bool IF
  {
    get => (Flags & (1 << 9)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 9) : Flags & ~(1 << 9));
  }
  public bool DF
  {
    get => (Flags & (1 << 10)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 10) : Flags & ~(1 << 10));
  }
  public bool OF
  {
    get => (Flags & (1 << 11)) != 0;
    set => Flags = (ushort)(value ? Flags | (1 << 11) : Flags & ~(1 << 11));
  }

  // memory 
  public Memory Mem { get; } = new();

  // instruction queue
  public byte[] Queue { get; set; } = [];

  // stub execute method 
  public void Execute() { }
}
