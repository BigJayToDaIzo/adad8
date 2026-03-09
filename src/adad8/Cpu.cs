namespace adad8;

using System.Numerics;

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

    // pointers stack, base
    public ushort SP { get; set; }
    public ushort BP { get; set; }

    // indicies source, destination
    public ushort SI { get; set; }
    public ushort DI { get; set; }

    // segments code, data, stack and extra
    public ushort CS { get; set; }
    public ushort DS { get; set; }
    public ushort SS { get; set; }
    public ushort ES { get; set; }

    // instruction pointer
    public ushort IP { get; set; }

    // flags — 16-bit source of truth, computed bool properties
    // flags ordered from least to most significant bit, we're
    // flowing down instead of left! Notice odd skipping of bits yo!
    // the cool thing about bits, you can leave em turned off from time to time!
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

    // Closing out flags finally to stage the next instruction!

    // memory
    public Memory Mem { get; } = new();

    // instruction queue
    public byte[] InstructionQueue { get; set; } = [];

    // Why Get/Set methods when the properties each have them already you ask?
    // Interesting question! Enumeration translation. Going from Register.XX (enum)
    // to cpu.Register.XX (ushort) yo
    public ushort GetRegisterValue(Register? reg)
    {
        return reg switch
        {
            // enum     => ushort
            Register.AX => AX,
            Register.AH => AH,
            Register.AL => AL,
            Register.BX => BX,
            Register.BH => BH,
            Register.BL => BL,
            Register.CX => CX,
            Register.CH => CH,
            Register.CL => CL,
            Register.DX => DX,
            Register.DH => DH,
            Register.DL => DL,
            Register.SP => SP,
            Register.BP => BP,
            Register.SI => SI,
            Register.DI => DI,
            _ => throw new NotImplementedException(),
        };
    }

    public void SetRegisterValue(Register? reg, ushort registerValue)
    {
        switch (reg)
        {
            // enum : ushort | byte
            case Register.AX:
                AX = registerValue;
                break;
            case Register.AH:
                AH = (byte)registerValue;
                break;
            case Register.AL:
                AL = (byte)registerValue;
                break;
            case Register.BX:
                BX = registerValue;
                break;
            case Register.BH:
                BH = (byte)registerValue;
                break;
            case Register.BL:
                BL = (byte)registerValue;
                break;
            case Register.CX:
                CX = registerValue;
                break;
            case Register.CH:
                CH = (byte)registerValue;
                break;
            case Register.CL:
                CL = (byte)registerValue;
                break;
            case Register.DX:
                DX = registerValue;
                break;
            case Register.DH:
                DH = (byte)registerValue;
                break;
            case Register.DL:
                DL = (byte)registerValue;
                break;
            case Register.SP:
                SP = registerValue;
                break;
            case Register.BP:
                BP = registerValue;
                break;
            case Register.SI:
                SI = registerValue;
                break;
            case Register.DI:
                DI = registerValue;
                break;
        }
    }

    public void SetFlags(int result, ushort source, ushort destination, bool word)
    {
        // carry flag
        CF = word ? result > 0xFFFF : result > 0xFF;
        // auxiliary carry flag
        AF = (source & 0x0F) + (destination & 0x0F) > 0x0F;
        // sign flag
        SF = word ? (result & 0x8000) != 0 : (result & 0x80) != 0;
        // zero flag
        ZF = word ? (result & 0xFFFF) == 0 : (result & 0xFF) == 0;
        // parity flag (Intel legacy silliness)
        PF = word
            ? BitOperations.PopCount((uint)result & 0xFF) % 2 == 0
            : BitOperations.PopCount((uint)result & 0xFFFF) % 2 == 0;
        // overflow flag
        var sourceSignBit = word ? source & 0x8000 : source & 0x80;
        var destSignBit = word ? destination & 0x8000 : destination & 0x80;
        var resSignBit = word ? result & 0x8000 : result & 0x80;
        OF = (sourceSignBit == destSignBit) && resSignBit != sourceSignBit;
    }

    // stub execute method
    public void Execute(DecodedInstruction decodedInstruction)
    {
        // switch on op (wait till 2nd op before switch)
        // set execution flags for next instruction
        // this needs to be abstracted to a method, void SetFlags(DecodedInstruction di)
        // execute operation on source and destination
        IP += (ushort)decodedInstruction.ByteLength;
        var result =
            GetRegisterValue(decodedInstruction.Source)
            + GetRegisterValue(decodedInstruction.Destination);

        // set result into destination
        SetFlags(
            result,
            GetRegisterValue(decodedInstruction.Source),
            GetRegisterValue(decodedInstruction.Destination),
            decodedInstruction.Word
        );
        SetRegisterValue(decodedInstruction.Destination, (ushort)result);
    }
}
