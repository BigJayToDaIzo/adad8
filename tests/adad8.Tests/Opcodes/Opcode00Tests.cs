namespace adad8.Tests.Opcodes
{
  public class Opcode00Tests
  {
    [Fact(Skip = "Integration tests and pipeline isn't ready yet")]
    public void ExecuteInstructionSetSuccessfully()
    {
      var TestList = TestLoader.Load("00");

      foreach (TestCase test in TestList)
      {
        var cpu = TestHarness.Setup(test);
        // cpu.Execute();

        try
        {
          TestHarness.Verify(cpu, test);
        }
        catch (Exception ex)
        {
          throw new Exception($"[{test.Name}] {ex.Message}", ex);
        }
      }
    }
  }
}
