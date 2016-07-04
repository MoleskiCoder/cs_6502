namespace Processor
{
	using System;

	public interface IMemory
	{
		event EventHandler<AddressEventArgs> InvalidWriteAttempt;

		event EventHandler<AddressEventArgs> WritingByte;

		event EventHandler<AddressEventArgs> ReadingByte;

		byte GetByte(ushort offset);

		void SetByte(ushort offset, byte value);

		void ClearMemory();

		void ClearLocking();

		void LoadRom(string path, ushort offset);

		void LoadRam(string path, ushort offset);

		void LockMemory(ushort offset, ushort length);

		ushort LoadMemory(string path, ushort offset);
	}
}
