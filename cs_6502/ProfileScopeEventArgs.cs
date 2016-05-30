namespace Simulator
{
	using System;

	public class ProfileScopeEventArgs : EventArgs
	{
		private string scope;
		private ulong cycles;
		private ulong count;

		public ProfileScopeEventArgs(string scope, ulong cycles, ulong count)
		{
			this.scope = scope;
			this.cycles = cycles;
			this.count = count;
		}

		public string Scope
		{
			get
			{
				return this.scope;
			}
		}
		
		public ulong Cycles
		{
			get
			{
				return this.cycles;
			}
		}

		public ulong Count
		{
			get
			{
				return this.count;
			}
		}
	}
}
