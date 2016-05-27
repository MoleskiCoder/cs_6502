namespace Simulator
{
    public delegate string Dumper(ushort current);

    public struct AddressingModeDumper
    {
        private Dumper byteDumper;
        private Dumper disassemblyDumper;

        public Dumper ByteDumper
        {
            get
            {
                return this.byteDumper;
            }

            set
            {
                this.byteDumper = value;
            }
        }

        public Dumper DisassemblyDumper
        {
            get
            {
                return this.disassemblyDumper;
            }

            set
            {
                this.disassemblyDumper = value;
            }
        }
    }
}
