namespace adad8.Tests;

public class DecoderTests
{
  [Fact]
  public void Decode_0x00_ReturnsAdd_DirectionZero_WordZero()
  {
    var instruction = Decoder.Decode([0x00, 0x00]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.False(instruction.Direction);
    Assert.False(instruction.Word);
  }

  [Fact]
  public void Decode_0x03_ReturnsAdd_DirectionOne_WordOne()
  {
    var instruction = Decoder.Decode([0x03]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.True(instruction.Direction);
    Assert.True(instruction.Word);
  }

  [Fact]
  public void Decode_AddAlCl_ResolvesRegisters()
  {
    var instruction = Decoder.Decode([0x00, 0xC8]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.Equal(Register.AL, instruction.Destination);
    Assert.Equal(Register.CL, instruction.Source);
  }
}
