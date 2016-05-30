namespace Simulator
{
	using System;

	public class ProfileLineEventArgs : EventArgs
	{
		private string source;
		private ulong cycles;

		public ProfileLineEventArgs(string source, ulong cycles)
		{
			this.source = source;
			this.cycles = cycles;
		}

		public string Source
		{
			get
			{
				return this.source;
			}
		}

		public ulong Cycles
		{
			get
			{
				return this.cycles;
			}
		}
	}
}
