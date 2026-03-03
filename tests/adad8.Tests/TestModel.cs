using System.Text.Json.Serialization;

namespace adad8.Tests;

public record TestRegisters
{
  [JsonPropertyName("ax")]
  public ushort? Ax { get; init; }

  [JsonPropertyName("bx")]
  public ushort? Bx { get; init; }

  [JsonPropertyName("cx")]
  public ushort? Cx { get; init; }

  [JsonPropertyName("dx")]
  public ushort? Dx { get; init; }

  [JsonPropertyName("cs")]
  public ushort? Cs { get; init; }

  [JsonPropertyName("ss")]
  public ushort? Ss { get; init; }

  [JsonPropertyName("ds")]
  public ushort? Ds { get; init; }

  [JsonPropertyName("es")]
  public ushort? Es { get; init; }

  [JsonPropertyName("sp")]
  public ushort? Sp { get; init; }

  [JsonPropertyName("bp")]
  public ushort? Bp { get; init; }

  [JsonPropertyName("si")]
  public ushort? Si { get; init; }

  [JsonPropertyName("di")]
  public ushort? Di { get; init; }

  [JsonPropertyName("ip")]
  public ushort? Ip { get; init; }

  [JsonPropertyName("flags")]
  public ushort? Flags { get; init; }
}

public record TestState
{
  [JsonPropertyName("regs")]
  public TestRegisters Regs { get; init; } = new();

  [JsonPropertyName("ram")]
  public int[][] Ram { get; init; } = [];

  [JsonPropertyName("queue")]
  public int[] Queue { get; init; } = [];
}

public record TestCase
{
  public override string ToString() => Name;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("bytes")]
  public int[] Bytes { get; init; } = [];

  [JsonPropertyName("initial")]
  public TestState Initial { get; init; } = new();

  [JsonPropertyName("final")]
  public TestState Final { get; init; } = new();

  //[JsonPropertyName("cycles")]

  [JsonPropertyName("hash")]
  public string Hash { get; init; } = string.Empty;

  [JsonPropertyName("idx")]
  public int Idx { get; init; }
}
