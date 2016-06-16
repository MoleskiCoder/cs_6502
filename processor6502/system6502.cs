namespace Processor
{
	using System;
	using System.Diagnostics;
	using System.IO;

	public sealed class System6502 : MOS6502
	{
		public const double Mega = 1000000;
		public const double Milli = 0.001;

		private readonly byte[] memory;
		private readonly bool[] locked;

		private readonly double speed;  // Speed in MHz, e.g. 2.0 == 2Mhz, 1.79 = 1.79Mhz

		private readonly TimeSpan pollInterval;

		private readonly Stopwatch highResolutionTimer = new Stopwatch();

		private readonly double cyclesPerSecond;
		private readonly double cyclesPerMillisecond;
		private readonly ulong cyclesPerInterval;

		private ulong heldCycles = 0;

		public System6502(ProcessorType level, double speed, TimeSpan pollInterval)
		: base(level)
		{
			this.memory = new byte[0x10000];
			this.locked = new bool[0x10000];

			this.speed = speed;
			this.pollInterval = pollInterval;

			this.cyclesPerSecond = this.speed * Mega;     // speed is in MHz
			this.cyclesPerMillisecond = this.cyclesPerSecond * Milli;
			this.cyclesPerInterval = (ulong)(this.cyclesPerSecond / pollInterval.TotalMilliseconds);

			this.Polling += this.System6502_Polling;
			this.Starting += this.System6502_Starting;
			this.ExecutedInstruction += this.System6502_ExecutedInstruction;
		}

		public event EventHandler<EventArgs> Starting;

		public event EventHandler<EventArgs> Finished;

		public event EventHandler<EventArgs> Polling;

		public event EventHandler<AddressEventArgs> InvalidWriteAttempt;

		public event EventHandler<AddressEventArgs> WritingByte;

		public event EventHandler<AddressEventArgs> ReadingByte;

		public event EventHandler<AddressEventArgs> ExecutingInstruction;

		public event EventHandler<AddressEventArgs> ExecutedInstruction;

		public ulong HeldCycles
		{
			get
			{
				return this.heldCycles;
			}
		}

		public void Clear()
		{
			Array.Clear(this.locked, 0, 0x1000);
			this.ClearMemory();
			this.ResetRegisters();
		}

		public void LoadRom(string path, ushort offset)
		{
			var length = this.LoadMemory(path, offset);
			this.LockMemory(offset, length);
		}

		public void LoadRam(string path, ushort offset)
		{
			this.LoadMemory(path, offset);
		}

		public void LockMemory(ushort offset, ushort length)
		{
			for (var i = 0; i < length; ++i)
			{
				this.locked[offset + i] = true;
			}
		}

		public override void Run()
		{
			this.OnStarting();
			try
			{
				base.Run();
			}
			finally
			{
				this.OnFinished();
			}
		}

		public override byte GetByte(ushort offset)
		{
			var content = this.memory[offset];
			this.OnReadingByte(offset, content);
			return content;
		}

		public override void SetByte(ushort offset, byte value)
		{
			if (this.locked[offset])
			{
				this.OnInvalidWriteAttempt(offset, value);
			}
			else
			{
				this.memory[offset] = value;
				this.OnWritingByte(offset, value);
			}
		}

		protected override void Execute(byte cell)
		{
			// XXXX Fetch byte has already incremented PC.
			var executingAddress = (ushort)(this.PC - 1);

			this.OnExecutingInstruction(executingAddress, cell);
			try
			{
				base.Execute(cell);
			}
			finally
			{
				this.OnExecutedInstruction(executingAddress, cell);
			}
		}

		private void CheckPoll()
		{
			if ((this.Cycles % this.cyclesPerInterval) == 0)
			{
				this.OnPolling();
			}
		}

		private void System6502_ExecutedInstruction(object sender, AddressEventArgs e)
		{
			this.CheckPoll();
		}

		private void System6502_Starting(object sender, EventArgs e)
		{
			this.highResolutionTimer.Start();
		}

		private void System6502_Polling(object sender, EventArgs e)
		{
			var timerCurrent = this.highResolutionTimer.ElapsedMilliseconds;

			var cyclesAllowed = timerCurrent * this.cyclesPerMillisecond;
			var cyclesMismatch = this.Cycles - cyclesAllowed;
			if (cyclesMismatch > 0.0)
			{
				var delay = (int)(cyclesMismatch / this.cyclesPerMillisecond);
				if (delay > 0)
				{
					this.heldCycles += (ulong)cyclesMismatch;
					System.Threading.Thread.Sleep(delay);
				}
			}
		}

		private void ClearMemory()
		{
			Array.Clear(this.memory, 0, this.memory.Length);
		}

		private void OnStarting()
		{
			this.Starting?.Invoke(this, EventArgs.Empty);
		}

		private void OnFinished()
		{
			this.Finished?.Invoke(this, EventArgs.Empty);
		}

		private void OnPolling()
		{
			this.Polling?.Invoke(this, EventArgs.Empty);
		}

		private void OnExecutingInstruction(ushort address, byte instruction)
		{
			this.ExecutingInstruction?.Invoke(this, new AddressEventArgs(address, instruction));
		}

		private void OnExecutedInstruction(ushort address, byte instruction)
		{
			this.ExecutedInstruction?.Invoke(this, new AddressEventArgs(address, instruction));
		}

		private void OnInvalidWriteAttempt(ushort address, byte character)
		{
			this.InvalidWriteAttempt?.Invoke(this, new AddressEventArgs(address, character));
		}

		private void OnWritingByte(ushort address, byte character)
		{
			this.WritingByte?.Invoke(this, new AddressEventArgs(address, character));
		}

		private void OnReadingByte(ushort address, byte character)
		{
			this.ReadingByte?.Invoke(this, new AddressEventArgs(address, character));
		}

		private ushort LoadMemory(string path, ushort offset)
		{
			var file = File.Open(path, FileMode.Open);
			var size = file.Length;
			if (size > 0x10000)
			{
				throw new InvalidOperationException("File is too large");
			}

			using (var reader = new BinaryReader(file))
			{
				reader.Read(this.memory, offset, (int)size);
			}

			return (ushort)size;
		}
	}
}
