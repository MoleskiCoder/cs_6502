#define INSTRUCTION_COUNT
#define ADDRESS_PROFILE

namespace Simulator
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class System6502 : MOS6502
	{
		private const ulong PollInterval = 10000;

		private byte[] memory;

#if INSTRUCTION_COUNT
		private ulong[] instructionCounts;
#endif
#if ADDRESS_PROFILE
		private ulong[] addressProfiles;
#endif

		private ushort input;
		private ushort output;

		private byte breakInstruction;

		public System6502(ushort addressInput, ushort addressOutput, byte breakInstruction)
		{
			this.input = addressInput;
			this.output = addressOutput;

			this.breakInstruction = breakInstruction;

			this.memory = new byte[0x10000];

#if INSTRUCTION_COUNT
			this.instructionCounts = new ulong[0x100];
#endif
#if ADDRESS_PROFILE
			this.addressProfiles = new ulong[0x10000];
#endif
		}

		public System6502(ushort addressInput, ushort addressOutput)
		:	this(addressInput, addressOutput, 0x00)
		{
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

		protected void ClearMemory()
		{
			Array.Clear(this.memory, 0, this.memory.Length);
		}

		protected override bool Step() 
		{
			this.Poll();
			return base.Step();
		}

		protected override bool Execute(byte instruction)
		{
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

			if (instruction == this.breakInstruction)
			{
				return false;
			}

			return returnValue;
		}

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

		private void Poll()
		{
			this.PollInput();
		}

		private void PollInput()
		{
			if (this.Cycles % PollInterval == 0)
			{
				if (System.Console.KeyAvailable)
				{
					var key = System.Console.Read();
					this.SetByte(this.input, (byte)key);
				}
			}
		}
	}
}
