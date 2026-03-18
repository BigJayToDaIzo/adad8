namespace adad8
{
    public record MemoryOperand
    {
        public Register? Base { get; init; }
        public Register? Index { get; init; }
        public short? Displacement { get; init; }
        public Register? Segment { get; init; }
    }
}
