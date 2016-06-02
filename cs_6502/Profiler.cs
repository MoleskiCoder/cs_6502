namespace Simulator
{
	using System;
	using System.Collections.Generic;

	public sealed class Profiler
	{
		private readonly ulong[] instructionCounts;
		private readonly ulong[] addressProfiles;
		private readonly ulong[] addressCounts;

		private readonly string[] addressScopes;
		private readonly Dictionary<string, ulong> scopeCycles;

		private readonly Processor.System6502 processor;
		private readonly Disassembly disassembler;
		private readonly Symbols symbols;

		private readonly bool countInstructions;
		private readonly bool profileAddresses;

		private ulong priorCycleCount = 0;

		public Profiler(Processor.System6502 processor, Disassembly disassembler, Symbols symbols, bool countInstructions, bool profileAddresses)
		{
			this.processor = processor;
			this.disassembler = disassembler;
			this.symbols = symbols;
			this.countInstructions = countInstructions;
			this.profileAddresses = profileAddresses;

			if (countInstructions || profileAddresses)
			{
				this.processor.ExecutingInstruction += this.Processor_ExecutingInstruction;
			}

			if (profileAddresses)
			{
				this.processor.ExecutedInstruction += this.Processor_ExecutedInstruction;
			}
			
			this.instructionCounts = new ulong[0x100];
			this.addressProfiles = new ulong[0x10000];
			this.addressCounts = new ulong[0x10000];

			this.addressScopes = new string[0x10000];
			this.scopeCycles = new Dictionary<string, ulong>();

			this.BuildAddressScopes();
		}

		public event EventHandler<EventArgs> StartingOutput;

		public event EventHandler<EventArgs> FinishedOutput;

		public event EventHandler<EventArgs> StartingLineOutput;

		public event EventHandler<EventArgs> FinishedLineOutput;

		public event EventHandler<ProfileLineEventArgs> EmitLine;

		public event EventHandler<EventArgs> StartingScopeOutput;

		public event EventHandler<EventArgs> FinishedScopeOutput;

		public event EventHandler<ProfileScopeEventArgs> EmitScope;

		public void Generate()
		{
			this.OnStartingOutput();
			try
			{
				this.EmitProfileInformation();
			}
			finally
			{
				this.OnFinishedOutput();
			}
		}

		private void EmitProfileInformation()
		{
			this.OnStartingLineOutput();
			try
			{
				// For each memory address
				for (var i = 0; i < 0x10000; ++i)
				{
					// If there are any cycles associated
					var cycles = this.addressProfiles[i];
					if (cycles > 0)
					{
						var address = (ushort)i;

						// Dump a profile/disassembly line
						var source = this.disassembler.Disassemble(address);
						this.OnEmitLine(source, cycles);
					}
				}
			}
			finally
			{
				this.OnFinishedLineOutput();
			}

			this.OnStartingScopeOutput();
			try
			{
				foreach (var scopeCycle in this.scopeCycles)
				{
					var name = scopeCycle.Key;
					var cycles = scopeCycle.Value;
					var count = this.addressCounts[this.symbols.Addresses[name]];
					this.OnEmitScope(name, cycles, count);
				}
			}
			finally
			{
				this.OnFinishedScopeOutput();
			}
		}

		private void Processor_ExecutingInstruction(object sender, Processor.AddressEventArgs e)
		{
			if (this.profileAddresses)
			{
				this.priorCycleCount = this.processor.Cycles;
				this.addressCounts[e.Address]++;
			}

			if (this.countInstructions)
			{
				++this.instructionCounts[e.Cell];
			}
		}

		private void Processor_ExecutedInstruction(object sender, Processor.AddressEventArgs e)
		{
			if (this.profileAddresses)
			{
				var address = e.Address;
				var cycles = this.processor.Cycles - this.priorCycleCount;

				this.addressProfiles[address] += cycles;

				var addressScope = this.addressScopes[address];
				if (addressScope != null)
				{
					if (!this.scopeCycles.ContainsKey(addressScope))
					{
						this.scopeCycles[addressScope] = 0;
					}

					this.scopeCycles[addressScope] += cycles;
				}
			}
		}

		private void BuildAddressScopes()
		{
			foreach (var label in this.symbols.Labels)
			{
				var address = label.Key;
				var key = label.Value;

				ushort scope;
				if (this.symbols.Scopes.TryGetValue(key, out scope))
				{
					for (ushort i = address; i < address + scope; ++i)
					{
						this.addressScopes[i] = key;
					}
				}
			}
		}

		private void OnStartingOutput()
		{
			this.StartingOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnFinishedOutput()
		{
			this.FinishedOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnStartingLineOutput()
		{
			this.StartingLineOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnFinishedLineOutput()
		{
			this.FinishedLineOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnStartingScopeOutput()
		{
			this.StartingScopeOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnFinishedScopeOutput()
		{
			this.FinishedScopeOutput?.Invoke(this, EventArgs.Empty);
		}

		private void OnEmitLine(string source, ulong cycles)
		{
			this.EmitLine?.Invoke(this, new ProfileLineEventArgs(source, cycles));
		}

		private void OnEmitScope(string scope, ulong cycles, ulong count)
		{
			this.EmitScope?.Invoke(this, new ProfileScopeEventArgs(scope, cycles, count));
		}
	}
}
