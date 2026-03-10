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
    // early unit allowed us to pass an add instruction with out intended
    // follow up bytes, now that logic dictates a second byte be present
    // we borkied this one
    // var instruction = Decoder.Decode([0x03]);
    var instruction = Decoder.Decode([0x03, 0xC8]);

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

  [Fact]
  public void Decode_0x04_AddImmediateToAl()
  {
    var instruction = Decoder.Decode([0x04, 0x42]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.False(instruction.Word);
    Assert.Equal(Register.AL, instruction.Destination);
    Assert.Null(instruction.Source);
    Assert.Equal((ushort)0x42, instruction.Immediate);
  }
}
