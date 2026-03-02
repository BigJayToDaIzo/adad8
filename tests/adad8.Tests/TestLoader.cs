using System.IO.Compression;
using System.Text.Json;

namespace adad8.Tests;

public static class TestLoader
{
  static readonly string DataDir = FindDataDirectory();

  public static List<TestCase> Load(string opcode)
  {
    var path = Path.Combine(DataDir, $"{opcode}.json.gz");
    using var file = File.OpenRead(path);
    using var gzip = new GZipStream(file, CompressionMode.Decompress);
    return JsonSerializer.Deserialize<List<TestCase>>(gzip) ?? [];
  }

  private static string FindDataDirectory()
  {
    var dir = Directory.GetCurrentDirectory();
    while (dir is not null)
    {
      if (File.Exists(Path.Combine(dir, "adad8.slnx")))
        return Path.Combine(dir, "tests", "data");
      dir = Path.GetDirectoryName(dir);
    }
    throw new DirectoryNotFoundException("Could not find solution root");
  }
}
