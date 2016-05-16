namespace Simulator
{
	using System;

	public class Controller : IDisposable
	{
		private Configuration configuration;
		private System6502 processor;

		private bool stopWhenLoopDetected = false;
		private ushort oldPC = 0;

		private DateTime startTime;
		private DateTime finishTime;

		private bool disposed;

		public Controller(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public System6502 Processor
		{
			get
			{
				return this.processor;
			}
		}

		public DateTime StartTime
		{
			get
			{
				return this.startTime;
			}
		}

		public DateTime FinishTime
		{
			get
			{
				return this.finishTime;
			}
		}

		public void Configure()
		{
			if (this.configuration.StopBreak)
			{
				this.processor = new System6502(
					this.configuration.ProcessorLevel,
					this.configuration.InputAddress,
					this.configuration.OutputAddress,
					this.configuration.BreakInstruction);
			}
			else
			{
				this.processor = new System6502(
					this.configuration.ProcessorLevel,
					this.configuration.InputAddress,
					this.configuration.OutputAddress);
			}

			this.processor.Stepping += this.Processor_Stepping;
			this.processor.Stepped += this.Processor_Stepped;

			this.processor.Clear();

			this.processor.LoadRom(this.configuration.RomPath, this.configuration.LoadAddress);
			this.processor.BreakAllowed = this.configuration.StopBreak;
			this.stopWhenLoopDetected = this.configuration.StopWhenLoopDetected;

			this.processor.Disassemble = this.configuration.Disassemble;
			this.processor.CountInstructions = this.configuration.CountInstructions;
			this.processor.ProfileAddresses = this.configuration.ProfileAddresses;

			if (this.configuration.ResetStart)
			{
				this.processor.Reset();
			}
			else
			{
				this.processor.Start(this.configuration.StartAddress);
			}
		}

		public void Start()
		{
			this.startTime = DateTime.Now;
			try
			{
				this.processor.Run();
			}
			finally
			{
				this.finishTime = DateTime.Now;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !this.disposed)
			{
				if (this.processor != null)
				{
					this.processor.Dispose();
				}

				this.disposed = true;
			}
		}

		private void Processor_Stepping(object sender, EventArgs e)
		{
            if (this.configuration.StopAddressEnabled && this.configuration.StopAddress == this.Processor.PC)
			{
				this.processor.Proceed = false;
			}

			if (this.stopWhenLoopDetected)
			{
				if (this.oldPC == this.processor.PC)
				{
					this.processor.Proceed = false;
				}
				else
				{
					this.oldPC = this.processor.PC;
				}
			}
		}

		private void Processor_Stepped(object sender, EventArgs e)
		{
		}
	}
}
