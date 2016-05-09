#define INSTRUCTION_COUNT
#define ADDRESS_PROFILE

#define TEST_SUITE1
//#define TEST_SUITE2

namespace Simulator
{
	using System;
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

#if TEST_SUITE2
		private ushort oldPC = 0;
#endif

		private readonly ushort input;
		private readonly ushort output;

		private readonly byte breakInstruction;

		private bool disposed;

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

			this.inputPollTimer = new System.Timers.Timer(this.pollInterval.TotalMilliseconds);
			this.inputPollTimer.Elapsed += this.InputPollTimer_Elapsed;
			this.inputPollTimer.Start();
		}

		public System6502(ushort addressInput, ushort addressOutput)
		:	this(addressInput, addressOutput, 0x00)
		{
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
			var test = this.GetByte(0x0200);
			if (oldPC == PC)
			{
				System.Console.Out.WriteLine("\n** PC={0:x4}: test={1:x2}: stopped!!", this.PC, test);
				return false;
			}
			else
			{
				oldPC = PC;
			}
#endif
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

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(this.GetType().FullName);
			}
		}
	}
}
