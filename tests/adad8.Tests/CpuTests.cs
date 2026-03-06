namespace adad8.Tests;

public class CpuTests
{
  [Fact]
  public void GetRegisterValue_AL_ReturnsAlValue()
  {
    var cpu = new Cpu { AL = 0x42 };

    Assert.Equal(0x42, cpu.GetRegisterValue(Register.AL));
  }

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
    var cpu = new Cpu();
    cpu.AL = 0x10;
    cpu.CL = 0x20;

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
    Assert.True(cpu.PF);
    Assert.False(cpu.AF);
  }

  [Fact]
  public void Execute_AddAlCl_ByteCarryAndAuxCarry()
  {
    var cpu = new Cpu();
    cpu.AL = 0x0F;
    cpu.CL = 0x01;

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
    var cpu = new Cpu();
    cpu.AL = 0x00;
    cpu.CL = 0x00;

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
    var cpu = new Cpu();
    cpu.AL = 0x80;
    cpu.CL = 0x01;

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
    var cpu = new Cpu();
    cpu.AL = 0x7F;
    cpu.CL = 0x01;

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
}
