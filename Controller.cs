namespace Simulator
{
	using System;
	using System.Globalization;
	using System.IO;

	public class Controller : IDisposable
	{
		private Configuration configuration;
		private System6502 processor;

		private bool stopWhenLoopDetected = false;
		private ushort oldPC = 0;

		private DateTime startTime;
		private DateTime finishTime;

		private string disassemblyBuffer;

		private string disassemblyLogPath;
		private StreamWriter disassemblyLog;

		private Symbols symbols;

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
			this.symbols = new Symbols(this.configuration.DebugFile);

			if (this.configuration.StopBreak)
			{
				this.processor = new System6502(
					this.configuration.ProcessorLevel,
					this.symbols,
					this.configuration.InputAddress,
					this.configuration.OutputAddress,
					this.configuration.BreakInstruction);
			}
			else
			{
				this.processor = new System6502(
					this.configuration.ProcessorLevel,
					this.symbols,
					this.configuration.InputAddress,
					this.configuration.OutputAddress);
			}

			this.processor.Stepping += this.Processor_Stepping;
			this.processor.Stepped += this.Processor_Stepped;

			this.processor.Clear();

			var bbc = !string.IsNullOrWhiteSpace(this.configuration.BbcLanguageRomPath) && !string.IsNullOrWhiteSpace(this.configuration.BbcOSRomPath);
			if (bbc)
			{
				this.processor.LoadRom(this.configuration.BbcOSRomPath, 0xc000);
				this.processor.LoadRom(this.configuration.BbcLanguageRomPath, 0x8000);
			}
			else
			{
				this.processor.LoadRom(this.configuration.RomPath, this.configuration.LoadAddress);
			}

			this.processor.BreakAllowed = this.configuration.StopBreak;
			this.stopWhenLoopDetected = this.configuration.StopWhenLoopDetected;

			this.processor.Disassemble = this.configuration.Disassemble;
			this.disassemblyLogPath = this.configuration.DisassemblyLogPath;
			this.processor.CountInstructions = this.configuration.CountInstructions;
			this.processor.ProfileAddresses = this.configuration.ProfileAddresses;

			this.processor.WritingCharacter += this.Processor_WritingCharacter;
			this.processor.ReadingCharacter += this.Processor_ReadingCharacter;

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
			if (!string.IsNullOrWhiteSpace(this.disassemblyLogPath))
			{
				this.disassemblyLog = new StreamWriter(this.disassemblyLogPath);
			}

			this.Processor.Disassembly += this.Processor_Disassembly;
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

				if (this.disassemblyLog != null)
				{
					this.disassemblyLog.Close();
					this.disassemblyLog.Dispose();
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

		private void Processor_Disassembly(object sender, DisassemblyEventArgs e)
		{
			var output = e.Output;

#if DEBUG
			foreach (var character in output)
			{
				this.disassemblyBuffer += character;
				if (character == '\n')
				{
					System.Diagnostics.Debug.Write(this.disassemblyBuffer);
					this.disassemblyBuffer = string.Empty;
				}
			}
#endif

			if (this.disassemblyLog != null)
			{
				this.disassemblyLog.Write(output);
			}
		}

		private void Processor_WritingCharacter(object sender, ByteEventArgs e)
		{
			var binary = e.Cell;
			var character = (char)binary;
			if (this.configuration.BbcVduEmulation)
			{
				switch (binary)
				{
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
						break;
					case 7:
						System.Console.Beep();
						break;
					case 8:
						if (System.Console.CursorLeft > 0)
						{
							--System.Console.CursorLeft;
						}

						break;
					case 9:
						if (System.Console.CursorLeft < System.Console.LargestWindowWidth)
						{
							++System.Console.CursorLeft;
						}

						break;
					case 10:
						if (System.Console.CursorTop < System.Console.LargestWindowHeight)
						{
							++System.Console.CursorTop;
						}

						break;
					case 11:
						if (System.Console.CursorTop > 0)
						{
							--System.Console.CursorTop;
						}

						break;
					case 12:
						System.Console.Clear();
						break;
					case 13:
						System.Console.CursorLeft = 0;
						break;
					case 14:
					case 15:
					case 16:
					case 17:
					case 18:
					case 19:
					case 20:
					case 21:
					case 22:
					case 23:
					case 24:
					case 25:
					case 26:
					case 27:
					case 28:
					case 29:
						break;
					case 30:
						System.Console.SetCursorPosition(0, 0);
						break;
					case 31:
						break;
					case 127:
						--System.Console.CursorLeft;
						System.Console.Write(character);
						break;
					default:
						System.Console.Out.Write(character);
						break;
				}
			}
			else
			{
				System.Console.Out.Write(character);
			}
#if DEBUG
			System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Write: {0:x2}:{1}", binary, character));
#endif
		}

		private void Processor_ReadingCharacter(object sender, ByteEventArgs e)
		{
			var binary = e.Cell;
			var character = (char)binary;
#if DEBUG
			System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Read: {0:x2}:{1}", binary, character));
#endif
		}
	}
}
