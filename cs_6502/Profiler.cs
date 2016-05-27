namespace Simulator
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	class Profiler
	{
		private readonly ulong[] instructionCounts;
		private readonly ulong[] addressProfiles;

		private readonly string[] addressScopes;

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

			this.addressScopes = new string[0x10000];

			this.BuildAddressScopes();
		}

		public event EventHandler<ProfileEventArgs> Profile;

		public void Generate()
		{
			var disassembler = this.disassembler;
			var profiles = this.addressProfiles;

			var scopeCycles = new Dictionary<string, ulong>();

			// For each memory address
			for (var i = 0; i < 0x10000; ++i)
			{
				// If there are any cycles associated
				var cycles = profiles[i];
				if (cycles > 0)
				{
					var addressScope = addressScopes[i];
					if (addressScope != null)
					{
						if (!scopeCycles.ContainsKey(addressScope))
						{
							scopeCycles[addressScope] = 0;
						}

						scopeCycles[addressScope] += cycles;
					}

					// Grab a label, if possible
					var address = (ushort)i;
					string label;
					if (this.symbols.Labels.TryGetValue(address, out label))
					{
						this.OnProfile(string.Format(CultureInfo.InvariantCulture, "{0}:\n", label));
					}

					// Dump a profile/disassembly line
					var source = disassembler.Disassemble(address);
					var proportion = (double)cycles / this.processor.Cycles;
					this.OnProfile(string.Format(CultureInfo.InvariantCulture, "\t[{0:P2}][{1:d9}]\t{2}\n", proportion, cycles, source));
				}
			}

			this.OnProfile("Cycles used by scope:\n");
			foreach (var scopeCycle in scopeCycles)
			{
				var name = scopeCycle.Key;
				var cycles = scopeCycle.Value;
				var proportion = (double)cycles / this.processor.Cycles;
				this.OnProfile(string.Format(CultureInfo.InvariantCulture, "\t[{0:P2}][{1:d9}]\t{2}\n", proportion, cycles, name));
			}
		}

		protected void OnProfile(string output)
		{
			this.Profile?.Invoke(this, new ProfileEventArgs(output));
		}

		private void Processor_ExecutingInstruction(object sender, Processor.AddressEventArgs e)
		{
			if (this.profileAddresses)
			{
				this.priorCycleCount = this.processor.Cycles;
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
				this.addressProfiles[e.Address] += this.processor.Cycles - this.priorCycleCount;
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
	}
}
