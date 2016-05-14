namespace Simulator
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public class System6502 : MOS6502, IDisposable
	{
		private readonly TimeSpan pollInterval = new TimeSpan(0, 0, 0, 0, 250);

		private readonly System.Timers.Timer inputPollTimer;

		private readonly byte[] memory;

		private readonly ulong[] instructionCounts;
		private readonly ulong[] addressProfiles;

		private readonly ushort input;
		private readonly ushort output;

		private byte breakInstruction;
		private bool breakAllowed;

		private bool disassemble;
		private bool countInstructions;
		private bool profileAddresses;

		private bool proceed = true;

		private Dictionary<AddressingMode, AddressingModeDumper> dumpers;

		private bool disposed;

		public System6502(ushort addressInput, ushort addressOutput, byte breakInstruction, bool breakAllowed)
		{
			this.input = addressInput;
			this.output = addressOutput;

			this.breakInstruction = breakInstruction;
			this.breakAllowed = breakAllowed;

			this.memory = new byte[0x10000];

			this.instructionCounts = new ulong[0x100];
			this.addressProfiles = new ulong[0x10000];

			this.disassemble = false;
			this.countInstructions = false;
			this.profileAddresses = false;

			this.inputPollTimer = new System.Timers.Timer(this.pollInterval.TotalMilliseconds);
			this.inputPollTimer.Elapsed += this.InputPollTimer_Elapsed;
			this.inputPollTimer.Start();

			this.dumpers = new Dictionary<AddressingMode, AddressingModeDumper>()
			{
				{ AddressingMode.Illegal, new AddressingModeDumper { ByteDumper = Dump_Nothing, DisassemblyDumper = Dump_Nothing } },
				{ AddressingMode.Implied, new AddressingModeDumper { ByteDumper = Dump_Nothing, DisassemblyDumper = Dump_A } },
				{ AddressingMode.Immediate, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_imm } },
				{ AddressingMode.Relative, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_rel } },
				{ AddressingMode.XIndexed, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_xind } },
				{ AddressingMode.IndexedY, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_indy } },
				{ AddressingMode.ZeroPage, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zp } },
				{ AddressingMode.ZeroPageX, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zpx } },
				{ AddressingMode.ZeroPageY, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zpy } },
				{ AddressingMode.Absolute, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_abs } },
				{ AddressingMode.AbsoluteX, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_absx } },
				{ AddressingMode.AbsoluteY, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_absy } },
				{ AddressingMode.Indirect, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_ind } },
			};
		}

		public System6502(ushort addressInput, ushort addressOutput)
		:	this(addressInput, addressOutput, 0x00, false)
		{
		}

		public System6502(ushort addressInput, ushort addressOutput, byte breakInstruction)
		:	this(addressInput, addressOutput, breakInstruction, true)
		{
		}

		public event EventHandler<EventArgs> Stepping;

		public event EventHandler<EventArgs> Stepped;

		public byte BreakInstruction
		{
			get
			{
				return this.breakInstruction;
			}

			set
			{
				if (this.breakInstruction != value)
				{
					this.breakInstruction = value;
					this.OnPropertyChanged("BreakInstruction");
				}
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
				if (this.breakAllowed != value)
				{
					this.breakAllowed = value;
					this.OnPropertyChanged("BreakAllowed");
				}
			}
		}

		public bool Disassemble
		{
			get
			{
				return this.disassemble;
			}

			set
			{
				if (this.disassemble != value)
				{
					this.disassemble = value;
					this.OnPropertyChanged("Disassemble");
				}
			}
		}

		public bool CountInstructions
		{
			get
			{
				return this.countInstructions;
			}

			set
			{
				if (this.countInstructions != value)
				{
					this.countInstructions = value;
					this.OnPropertyChanged("CountInstructions");
				}
			}
		}

		public bool ProfileAddresses
		{
			get
			{
				return this.profileAddresses;
			}

			set
			{
				if (this.profileAddresses != value)
				{
					this.profileAddresses = value;
					this.OnPropertyChanged("ProfileAddresses");
				}
			}
		}

		public bool Proceed
		{
			get
			{
				return this.proceed;
			}

			set
			{
				if (this.proceed != value)
				{
					this.proceed = value;
					this.OnPropertyChanged("Proceed");
				}
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

		public override void Run()
		{
			base.Run();

			if (this.CountInstructions)
			{
				byte i = 0;
				var instructions = this.instructionCounts.ToDictionary(s => i++, s => s);

				var organisedInstructions = new Dictionary<byte, Tuple<Instruction, ulong>>();
				foreach (var instruction in instructions)
				{
					var key = instruction.Key;
					var count = instruction.Value;
					var details = this.Instructions[key];
					if (count > 0)
					{
						organisedInstructions[key] = new Tuple<Instruction, ulong>(details, count);
					}
				}
			}

			if (this.ProfileAddresses)
			{
				ushort i = 0;
				var addresses = this.addressProfiles.ToDictionary(s => i++, s => s);

				var organisedAddresses = new Dictionary<ushort, Tuple<Instruction, ulong>>();
				foreach (var address in addresses)
				{
					var key = address.Key;
					var cycles = address.Value;
					var details = this.Instructions[this.memory[key]];
					if (cycles > 0)
					{
						organisedAddresses[key] = new Tuple<Instruction, ulong>(details, cycles);
					}
				}

				var ordered = from address in organisedAddresses
							  let cycles = address.Value.Item2
							  let details = address.Value.Item1
							  let key = address.Key
							  let returned = new Tuple<ushort, Instruction, ulong>(key, details, cycles)
							  orderby cycles descending
							  select returned;
			}
		}

		public override byte GetByte(ushort offset)
		{
			var content = this.memory[offset];
			if (offset == this.input)
			{
				this.memory[offset] = 0x0;
			}

			return content;
		}

		public override void SetByte(ushort offset, byte value)
		{
			this.memory[offset] = value;
			if (offset == this.output)
			{
				System.Console.Out.Write((char)value);
			}
		}

		protected void ClearMemory()
		{
			Array.Clear(this.memory, 0, this.memory.Length);
		}

		protected override bool Step()
		{
			if (this.Disassemble)
			{
				System.Console.Out.Write(
					"\n[{0:d9}] PC={1:x4}:P={2:x2}, A={3:x2}, X={4:x2}, Y={5:x2}, S={6:x2}\t", this.Cycles, this.PC, (byte)this.P, this.A, this.X, this.Y, this.S);
			}

			this.OnStepping();
			try
			{
				return this.Proceed ? base.Step() : false;
			}
			finally
			{
				this.OnStepped();
			}
		}

		protected override bool Execute(byte instruction)
		{
			if (this.Disassemble)
			{
				Dump_ByteValue(instruction);
			}

			ushort profileAddress = 0;
			ulong currentCycles = 0;
			if (this.ProfileAddresses)
			{
				profileAddress = (ushort)(this.PC - 1);   // We've already completed the fetch cycle.
				currentCycles = this.Cycles;
			}

			if (this.CountInstructions)
			{
				++this.instructionCounts[instruction];
			}

			var returnValue = base.Execute(instruction);

			if (this.ProfileAddresses)
			{
				var cycleCount = this.Cycles - currentCycles;
				this.addressProfiles[profileAddress] += cycleCount;
			}

			if (this.BreakAllowed && instruction == this.BreakInstruction)
			{
				return false;
			}

			return returnValue;
		}

		protected override bool Execute(Instruction instruction)
		{
			if (this.Disassemble)
			{
				var mode = instruction.Mode;
				var mnemomic = instruction.Display;

				var dumper = this.dumpers[mode];

				dumper.ByteDumper();

				System.Console.Out.Write("\t{0} ", mnemomic);
				dumper.DisassemblyDumper();
			}

			return base.Execute(instruction);
		}

		protected override ushort GetWord(ushort offset)
		{
			return BitConverter.ToUInt16(this.memory, offset);
		}

		protected void OnStepping()
		{
			var handler = this.Stepping;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		protected void OnStepped()
		{
			var handler = this.Stepped;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
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

		private static void Dump_Nothing()
		{
		}

		private static void Dump_ByteValue(byte value)
		{
			System.Console.Out.Write("{0:x2}", value);
		}

		private static void Dump_A()
		{
			System.Console.Out.Write("A");
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

		private void InputPollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (System.Console.KeyAvailable)
			{
				var key = System.Console.ReadKey(true);
				var character = key.KeyChar;
				this.SetByte(this.input, (byte)character);
			}
		}
	}
}
