using adad8.Tests;

public class Opcode00Tests
{
  [Fact]
  public void Execute()
  {
    var TestList = TestLoader.Load("00");

    foreach (TestCase test in TestList)
    {
      var cpu = TestHarness.Setup(test);
    }
  }
}
