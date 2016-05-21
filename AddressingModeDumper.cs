namespace Simulator
{
    public delegate void Dumper();

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
