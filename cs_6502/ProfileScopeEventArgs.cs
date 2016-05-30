namespace Simulator
{
	using System;

	public class ProfileScopeEventArgs : EventArgs
	{
		private string scope;
		private ulong cycles;

		public ProfileScopeEventArgs(string scope, ulong cycles)
		{
			this.scope = scope;
			this.cycles = cycles;
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
	}
}
