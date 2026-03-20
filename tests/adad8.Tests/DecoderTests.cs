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

  [Fact]
  public void Decode_0x05_AddImmediateToAx()
  {
    var instruction = Decoder.Decode([0x05, 0x34, 0x12]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.True(instruction.Word);
    Assert.Equal(Register.AX, instruction.Destination);
    Assert.Null(instruction.Source);
    Assert.Equal((ushort)0x1234, instruction.Immediate);
  }

  [Fact]
  public void Decode_0x00_ModRM_MemoryOperand_BxSi()
  {
    // 0x00 = ADD r/m8, r8 (d=0, w=0)
    // ModR/M byte 0x00 = MOD=00, REG=000 (AL), R/M=000 ([BX+SI])
    var instruction = Decoder.Decode([0x00, 0x00]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.Equal(Register.AL, instruction.Source);
    Assert.NotNull(instruction.MemoryOperand);
    Assert.Equal(Register.BX, instruction.MemoryOperand.Base);
    Assert.Equal(Register.SI, instruction.MemoryOperand.Index);
    Assert.Null(instruction.MemoryOperand.Displacement);
  }

  [Fact]
  public void Decode_0x00_ModRM_Mod00_BxSi_SegmentIsDS()
  {
    // [BX+SI] defaults to DS segment
    var instruction = Decoder.Decode([0x00, 0x00]);

    Assert.Equal(Register.DS, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_0x00_ModRM_Mod01_BpSi_SegmentIsSS()
  {
    // 0x42 = MOD=01, REG=000 (AL), R/M=010 ([BP+SI+disp8])
    // BP-based addressing defaults to SS
    var instruction = Decoder.Decode([0x00, 0x42, 0x00]);

    Assert.Equal(Register.SS, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_0x00_ModRM_Mod01_BxSi_Displacement8()
  {
    // 0x00 = ADD r/m8, r8 (d=0, w=0)
    // ModR/M byte 0x40 = MOD=01, REG=000 (AL), R/M=000 ([BX+SI+disp8])
    // Displacement byte 0xFC = -4 signed
    var instruction = Decoder.Decode([0x00, 0x40, 0xFC]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.Equal(Register.AL, instruction.Source);
    Assert.NotNull(instruction.MemoryOperand);
    Assert.Equal(Register.BX, instruction.MemoryOperand.Base);
    Assert.Equal(Register.SI, instruction.MemoryOperand.Index);
    Assert.Equal((short)-4, instruction.MemoryOperand.Displacement);
  }

  [Fact]
  public void Decode_0x00_ModRM_Mod10_BxSi_Displacement16()
  {
    // 0x00 = ADD r/m8, r8 (d=0, w=0)
    // ModR/M byte 0x80 = MOD=10, REG=000 (AL), R/M=000 ([BX+SI+disp16])
    // Displacement bytes 0x00, 0x10 = 0x1000 (little-endian)
    var instruction = Decoder.Decode([0x00, 0x80, 0x00, 0x10]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.Equal(Register.AL, instruction.Source);
    Assert.NotNull(instruction.MemoryOperand);
    Assert.Equal(Register.BX, instruction.MemoryOperand.Base);
    Assert.Equal(Register.SI, instruction.MemoryOperand.Index);
    Assert.Equal((short)0x1000, instruction.MemoryOperand.Displacement);
  }

  [Fact]
  public void Decode_SegmentPrefix_DecodesOpcodeAfterPrefix()
  {
    // CS: ADD CL, BL — prefix 0x2E + opcode 0x00 + ModR/M 0xD9 (MOD=11, REG=011=BL, R/M=001=CL)
    var instruction = Decoder.Decode([0x2E, 0x00, 0xD9]);

    Assert.Equal(Register.CL, instruction.Destination);
    Assert.Equal(Register.BL, instruction.Source);
  }

  [Fact]
  public void Decode_SegmentPrefix_OverridesDefaultSegmentOnMemoryOperand()
  {
    // ES: ADD [BX+SI], AL — 0x26 prefix + 0x00 opcode + 0x00 ModR/M (MOD=00, R/M=000)
    // [BX+SI] normally defaults to DS, but ES prefix should override to ES
    var instruction = Decoder.Decode([0x26, 0x00, 0x00]);

    Assert.Equal(Register.ES, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_CsSegmentPrefix_OverridesDefaultSegmentOnMemoryOperand()
  {
    // CS: ADD [BX+SI], AL — 0x2E prefix + 0x00 opcode + 0x00 ModR/M (MOD=00, R/M=000)
    // [BX+SI] normally defaults to DS, but CS prefix should override to CS
    var instruction = Decoder.Decode([0x2E, 0x00, 0x00]);

    Assert.Equal(Register.CS, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_SsSegmentPrefix_OverridesDefaultSegmentOnMemoryOperand()
  {
    // SS: ADD [BX+SI], AL — 0x36 prefix + 0x00 opcode + 0x00 ModR/M (MOD=00, R/M=000)
    // [BX+SI] normally defaults to DS, but SS prefix should override to SS
    var instruction = Decoder.Decode([0x36, 0x00, 0x00]);

    Assert.Equal(Register.SS, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_DsSegmentPrefix_OverridesDefaultSegmentOnMemoryOperand()
  {
    // DS: ADD [BP+SI], AL — 0x3E prefix + 0x00 opcode + 0x02 ModR/M (MOD=00, REG=000, R/M=010 [BP+SI])
    // [BP+SI] normally defaults to SS, but DS prefix should override to DS
    var instruction = Decoder.Decode([0x3E, 0x00, 0x02]);

    Assert.Equal(Register.DS, instruction.MemoryOperand!.Segment);
  }

  [Fact]
  public void Decode_0x00_ModRM_Mod00_RM110_DirectAddress()
  {
    // 0x00 = ADD r/m8, r8 (d=0, w=0)
    // ModR/M byte 0x06 = MOD=00, REG=000 (AL), R/M=110 (direct address)
    // Displacement bytes 0x00, 0x20 = 0x2000 (little-endian)
    var instruction = Decoder.Decode([0x00, 0x06, 0x00, 0x20]);

    Assert.Equal(Operation.Add, instruction.Operation);
    Assert.Equal(Register.AL, instruction.Source);
    Assert.NotNull(instruction.MemoryOperand);
    Assert.Null(instruction.MemoryOperand.Base);
    Assert.Null(instruction.MemoryOperand.Index);
    Assert.Equal((short)0x2000, instruction.MemoryOperand.Displacement);
  }
}
