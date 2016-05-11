namespace Simulator
{
    public delegate void Dumper();

    public struct AddressingModeDumper
    {
        public Dumper ByteDumper;
        public Dumper DisassemblyDumper;
    }
}
