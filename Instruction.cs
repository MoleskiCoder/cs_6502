namespace Simulator
{
    public delegate void Implementation();

    public struct Instruction
    {
        public Implementation Vector;
        public ulong Count;
        public AddressingMode Mode;
        public string Display;
    }
}
