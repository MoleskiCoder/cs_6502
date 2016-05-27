namespace Processor
{
	using System;

	public class AddressEventArgs : ByteEventArgs
	{
		private ushort address;

		public AddressEventArgs(ushort address, byte cell)
		: base(cell)
		{
			this.address = address;
		}

		public ushort Address
		{
			get
			{
				return this.address;
			}
		}
	}
}
