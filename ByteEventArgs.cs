namespace Simulator
{
    using System;

    public class ByteEventArgs : EventArgs
    {
        private byte cell;

        public ByteEventArgs(byte cell)
        {
            this.cell = cell;
        }

        public byte Cell
        {
            get
            {
                return this.cell;
            }
        }
    }
}
