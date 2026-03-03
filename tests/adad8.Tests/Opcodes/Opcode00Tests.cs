namespace adad8.Tests.Opcodes
{
  public class Opcode00Tests
  {
    [Fact(Skip = "Focus on Decoder tests")]
    public void ExecuteInstructionSetSuccessfully()
    {
      var TestList = TestLoader.Load("00");

      foreach (TestCase test in TestList)
      {
        var cpu_init = TestHarness.Setup(test);
        // run it through instructions
        cpu_init.Execute();
        // pass the init and final cpu into verify
        TestHarness.Verify(cpu_init, test);
      }
    }
  }
}
