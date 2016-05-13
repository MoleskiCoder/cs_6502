// Test suite one: https://github.com/pmonta/FPGA-netlist-tools/tree/master/6502-test-code
// Test suite two: https://github.com/Klaus2m5/6502_65C02_functional_tests

//#define SUDOKU
//#define TEST_SUITE1
#define TEST_SUITE2
//#define EHBASIC

namespace Simulator
{
	public class Configuration
	{
		private ushort inputAddress;
		private ushort outputAddress;

		private string romPath;
		private ushort loadAddress;

		private ushort startAddress = 0;

		private bool resetStart = false;

		private bool stopBreak = false;
		private byte breakInstruction = 0x0;

		private bool disassemble = false;

		public Configuration()
		{
#if SUDOKU
			this.inputAddress = (ushort)0xe000;
			this.outputAddress = (ushort)0xe001;
#else
			this.inputAddress = (ushort)0xf004;
			this.outputAddress = (ushort)0xf001;
#endif

#if TEST_SUITE1
			this.romPath = "C:\\github\\cpp\\cpp_6502\\AllSuiteA.bin";
			this.loadAddress = 0x4000;
			this.startAddress = 0x4000;
#endif

#if TEST_SUITE2
			this.romPath = "C:\\github\\cpp\\cpp_6502\\6502_functional_test.bin";
			this.loadAddress = 0x0;
			this.startAddress = 0x400;
#endif

#if EHBASIC
			this.romPath = "C:\\github\\cpp\\cpp_6502\\ehbasic.bin";
			this.loadAddress = 0xc000;
			this.resetStart = true;
#endif

#if SUDOKU
			this.romPath = "C:\\github\\cpp\\cpp_6502\\sudoku.bin";
			this.loadAddress = 0xf000;
			this.resetStart = true;
			this.stopBreak = true;
#endif

#if DEBUG
			//this.disassemble = true;
#endif
		}

		public ushort InputAddress
		{
			get
			{
				return this.inputAddress;
			}
		}

		public ushort OutputAddress
		{
			get
			{
				return this.outputAddress;
			}
		}

		public string RomPath
		{
			get
			{
				return this.romPath;
			}
		}

		public ushort LoadAddress
		{
			get
			{
				return this.loadAddress;
			}
		}

		public ushort StartAddress
		{
			get
			{
				return this.startAddress;
			}
		}

		public bool ResetStart
		{
			get
			{
				return this.resetStart;
			}
		}

		public bool StopBreak
		{
			get
			{
				return this.stopBreak;
			}
		}

		public byte BreakInstruction
		{
			get
			{
				return this.breakInstruction;
			}
		}

		public bool Disassemble
		{
			get
			{
				return this.disassemble;
			}
		}
	}
}