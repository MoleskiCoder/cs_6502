﻿namespace Simulator
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;

	public class System6502 : MOS6502, IDisposable
	{
		private readonly TimeSpan pollInterval = new TimeSpan(0, 0, 0, 0, 100);

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

		private Disassembly disassembler;

		private bool disposed;

		public System6502(ProcessorType level, Symbols symbols, ushort addressInput, ushort addressOutput, byte breakInstruction, bool breakAllowed)
		: base(level)
		{
			this.input = addressInput;
			this.output = addressOutput;

			this.breakInstruction = breakInstruction;
			this.breakAllowed = breakAllowed;

			this.memory = new byte[0x10000];

			this.disassembler = new Disassembly(this, symbols);

			this.instructionCounts = new ulong[0x100];
			this.addressProfiles = new ulong[0x10000];

			this.disassemble = false;
			this.countInstructions = false;
			this.profileAddresses = false;

			this.inputPollTimer = new System.Timers.Timer(this.pollInterval.TotalMilliseconds);
			this.inputPollTimer.Elapsed += this.InputPollTimer_Elapsed;
			this.inputPollTimer.Start();
		}

		public System6502(ProcessorType level, Symbols symbols, ushort addressInput, ushort addressOutput)
		:	this(level, symbols, addressInput, addressOutput, 0x00, false)
		{
		}

		public System6502(ProcessorType level, Symbols symbols, ushort addressInput, ushort addressOutput, byte breakInstruction)
		:	this(level, symbols, addressInput, addressOutput, breakInstruction, true)
		{
		}

		public event EventHandler<EventArgs> Stepping;

		public event EventHandler<EventArgs> Stepped;

		public event EventHandler<DisassemblyEventArgs> Disassembly;

		public event EventHandler<ByteEventArgs> WritingCharacter;

		public event EventHandler<ByteEventArgs> ReadingCharacter;

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
			if (offset == this.input && content != 0x0)
			{
				this.OnReadingCharacter(content);
				this.memory[offset] = 0x0;
			}

			return content;
		}

		public override ushort GetWord(ushort offset)
		{
			return BitConverter.ToUInt16(this.memory, offset);
		}

		public override void SetByte(ushort offset, byte value)
		{
			this.memory[offset] = value;
			if (offset == this.output)
			{
				this.OnWritingCharacter(value);
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
				this.OnDisassembly(
					string.Format(
						CultureInfo.InvariantCulture,
						"\n[{0:d9}] PC={1:x4}:P={2:x2}, A={3:x2}, X={4:x2}, Y={5:x2}, S={6:x2}\t",
						this.Cycles,
						this.PC,
						(byte)this.P,
						this.A,
						this.X,
						this.Y,
						this.S));
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
				this.OnDisassembly(this.disassembler.Dump_ByteValue(instruction));
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

				this.OnDisassembly(this.disassembler.DumpBytes(mode, this.PC));
				this.OnDisassembly(string.Format(CultureInfo.InvariantCulture, "\t{0} ", mnemomic));
				this.OnDisassembly(this.disassembler.DumpOperand(mode, this.PC));
			}

			return base.Execute(instruction);
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

		protected void OnDisassembly(string test)
		{
			var handler = this.Disassembly;
			if (handler != null)
			{
				handler(this, new DisassemblyEventArgs(test));
			}
		}

		protected void OnWritingCharacter(byte character)
		{
			var handler = this.WritingCharacter;
			if (handler != null)
			{
				handler(this, new ByteEventArgs(character));
			}
		}

		protected void OnReadingCharacter(byte character)
		{
			var handler = this.ReadingCharacter;
			if (handler != null)
			{
				handler(this, new ByteEventArgs(character));
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
				var key = System.Console.ReadKey(true);
				var character = key.KeyChar;
				this.SetByte(this.input, (byte)character);
			}
		}
	}
}
