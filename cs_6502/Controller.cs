﻿namespace Simulator
{
	using System;
	using System.Globalization;
	using System.IO;

	using Processor;

	public class Controller : IDisposable
	{
		private readonly TimeSpan pollInterval = new TimeSpan(0, 0, 0, 0, 100);
		private readonly System.Timers.Timer inputPollTimer;

		private readonly Configuration configuration;

		private System6502 processor;

		private ushort oldPC = 0;

		private DateTime startTime;
		private DateTime finishTime;

		private Disassembly disassembler;
		private StreamWriter disassemblyLog;

		private Profiler profiler;

#if DEBUG
		private string diagnosticsBuffer;
#endif

		private Symbols symbols;

		private bool disposed;

		public Controller(Configuration configuration)
		{
			this.configuration = configuration;

			this.inputPollTimer = new System.Timers.Timer(this.pollInterval.TotalMilliseconds);
			this.inputPollTimer.Elapsed += this.InputPollTimer_Elapsed;
		}

		public event EventHandler<DisassemblyEventArgs> Disassembly;

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
			this.processor = new System6502(this.configuration.ProcessorLevel);

			if (this.configuration.Disassemble || this.configuration.StopAddressEnabled || this.configuration.StopWhenLoopDetected || this.configuration.ProfileAddresses)
			{
				this.processor.ExecutingInstruction += this.Processor_ExecutingInstruction;
			}

			if (this.configuration.StopBreak)
			{
				this.processor.ExecutedInstruction += this.Processor_ExecutedInstruction;
			}

			this.processor.WritingByte += this.Processor_WritingByte;
			this.processor.ReadingByte += this.Processor_ReadingByte;

			this.processor.InvalidWriteAttempt += this.Processor_InvalidWriteAttempt;

			this.processor.Starting += this.Processor_Starting;
			this.processor.Finished += this.Processor_Finished;

			this.processor.Clear();

			var bbc = !string.IsNullOrWhiteSpace(this.configuration.BbcLanguageRomPath) && !string.IsNullOrWhiteSpace(this.configuration.BbcOSRomPath);
			if (bbc)
			{
				this.processor.LoadRom(this.configuration.BbcOSRomPath, 0xc000);
				this.processor.LoadRom(this.configuration.BbcLanguageRomPath, 0x8000);
			}

			var rom = !string.IsNullOrWhiteSpace(this.configuration.RomPath);
			if (rom)
			{
				this.processor.LoadRom(this.configuration.RomPath, this.configuration.RomLoadAddress);
			}

			var ram = !string.IsNullOrWhiteSpace(this.configuration.RamPath);
			if (ram)
			{
				this.processor.LoadRam(this.configuration.RamPath, this.configuration.RamLoadAddress);
			}

			if (this.configuration.ResetStart)
			{
				this.processor.Reset();
			}
			else
			{
				this.processor.Start(this.configuration.StartAddress);
			}

			this.symbols = new Symbols(this.configuration.DebugFile);

			this.disassembler = new Disassembly(this.processor, this.symbols);
			this.Disassembly += this.Controller_Disassembly;

			this.profiler = new Profiler(this.processor, this.disassembler, this.symbols, this.configuration.CountInstructions, this.configuration.ProfileAddresses);
			this.profiler.StartingOutput += this.Profiler_StartingOutput;
			this.profiler.FinishedOutput += this.Profiler_FinishedOutput;
			this.profiler.StartingLineOutput += this.Profiler_StartingLineOutput;
			this.profiler.FinishedLineOutput += this.Profiler_FinishedLineOutput;
			this.profiler.StartingScopeOutput += this.Profiler_StartingScopeOutput;
			this.profiler.FinishedScopeOutput += this.Profiler_FinishedScopeOutput;
			this.profiler.EmitLine += this.Profiler_EmitLine;
			this.profiler.EmitScope += this.Profiler_EmitScope;
		}

		public void Start()
		{
			this.processor.Run();
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
				if (this.inputPollTimer != null)
				{
					this.inputPollTimer.Stop();
					this.inputPollTimer.Dispose();
				}

				if (this.disassemblyLog != null)
				{
					this.disassemblyLog.Close();
					this.disassemblyLog.Dispose();
				}

				this.disposed = true;
			}
		}

		protected void OnDisassembly(string output)
		{
			this.Disassembly?.Invoke(this, new DisassemblyEventArgs(output));
		}

		private void Processor_Starting(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(this.configuration.DisassemblyLogPath))
			{
				this.disassemblyLog = new StreamWriter(this.configuration.DisassemblyLogPath);
			}

			this.inputPollTimer.Start();
			this.startTime = DateTime.Now;
		}

		private void Processor_Finished(object sender, EventArgs e)
		{
			this.finishTime = DateTime.Now;
			this.profiler.Generate();
		}

		private void Processor_ExecutingInstruction(object sender, AddressEventArgs e)
		{
			if (this.configuration.Disassemble)
			{
				var address = e.Address;
				var cell = e.Cell;
				this.OnDisassembly(
					string.Format(
						CultureInfo.InvariantCulture,
						"\n[{0:d9}] PC={1:x4}:P={2}, A={3:x2}, X={4:x2}, Y={5:x2}, S={6:x2}\t",
						this.processor.Cycles,
						address,
						(string)this.processor.P,
						this.processor.A,
						this.processor.X,
						this.processor.Y,
						this.processor.S));

				var instruction = this.processor.Instructions[cell];
				var mode = instruction.Mode;
				this.OnDisassembly(this.disassembler.Dump_ByteValue(cell));
				this.OnDisassembly(this.disassembler.DumpBytes(mode, (ushort)(address + 1)));
				this.OnDisassembly(string.Format(CultureInfo.InvariantCulture, "\t{0} ", this.disassembler.Disassemble(address)));
			}

			if (this.configuration.StopAddressEnabled && this.configuration.StopAddress == e.Address)
			{
				this.processor.Proceed = false;
			}

			if (this.configuration.StopWhenLoopDetected)
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

		private void Processor_ExecutedInstruction(object sender, AddressEventArgs e)
		{
			if (this.configuration.StopBreak)
			{
				if (this.configuration.BreakInstruction == e.Cell)
				{
					this.processor.Proceed = false;
				}
			}
		}

		private void Controller_Disassembly(object sender, DisassemblyEventArgs e)
		{
			var output = e.Output;

#if DEBUG
			this.BufferDiagnosticsOutput(output);
#endif

			if (this.disassemblyLog != null)
			{
				this.disassemblyLog.Write(output);
			}
		}

		private void Processor_WritingByte(object sender, AddressEventArgs e)
		{
			if (e.Address == this.configuration.OutputAddress)
			{
				this.HandleByteWritten(e.Cell);
			}
		}

		private void Processor_ReadingByte(object sender, AddressEventArgs e)
		{
			var address = e.Address;
			if (e.Address == this.configuration.InputAddress)
			{
				var cell = e.Cell;
				if (cell != 0x0)
				{
					this.HandleByteRead(cell);
					this.processor.SetByte(address, 0x0);
				}
			}
		}

		private void Processor_InvalidWriteAttempt(object sender, AddressEventArgs e)
		{
			var address = e.Address;
			var value = e.Cell;
			this.OnDisassembly(string.Format(CultureInfo.InvariantCulture, "Invalid write: ${0:x4}:{1:x2}\n", address, value));
		}

		private void InputPollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (System.Console.KeyAvailable)
			{
				var key = System.Console.ReadKey(true);
				var character = key.KeyChar;
				this.processor.SetByte(this.configuration.InputAddress, (byte)character);
			}
		}

		private void BufferDiagnosticsOutput(string output)
		{
#if DEBUG
			foreach (var character in output)
			{
				this.diagnosticsBuffer += character;
				if (character == '\n')
				{
					System.Diagnostics.Debug.Write(this.diagnosticsBuffer);
					this.diagnosticsBuffer = string.Empty;
				}
			}
#endif
		}

		private void HandleByteWritten(byte cell)
		{
			var character = (char)cell;
			if (this.configuration.BbcVduEmulation)
			{
				switch (cell)
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
			this.OnDisassembly(string.Format(CultureInfo.InvariantCulture, "Write: {0:x2}:{1}\n", cell, character));
#endif
		}

		private void HandleByteRead(byte cell)
		{
			var character = (char)cell;
#if DEBUG
			this.OnDisassembly(string.Format(CultureInfo.InvariantCulture, "Read: {0:x2}:{1}\n", cell, character));
#endif
		}

		private void Profiler_EmitScope(object sender, ProfileScopeEventArgs e)
		{
			var proportion = (double)e.Cycles / this.processor.Cycles;
			this.BufferDiagnosticsOutput(string.Format(CultureInfo.InvariantCulture, "\t[{0:P2}][{1:d9}][{2:d9}]\t{3}\n", proportion, e.Cycles, e.Count, e.Scope));
		}

		private void Profiler_EmitLine(object sender, ProfileLineEventArgs e)
		{
			var proportion = (double)e.Cycles / this.processor.Cycles;
			this.BufferDiagnosticsOutput(string.Format(CultureInfo.InvariantCulture, "\t[{0:P2}][{1:d9}]\t{2}\n", proportion, e.Cycles, e.Source));
		}

		private void Profiler_FinishedScopeOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Finished profiler scope output...\n");
		}

		private void Profiler_StartingScopeOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Starting profiler scope output...\n");
		}

		private void Profiler_FinishedLineOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Finished profiler line output...\n");
		}

		private void Profiler_StartingLineOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Starting profiler line output...\n");
		}

		private void Profiler_FinishedOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Finished profiler output...\n");
		}

		private void Profiler_StartingOutput(object sender, EventArgs e)
		{
			this.BufferDiagnosticsOutput("Starting profiler output...\n");
		}
	}
}
