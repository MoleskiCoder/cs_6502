namespace Processor
{
	using System;
	using System.IO;

	public sealed class System6502 : MOS6502
	{
		private readonly byte[] memory;
		private readonly bool[] locked;

		public System6502(ProcessorType level)
		: base(level)
		{
			this.memory = new byte[0x10000];
			this.locked = new bool[0x10000];
		}

		public event EventHandler<EventArgs> Starting;

		public event EventHandler<EventArgs> Finished;

		public event EventHandler<AddressEventArgs> InvalidWriteAttempt;

		public event EventHandler<AddressEventArgs> WritingByte;

		public event EventHandler<AddressEventArgs> ReadingByte;

		public event EventHandler<AddressEventArgs> ExecutingInstruction;

		public event EventHandler<AddressEventArgs> ExecutedInstruction;

		public void Clear()
		{
			Array.Clear(this.locked, 0, 0x1000);
			this.ClearMemory();
			this.ResetRegisters();
		}

		public void LoadRom(string path, ushort offset)
		{
			var length = this.LoadMemory(path, offset);
			for (var i = 0; i < length; ++i)
			{
				this.locked[offset + i] = true;
			}
		}

		public void LoadRam(string path, ushort offset)
		{
			this.LoadMemory(path, offset);
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

		public override ushort GetWord(ushort offset)
		{
			return BitConverter.ToUInt16(this.memory, offset);
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
