namespace Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DisassemblyEventArgs : EventArgs
    {
        private string output;

        public DisassemblyEventArgs(string output)
        {
            this.output = output;
        }

        public string Output
        {
            get
            {
                return this.output;
            }
        }
    }
}
