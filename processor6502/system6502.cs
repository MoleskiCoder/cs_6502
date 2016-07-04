namespace Processor
{
	using System;
	using System.Diagnostics;
	using System.IO;

	public sealed class System6502 : MOS6502
	{
		public const double Mega = 1000000;
		public const double Milli = 0.001;
		public const uint MemorySize = 0x10000;

		private readonly IMemory memory;

		private readonly double speed;  // Speed in MHz, e.g. 2.0 == 2Mhz, 1.79 = 1.79Mhz

		private readonly Stopwatch highResolutionTimer = new Stopwatch();

		private readonly double cyclesPerSecond;
		private readonly double cyclesPerMillisecond;
		private readonly ulong cyclesPerInterval;

        private ulong intervalCycles;

		private bool running = false;
		private ulong heldCycles = 0;

		public System6502(ProcessorType level, double speed, TimeSpan pollInterval)
		: base(level)
		{
			this.memory = new Memory(MemorySize);

			this.speed = speed;

			this.cyclesPerSecond = this.speed * Mega;     // speed is in MHz
			this.cyclesPerMillisecond = this.cyclesPerSecond * Milli;
			this.cyclesPerInterval = (ulong)(this.cyclesPerMillisecond * pollInterval.TotalMilliseconds);

			this.Starting += this.System6502_Starting;
			this.Finished += this.System6502_Finished;
		}

		public event EventHandler<EventArgs> Starting;

		public event EventHandler<EventArgs> Finished;

		public event EventHandler<EventArgs> Polling;

		public event EventHandler<AddressEventArgs> ExecutingInstruction;

		public event EventHandler<AddressEventArgs> ExecutedInstruction;

		public ulong HeldCycles
		{
			get
			{
				return this.heldCycles;
			}
		}

		public bool Running
		{
			get
			{
				return this.running;
			}
		}

		public IMemory MemoryBus
		{
			get
			{
				return this.memory;
			}
		}

		public override void Initialise()
		{
			base.Initialise();
			this.memory.ClearLocking();
			this.memory.ClearMemory();
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
			return this.memory.GetByte(offset);
		}

		public override void SetByte(ushort offset, byte value)
		{
			this.memory.SetByte(offset, value);
		}

		protected override void Execute(byte cell)
		{
            var oldCycles = this.Cycles;

            this.CheckPoll();

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

            var deltaCycles = this.Cycles - oldCycles;
	        this.intervalCycles += deltaCycles;
		}

		private void CheckPoll()
		{
        	if (this.intervalCycles >= this.cyclesPerInterval)
			{
		        this.intervalCycles -= this.cyclesPerInterval;
				this.Throttle();
				this.OnPolling();
			}
		}

		private void System6502_Starting(object sender, EventArgs e)
		{
			this.highResolutionTimer.Start();
			this.running = true;
		}

		private void System6502_Finished(object sender, EventArgs e)
		{
			this.running = false;
		}

		private void Throttle()
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
	}
}
