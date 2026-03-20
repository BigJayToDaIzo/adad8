namespace adad8.Tests.Opcodes
{
    public class Opcode00Tests
    {
        // [Fact(Skip = "Integration tests and pipeline isn't ready yet")]
        [Fact]
        public void ExecuteInstructionSetSuccessfully()
        {
            var TestList = TestLoader.Load("00");

            foreach (TestCase test in TestList)
            {
                var cpu = TestHarness.Setup(test);
                var decoded = Decoder.Decode([.. test.Bytes.Select(b => (byte)b)]);
                if (test.Name == "add byte [ds:7D0Eh], cl")
                    System.Diagnostics.Debugger.Break();
                cpu.Execute(decoded);

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
