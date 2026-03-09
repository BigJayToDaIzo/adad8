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
}
