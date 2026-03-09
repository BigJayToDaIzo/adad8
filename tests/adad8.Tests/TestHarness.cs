namespace adad8.Tests;

public static class TestHarness
{
    // Initial state registers are always fully populated by the test suite.
    // Null-forgiving (!) overrides nullable warnings — the harness guarantees these are set.
    public static Cpu Setup(TestCase test)
    {
        var cpu = new Cpu
        {
            AX = test.Initial.Regs.Ax!.Value,
            BX = test.Initial.Regs.Bx!.Value,
            CX = test.Initial.Regs.Cx!.Value,
            DX = test.Initial.Regs.Dx!.Value,
            CS = test.Initial.Regs.Cs!.Value,
            SS = test.Initial.Regs.Ss!.Value,
            DS = test.Initial.Regs.Ds!.Value,
            ES = test.Initial.Regs.Es!.Value,
            SP = test.Initial.Regs.Sp!.Value,
            BP = test.Initial.Regs.Bp!.Value,
            SI = test.Initial.Regs.Si!.Value,
            DI = test.Initial.Regs.Di!.Value,
            IP = test.Initial.Regs.Ip!.Value,
            Flags = test.Initial.Regs.Flags!.Value,
            InstructionQueue = [.. test.Initial.Queue.Select(b => (byte)b)],
        };

        foreach (int[] addr_val in test.Initial.Ram)
        {
            cpu.Mem.WriteByte(addr_val[0], (byte)addr_val[1]);
        }

        return cpu;
    }

    public static void Verify(Cpu cpu, TestCase test)
    {
        Assert.Equal(test.Final.Regs.Ax ?? test.Initial.Regs.Ax!.Value, cpu.AX);
        Assert.Equal(test.Final.Regs.Bx ?? test.Initial.Regs.Bx!.Value, cpu.BX);
        Assert.Equal(test.Final.Regs.Cx ?? test.Initial.Regs.Cx!.Value, cpu.CX);
        Assert.Equal(test.Final.Regs.Dx ?? test.Initial.Regs.Dx!.Value, cpu.DX);
        Assert.Equal(test.Final.Regs.Cs ?? test.Initial.Regs.Cs!.Value, cpu.CS);
        Assert.Equal(test.Final.Regs.Ss ?? test.Initial.Regs.Ss!.Value, cpu.SS);
        Assert.Equal(test.Final.Regs.Ds ?? test.Initial.Regs.Ds!.Value, cpu.DS);
        Assert.Equal(test.Final.Regs.Es ?? test.Initial.Regs.Es!.Value, cpu.ES);
        Assert.Equal(test.Final.Regs.Sp ?? test.Initial.Regs.Sp!.Value, cpu.SP);
        Assert.Equal(test.Final.Regs.Bp ?? test.Initial.Regs.Bp!.Value, cpu.BP);
        Assert.Equal(test.Final.Regs.Si ?? test.Initial.Regs.Si!.Value, cpu.SI);
        Assert.Equal(test.Final.Regs.Di ?? test.Initial.Regs.Di!.Value, cpu.DI);
        Assert.Equal(test.Final.Regs.Ip ?? test.Initial.Regs.Ip!.Value, cpu.IP);
        Assert.Equal(test.Final.Regs.Flags ?? test.Initial.Regs.Flags!.Value, cpu.Flags);

        foreach (int[] addr_val in test.Final.Ram)
        {
            Assert.Equal((byte)addr_val[1], cpu.Mem.ReadByte(addr_val[0]));
        }
    }
}
