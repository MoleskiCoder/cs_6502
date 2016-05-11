////#define INSTRUCTION_COUNT
////#define ADDRESS_PROFILE

////#define TEST_SUITE1
#define TEST_SUITE2

#if DEBUG
#define DISSASSEMBLE
#endif

namespace Simulator
{
	using System;
#if DISSASSEMBLE
	using System.Collections.Generic;
#endif
	using System.IO;

	public class System6502 : MOS6502, IDisposable
	{
		private readonly TimeSpan pollInterval = new TimeSpan(0, 0, 0, 0, 250);

		private readonly System.Timers.Timer inputPollTimer;

		private readonly byte[] memory;

#if INSTRUCTION_COUNT
		private readonly ulong[] instructionCounts;
#endif
#if ADDRESS_PROFILE
		private readonly ulong[] addressProfiles;
#endif

		private readonly ushort input;
		private readonly ushort output;

#if TEST_SUITE2
		private ushort oldPC = 0;
#endif

		private byte breakInstruction;
		private bool breakAllowed;

#if DISSASSEMBLE
		private Dictionary<AddressingMode, AddressingModeDumper> dumper;
#endif

		private bool disposed;

		public System6502(ushort addressInput, ushort addressOutput, byte breakInstruction, bool breakAllowed)
		{
			this.input = addressInput;
			this.output = addressOutput;

			this.breakInstruction = breakInstruction;
			this.breakAllowed = breakAllowed;

			this.memory = new byte[0x10000];

#if INSTRUCTION_COUNT
			this.instructionCounts = new ulong[0x100];
#endif

#if ADDRESS_PROFILE
			this.addressProfiles = new ulong[0x10000];
#endif

			this.inputPollTimer = new System.Timers.Timer(this.pollInterval.TotalMilliseconds);
			this.inputPollTimer.Elapsed += this.InputPollTimer_Elapsed;
			this.inputPollTimer.Start();

#if DISSASSEMBLE
			this.dumper = new Dictionary<AddressingMode, AddressingModeDumper>()
			{
				{ AddressingMode.Illegal, new AddressingModeDumper {ByteDumper = Dump_Nothing, DisassemblyDumper = Dump_Nothing}},
				{ AddressingMode.Implied, new AddressingModeDumper {ByteDumper = Dump_Nothing, DisassemblyDumper = Dump_Nothing}},
				{ AddressingMode.Immediate, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_imm}},
				{ AddressingMode.Relative, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_rel}},
				{ AddressingMode.XIndexed, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_xind}},
				{ AddressingMode.IndexedY, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_indy}},
				{ AddressingMode.ZeroPage, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_zp}},
				{ AddressingMode.ZeroPageX, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_zpx}},
				{ AddressingMode.ZeroPageY, new AddressingModeDumper {ByteDumper = Dump_Byte, DisassemblyDumper = Dump_zpy}},
				{ AddressingMode.Absolute, new AddressingModeDumper {ByteDumper = Dump_DByte, DisassemblyDumper = Dump_abs}},
				{ AddressingMode.AbsoluteX, new AddressingModeDumper {ByteDumper = Dump_DByte, DisassemblyDumper = Dump_absx}},
				{ AddressingMode.AbsoluteY, new AddressingModeDumper {ByteDumper = Dump_DByte, DisassemblyDumper = Dump_absy}},
				{ AddressingMode.Indirect, new AddressingModeDumper {ByteDumper = Dump_DByte, DisassemblyDumper = Dump_ind}},
			};
