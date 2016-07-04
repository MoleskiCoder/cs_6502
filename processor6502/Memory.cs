namespace Processor
{
	using System;
	using System.IO;

	public class Memory : IMemory
	{
		private readonly byte[] memory;
		private readonly bool[] locked;

		public Memory(uint memorySize)
		{
			this.memory = new byte[memorySize];
			this.locked = new bool[memorySize];
		}

		public event EventHandler<AddressEventArgs> InvalidWriteAttempt;

		public event EventHandler<AddressEventArgs> WritingByte;

		public event EventHandler<AddressEventArgs> ReadingByte;

		public void ClearMemory()
		{
			Array.Clear(this.memory, 0, this.memory.Length);
		}

		public void ClearLocking()
		{
			Array.Clear(this.locked, 0, this.locked.Length);
		}

		public byte GetByte(ushort offset)
		{
			var content = this.memory[offset];
			this.OnReadingByte(offset, content);
			return content;
		}

		public void SetByte(ushort offset, byte value)
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

		public ushort LoadMemory(string path, ushort offset)
		{
			var file = File.Open(path, FileMode.Open);
			var size = file.Length;
			if (size > this.memory.Length)
			{
				throw new InvalidOperationException("File is too large");
			}

			using (var reader = new BinaryReader(file))
			{
				reader.Read(this.memory, offset, (int)size);
			}

			return (ushort)size;
		}

		protected void OnInvalidWriteAttempt(ushort address, byte character)
		{
			this.InvalidWriteAttempt?.Invoke(this, new AddressEventArgs(address, character));
		}

		protected void OnWritingByte(ushort address, byte character)
		{
			this.WritingByte?.Invoke(this, new AddressEventArgs(address, character));
		}

		protected void OnReadingByte(ushort address, byte character)
		{
			this.ReadingByte?.Invoke(this, new AddressEventArgs(address, character));
		}
	}
}
