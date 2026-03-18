namespace adad8.Tests;

public class CpuTests
{
    // register get method cuz enum translation to byte
    [Fact]
    public void GetRegisterValue_AL_ReturnsAlValue()
    {
        var cpu = new Cpu { AL = 0x42 };

        Assert.Equal(0x42, cpu.GetRegisterValue(Register.AL));
    }

    // register set method cuz enum translation to byte
    [Fact]
    public void SetRegisterValue_AL_SetsAlValue()
    {
        var cpu = new Cpu();
        cpu.SetRegisterValue(Register.AL, 0x42);

        Assert.Equal(0x42, cpu.AL);
    }

    [Fact]
    public void Execute_AddAlCl_ByteRegisterToRegister()
    {
        var cpu = new Cpu { AL = 0x10, CL = 0x20 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x30, cpu.AL);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        // checks number of on bits in the first byte of the instruction
        Assert.True(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlImmediate_ByteImmediateToAccumulator()
    {
        var cpu = new Cpu { AL = 0x10 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = null,
            Destination = Register.AL,
            Immediate = 0x42,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x52, cpu.AL);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxImmediate_WordImmediateToAccumulator()
    {
        var cpu = new Cpu { AX = 0x1000 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = null,
            Destination = Register.AX,
            Immediate = 0x1234,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x2234, cpu.AX);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlCl_ByteCarryAndAuxCarry()
    {
        var cpu = new Cpu { AL = 0x0F, CL = 0x01 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x10, cpu.AL);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.True(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlCl_ByteZeroFlag()
    {
        var cpu = new Cpu { AL = 0x00, CL = 0x00 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x00, cpu.AL);
        Assert.True(cpu.ZF);
        Assert.True(cpu.PF);
        Assert.False(cpu.SF);
        Assert.False(cpu.CF);
        Assert.False(cpu.OF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlCl_ByteSignFlag()
    {
        var cpu = new Cpu { AL = 0x80, CL = 0x01 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x81, cpu.AL);
        Assert.True(cpu.SF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.CF);
        Assert.False(cpu.OF);
        Assert.True(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlCl_ByteOverflowFlag()
    {
        var cpu = new Cpu { AL = 0x7F, CL = 0x01 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x80, cpu.AL);
        Assert.True(cpu.OF);
        Assert.True(cpu.SF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.CF);
        Assert.False(cpu.PF);
        Assert.True(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordRegisterToRegister()
    {
        var cpu = new Cpu { AX = 0x1000, BX = 0x2000 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x3000, cpu.AX);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.True(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordCarryAndAuxCarry()
    {
        var cpu = new Cpu { AX = 0x000F, BX = 0x0001 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x0010, cpu.AX);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.True(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordZeroFlag()
    {
        var cpu = new Cpu { AX = 0x0000, BX = 0x0000 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x0000, cpu.AX);
        Assert.True(cpu.ZF);
        Assert.True(cpu.PF);
        Assert.False(cpu.SF);
        Assert.False(cpu.CF);
        Assert.False(cpu.OF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordSignFlag()
    {
        var cpu = new Cpu { AX = 0x8000, BX = 0x0001 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x8001, cpu.AX);
        Assert.True(cpu.SF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.CF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordOverflowFlag()
    {
        var cpu = new Cpu { AX = 0x7FFF, BX = 0x0001 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x8000, cpu.AX);
        Assert.True(cpu.OF);
        Assert.True(cpu.SF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.CF);
        Assert.True(cpu.PF);
        Assert.True(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxBx_WordCarryOut()
    {
        var cpu = new Cpu { AX = 0xFFFF, BX = 0x0001 };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.BX,
            Destination = Register.AX,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x0000, cpu.AX);
        Assert.True(cpu.CF);
        Assert.True(cpu.ZF);
        Assert.True(cpu.PF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.True(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlMemBx_ByteFromMemory()
    {
        var cpu = new Cpu { AL = 0x10, BX = 0x0200 };
        cpu.Mem.WriteByte(0x0200, 0x05);

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = null,
            Destination = Register.AL,
            MemoryOperand = new MemoryOperand { Base = Register.BX },
        };

        cpu.Execute(instruction);

        Assert.Equal(0x15, cpu.AL);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAxMemBx_WordFromMemory()
    {
        var cpu = new Cpu { AX = 0x1000, BX = 0x0200 };
        cpu.Mem.WriteByte(0x0200, 0x34);
        cpu.Mem.WriteByte(0x0201, 0x12);

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = null,
            Destination = Register.AX,
            MemoryOperand = new MemoryOperand { Base = Register.BX },
        };

        cpu.Execute(instruction);

        Assert.Equal(0x2234, cpu.AX);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddMemBxAl_ByteToMemory()
    {
        var cpu = new Cpu { AL = 0x10, BX = 0x0200 };
        cpu.Mem.WriteByte(0x0200, 0x05);

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.AL,
            Destination = null,
            MemoryOperand = new MemoryOperand { Base = Register.BX },
        };

        cpu.Execute(instruction);

        Assert.Equal(0x15, cpu.Mem.ReadByte(0x0200));
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddMemBxAx_WordToMemory()
    {
        var cpu = new Cpu { AX = 0x1000, BX = 0x0200 };
        cpu.Mem.WriteByte(0x0200, 0x34);
        cpu.Mem.WriteByte(0x0201, 0x12);

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = true,
            Source = Register.AX,
            Destination = null,
            MemoryOperand = new MemoryOperand { Base = Register.BX },
        };

        cpu.Execute(instruction);

        Assert.Equal(0x34, cpu.Mem.ReadByte(0x0200));
        Assert.Equal(0x22, cpu.Mem.ReadByte(0x0201));
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void Execute_AddAlDirectAddr_ByteFromDirectAddress()
    {
        var cpu = new Cpu { AL = 0x10 };
        cpu.Mem.WriteByte(0x0300, 0x05);

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = null,
            Destination = Register.AL,
            MemoryOperand = new MemoryOperand { Base = null, Index = null, Displacement = 0x0300 },
        };

        cpu.Execute(instruction);

        Assert.Equal(0x15, cpu.AL);
        Assert.False(cpu.CF);
        Assert.False(cpu.ZF);
        Assert.False(cpu.SF);
        Assert.False(cpu.OF);
        Assert.False(cpu.PF);
        Assert.False(cpu.AF);
    }

    [Fact]
    public void ResolveEffectiveAddress_WithSegment_AddsSegmentShifted()
    {
        var cpu = new Cpu { DS = 0x0200, BX = 0x0010 };

        var operand = new MemoryOperand
        {
            Base = Register.BX,
            Segment = Register.DS,
        };

        var address = cpu.ResolveEffectiveAddress(operand);

        Assert.Equal((ushort)0x2010, address);
    }

    [Fact]
    public void MemoryOperand_HasSegmentField()
    {
        var operand = new MemoryOperand
        {
            Base = Register.BX,
            Segment = Register.DS,
        };

        Assert.Equal(Register.DS, operand.Segment);
    }

    [Fact]
    public void GetRegisterValue_InvalidRegister_ThrowsInvalidRegisterException()
    {
        var cpu = new Cpu();

        Assert.Throws<InvalidRegisterException>(() => cpu.GetRegisterValue((Register)99));
    }

    [Fact]
    public void GetRegisterValue_InvalidRegister_MessageContainsRegisterValue()
    {
        var cpu = new Cpu();
        var ex = Assert.Throws<InvalidRegisterException>(() => cpu.GetRegisterValue((Register)99));

        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public void SetRegisterValue_InvalidRegister_ThrowsInvalidRegisterException()
    {
        var cpu = new Cpu();

        Assert.Throws<InvalidRegisterException>(() => cpu.SetRegisterValue((Register)99, 0x42));
    }

    [Fact]
    public void Execute_AdvancesIP_ByInstructionByteLength()
    {
        var cpu = new Cpu
        {
            IP = 0x0000,
            AL = 0x10,
            CL = 0x20,
        };

        var instruction = new DecodedInstruction
        {
            Operation = Operation.Add,
            Direction = false,
            Word = false,
            Source = Register.CL,
            Destination = Register.AL,
            ByteLength = 2,
        };

        cpu.Execute(instruction);

        Assert.Equal(0x0002, cpu.IP);
    }

    [Fact]
    public void ResolveEffectiveAddress_BxPlusSi_ReturnsSum()
    {
        var cpu = new Cpu { BX = 0x0100, SI = 0x0010 };

        var operand = new MemoryOperand { Base = Register.BX, Index = Register.SI };

        var address = cpu.ResolveEffectiveAddress(operand);

        Assert.Equal((ushort)0x0110, address);
    }

    [Fact]
    public void ResolveEffectiveAddress_BxOnly_ReturnsBaseValue()
    {
        var cpu = new Cpu { BX = 0x0200 };

        var operand = new MemoryOperand { Base = Register.BX, Index = null };

        var address = cpu.ResolveEffectiveAddress(operand);

        Assert.Equal((ushort)0x0200, address);
    }

    [Fact]
    public void MemoryOperand_HoldsBaseIndexDisplacement()
    {
        var operand = new MemoryOperand
        {
            Base = Register.BX,
            Index = Register.SI,
            Displacement = 0x05,
        };

        Assert.Equal(Register.BX, operand.Base);
        Assert.Equal(Register.SI, operand.Index);
        Assert.Equal((short?)0x05, operand.Displacement);
    }
}
