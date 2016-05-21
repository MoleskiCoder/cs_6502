namespace Simulator
{
    public delegate void Implementation();

    public struct Instruction
    {
        private Implementation vector;
        private ulong count;
        private AddressingMode mode;
        private string display;

        public Implementation Vector
        {
            get
            {
                return this.vector;
            }

            set
            {
                this.vector = value;
            }
        }

        public ulong Count
        {
            get
            {
                return this.count;
            }

            set
            {
                this.count = value;
            }
        }

        public AddressingMode Mode
        {
            get
            {
                return this.mode;
            }

            set
            {
                this.mode = value;
            }
        }

        public string Display
        {
            get
            {
                return this.display;
            }

            set
            {
                this.display = value;
            }
        }

    }
}
