namespace Model
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;

	public class Symbols
	{
		private Dictionary<ushort, string> labels;
		private Dictionary<ushort, string> constants;
		private Dictionary<string, ushort> scopes;
		private Dictionary<string, ulong> addresses;

		private Dictionary<string, Dictionary<string, Dictionary<string, string>>> parsed;

		public Symbols(string path)
		{
			this.parsed = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
			this.labels = new Dictionary<ushort, string>();
			this.constants = new Dictionary<ushort, string>();
			this.scopes = new Dictionary<string, ushort>();
			this.addresses = new Dictionary<string, ulong>();

			if (!string.IsNullOrWhiteSpace(path))
			{
				this.Parse(path);
				this.AssignSymbols();
				this.AssignScopes();
			}
		}

		public Dictionary<ushort, string> Labels
		{
			get
			{
				return this.labels;
			}
		}

		public Dictionary<ushort, string> Constants
		{
			get
			{
				return this.constants;
			}
		}

		public Dictionary<string, ushort> Scopes
		{
			get
			{
				return this.scopes;
			}
		}

		public Dictionary<string, ulong> Addresses
		{
			get
			{
				return this.addresses;
			}
		}

		private void AssignScopes()
		{
			var parsedScopes = this.parsed["scope"];
			foreach (var parsedScope in parsedScopes.Values)
			{
				var name = parsedScope["name"].Trim(new char[] { '"' });
				var size = parsedScope["size"];
				this.scopes[name] = ushort.Parse(size, CultureInfo.InvariantCulture);
			}
		}

		private void AssignSymbols()
		{
			var symbols = this.parsed["sym"];
			foreach (var symbol in symbols.Values)
			{
				var name = symbol["name"].Trim(new char[] { '"' });
				var value = symbol["val"];
				var number = ushort.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				switch (symbol["type"])
				{
					case "lab":
						this.labels[number] = name;
						this.addresses[name] = number;
						break;

					case "equ":
						this.constants[number] = name;
						break;
				}
			}
		}

		private void Parse(string path)
		{
			using (var reader = new StreamReader(path))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var lineElements = line.Split();
					if (lineElements.Length == 2)
					{
						var type = lineElements[0];
						var dataElements = lineElements[1].Split(new char[] { ',' });
						var data = new Dictionary<string, string>();
						foreach (var dataElement in dataElements)
						{
							var definition = dataElement.Split(new char[] { '=' });
							if (definition.Length == 2)
							{
								data[definition[0]] = definition[1];
							}
						}

						if (data.ContainsKey("id"))
						{
							if (!this.parsed.ContainsKey(type))
							{
								this.parsed[type] = new Dictionary<string, Dictionary<string, string>>();
							}

							var id = data["id"];
							data.Remove("id");
							this.parsed[type][id] = data;
						}
					}
				}
			}
		}
	}
}
