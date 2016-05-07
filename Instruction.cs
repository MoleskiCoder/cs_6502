using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    public delegate void Implementation();

    public class Instruction
    {
        public Implementation vector;
        public ulong count;
        public AddressingMode mode;
        public string display;
    }
}
