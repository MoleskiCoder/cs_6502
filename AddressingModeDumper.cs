using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    public delegate void Dumper();

    public struct AddressingModeDumper
    {
        public Dumper ByteDumper;
        public Dumper DisassemblyDumper;
    }
}
