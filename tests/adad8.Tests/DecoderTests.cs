namespace adad8.Tests;

public class DecoderTests
{
  [Fact]
  public void Decode_0x00_ReturnsAdd_DirectionZero_WordZero()
  {
    var instruction = Decoder.Decode(0x00);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.False(instruction.Direction);
    Assert.False(instruction.Word);
  }

  [Fact]
  public void Decode_0x03_ReturnsAdd_DirectionOne_WordOne()
  {
    var instruction = Decoder.Decode(0x03);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.True(instruction.Direction);
    Assert.True(instruction.Word);
  }

  [Fact]
  public void Decode_0x28_ReturnsSub_DirectionZero_WordZero()
  {
    var instruction = Decoder.Decode(0x28);

    Assert.Equal(Operation.Sub, instruction.Operation);
    Assert.False(instruction.Direction);
    Assert.False(instruction.Word);
  }

  [Fact]
  public void Decode_0x00_0xC8_ParsesModRM_Mod11_Reg001_RM000()
  {
    var instruction = Decoder.Decode(0x00, 0xC8);

    Assert.Equal(0b11, instruction.Mod);
    Assert.Equal(0b001, instruction.Reg);
    Assert.Equal(0b000, instruction.Rm);
  }
}