#endif
		}

		public System6502(ushort addressInput, ushort addressOutput)
		:	this(addressInput, addressOutput, 0x00, false)
		{
		}

		public System6502(ushort addressInput, ushort addressOutput, byte breakInstruction)
		:	this(addressInput, addressOutput, breakInstruction, true)
		{
		}

		public byte BreakInstruction
		{
			get
			{
				return this.breakInstruction;
			}

			set
			{
				this.breakInstruction = value;
			}
		}

		public bool BreakAllowed
		{
			get
			{
				return this.breakAllowed;
			}

			set
			{
				this.breakAllowed = value;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Clear()
		{
			this.ClearMemory();
			this.ResetRegisters();

#if TEST_SUITE2
			this.oldPC = (ushort)0xffff;
#endif
		}

		public void LoadRom(string path, ushort offset)
		{
			var file = File.Open(path, FileMode.Open);
			using (var reader = new BinaryReader(file))
			{
				var size = file.Length;
				reader.Read(this.memory, offset, (int)size);
			}
		}

		protected void ClearMemory()
		{
			Array.Clear(this.memory, 0, this.memory.Length);
		}

		protected override bool Step()
		{
#if TEST_SUITE1
			if (this.PC == 0x45c0)
			{
				var test = this.GetByte(0x0210);
				if (test == 0xff)
					System.Console.Out.WriteLine("\n** success!!");
				else
					System.Console.Out.WriteLine("\n** {0} failed!!", test);
				return false;
			}
#endif

#if TEST_SUITE2
			if (this.oldPC == this.PC)
			{
				var test = this.GetByte(0x0200);
				System.Console.Out.WriteLine("\n** PC={0:x4}: test={1:x2}: stopped!!", this.PC, test);
				return false;
			}
			else
			{
				this.oldPC = this.PC;
			}
#endif

#if DISSASSEMBLE
			System.Console.Out.Write(
				"\n[{0:d9}] PC={1:x4}:P={2:x2}, A={3:x2}, X={4:x2}, Y={5:x2}, S={6:x2}\t", this.Cycles, this.PC, (byte)this.P, this.A, this.X, this.Y, this.S);
#endif

			return base.Step();
		}

		protected override bool Execute(byte instruction)
		{
#if DISSASSEMBLE
			Dump_ByteValue(instruction);
#endif

#if ADDRESS_PROFILE
			var profileAddress = (ushort)(this.PC - 1);   // We've already completed the fetch cycle.
			var currentCycles = this.Cycles;
#endif

#if INSTRUCTION_COUNT
			++this.instructionCounts[instruction];
#endif

			var returnValue = base.Execute(instruction);

#if ADDRESS_PROFILE
			var cycleCount = this.Cycles - currentCycles;
			this.addressProfiles[profileAddress] += cycleCount;
#endif

			if (this.BreakAllowed && instruction == this.BreakInstruction)
			{
				return false;
			}

			return returnValue;
		}

#if DISSASSEMBLE
		protected override bool Execute(Instruction instruction)
		{
			var mode = instruction.Mode;
			var mnemomic = instruction.Display;

			var dumper = this.dumper[mode];

			dumper.ByteDumper();

			System.Console.Out.Write("\t{0} ", mnemomic);
			dumper.DisassemblyDumper();

			return base.Execute(instruction);
		}
#endif

		protected override byte GetByte(ushort offset)
		{
			var content = this.memory[offset];
			if (offset == this.input)
			{
				this.memory[offset] = 0x0;
			}

			return content;
		}

		protected override void SetByte(ushort offset, byte value)
		{
			this.memory[offset] = value;
			if (offset == this.output)
			{
				System.Console.Out.Write((char)value);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !this.disposed)
			{
				if (this.inputPollTimer != null)
				{
					this.inputPollTimer.Stop();
					this.inputPollTimer.Dispose();
				}

				this.disposed = true;
			}
		}

		private void InputPollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (System.Console.KeyAvailable)
			{
				var key = System.Console.Read();
				this.SetByte(this.input, (byte)key);
			}
		}

#if DISSASSEMBLE
		private static void Dump_Nothing()
		{
		}

		private static void Dump_ByteValue(byte value)
		{
			System.Console.Out.Write("{0:x2}", value);
		}

		private void Dump_Byte(ushort address)
		{
			Dump_ByteValue(this.GetByte(address));
		}

		private void Dump_Byte()
		{
			this.Dump_Byte(this.PC);
		}

		private void Dump_DByte()
		{
			this.Dump_Byte(this.PC);
			this.Dump_Byte((ushort)(this.PC + 1));
		}

		private static void dump_imp()
		{
		}

		private static void Dump_A()
		{
			System.Console.Out.Write("A");
		}

		private void Dump_imm()
		{
			System.Console.Out.Write("#${0:x2}", this.GetByte(this.PC));
		}

		private void Dump_abs()
		{
			System.Console.Out.Write("${0:x4}", this.GetWord(this.PC));
		}

		private void Dump_zp()
		{
			System.Console.Out.Write("${0:x2}", this.GetByte(this.PC));
		}

		private void Dump_zpx()
		{
			System.Console.Out.Write("${0:x2},X", this.GetByte(this.PC));
		}

		private void Dump_zpy()
		{
			System.Console.Out.Write("${0:x2},Y", this.GetByte(this.PC));
		}

		private void Dump_absx()
		{
			System.Console.Out.Write("${0:x4},X", this.GetWord(this.PC));
		}

		private void Dump_absy()
		{
			System.Console.Out.Write("${0:x4},Y", this.GetWord(this.PC));
		}

		private void Dump_xind()
		{
			System.Console.Out.Write("(${0:x2},X)", this.GetByte(this.PC));
		}

		private void Dump_indy()
		{
			System.Console.Out.Write("(${0:x2}),Y", this.GetByte(this.PC));
		}

		private void Dump_ind()
		{
			System.Console.Out.Write("(${0:x4})", this.GetWord(this.PC));
		}

		private void Dump_rel()
		{
			System.Console.Out.Write("{0:x4}", (ushort)(1 + PC + (sbyte)this.GetByte(this.PC)));
		}
#endif

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(this.GetType().FullName);
			}
		}
	}
}
