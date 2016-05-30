namespace Simulator
{
	using System;

	public class ProfileLineEventArgs : EventArgs
	{
		private string label;
		private string source;
		private ulong cycles;

		public ProfileLineEventArgs(string label, string source, ulong cycles)
		{
			this.label = label;
			this.source = source;
			this.cycles = cycles;
		}

		public string Label
		{
			get
			{
				return this.label;
			}
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
