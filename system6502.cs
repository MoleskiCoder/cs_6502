using System;
using System.Collections.Generic;
using System.IO;

namespace Simulator
{
	class system6502 : mos6502
	{
		public system6502(ushort addressInput, ushort addressOutput, byte breakInstruction)
		{
			this.input = addressInput;
			this.output = addressOutput;

			this.breakInstruction = breakInstruction;

			this.memory = new byte[0x10000];
			this.instructionCounts = new Dictionary<ushort, ulong>();
			this.addressProfiles = new Dictionary<ushort, ulong>();
		}

		public system6502(ushort addressInput, ushort addressOutput)
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
			var profileAddress = (ushort)(this.PC - 1);   // We've already completed the fetch cycle.
			var currentCycles = this.Cycles;

			ulong instructionCount;
			if (this.instructionCounts.TryGetValue(instruction, out instructionCount))
				this.instructionCounts[instruction] = ++instructionCount;
			else
				this.instructionCounts[instruction] = 1;

			var returnValue = base.Execute(instruction);

			var cycleCount = this.Cycles - currentCycles;

			ulong addressProfile;
			if (this.addressProfiles.TryGetValue(profileAddress, out addressProfile))
				this.addressProfiles[profileAddress] = addressProfile + cycleCount;
			else
				this.addressProfiles[profileAddress] = cycleCount;

			if (instruction == this.breakInstruction)
				return false;

			return returnValue;
		}

		protected override byte GetByte(ushort offset)
		{
			var content = this.memory[offset];
			if (offset == this.input)
				this.memory[offset] = 0x0;
			return content;
		}

		protected override void SetByte(ushort offset, byte value)
		{
			this.memory[offset] = value;
			if (offset == this.output)
				System.Console.Out.Write((char)value);
		}

		private byte[] memory;
		private Dictionary<ushort, ulong> instructionCounts;
		private Dictionary<ushort, ulong> addressProfiles;

		private ushort input;
		private ushort output;

		private byte breakInstruction;

		private const ulong pollInterval = 10000;

		private void Poll()
		{
			this.PollInput();
		}

		private void PollInput()
		{
			if (this.Cycles % pollInterval == 0)
			{
				if (System.Console.KeyAvailable)
				{
					var key = System.Console.Read();
					this.SetByte(input, (byte)key);
				}
			}
		}
	};
}
