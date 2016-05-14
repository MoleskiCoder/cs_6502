﻿namespace Simulator
{
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization.Json;
	using System.Xml.Linq;
	using System.Xml.XPath;

	public class Configuration
	{
		private ushort inputAddress;
		private ushort outputAddress;

		private string romPath;
		private ushort loadAddress;

		private ushort startAddress = 0;

		private bool resetStart = false;

		private bool stopWhenLoopDetected = false;
		private bool stopBreak = false;
		private byte breakInstruction = 0x0;

		private bool disassemble = false;
		private bool countInstructions = false;
		private bool profileAddresses = false;

		public Configuration(string path)
		{
			using (var input = File.Open(path, FileMode.Open))
			{
				using (var reader = JsonReaderWriterFactory.CreateJsonReader(input, new System.Xml.XmlDictionaryReaderQuotas()))
				{
					var root = XElement.Load(reader);

					this.inputAddress = GetUShortValue(root, "//IO/inputAddress");
					this.outputAddress = GetUShortValue(root, "//IO/outputAddress");

					this.romPath = GetStringValue(root, "//ROM/path");
					this.loadAddress = GetUShortValue(root, "//ROM/loadAddress");
					this.startAddress = GetUShortValue(root, "//ROM/startAddress");

					this.resetStart = GetBooleanValue(root, "//run/resetStart");
					this.stopBreak = GetBooleanValue(root, "//run/stopBreak");
					this.stopWhenLoopDetected = GetBooleanValue(root, "//run/stopWhenLoopDetected");

#if DEBUG
					this.disassemble = GetBooleanValue(root, "//debug/disassemble");
					this.countInstructions = GetBooleanValue(root, "//debug/countInstructions");
					this.profileAddresses = GetBooleanValue(root, "//debug/profileAddresses");
#else
					this.disassemble = GetBooleanValue(root, "//release/disassemble");
					this.countInstructions = GetBooleanValue(root, "//release/countInstructions");
					this.profileAddresses = GetBooleanValue(root, "//release/profileAddresses");
#endif
				}
			}
		}

		public ushort InputAddress
		{
			get
			{
				return this.inputAddress;
			}
		}

		public ushort OutputAddress
		{
			get
			{
				return this.outputAddress;
			}
		}

		public string RomPath
		{
			get
			{
				return this.romPath;
			}
		}

		public ushort LoadAddress
		{
			get
			{
				return this.loadAddress;
			}
		}

		public ushort StartAddress
		{
			get
			{
				return this.startAddress;
			}
		}

		public bool ResetStart
		{
			get
			{
				return this.resetStart;
			}
		}

		public bool StopBreak
		{
			get
			{
				return this.stopBreak;
			}
		}

		public bool StopWhenLoopDetected
		{
			get
			{
				return this.stopWhenLoopDetected;
			}
		}
		
		public byte BreakInstruction
		{
			get
			{
				return this.breakInstruction;
			}
		}

		public bool Disassemble
		{
			get
			{
				return this.disassemble;
			}
		}

		public bool CountInstructions
		{
			get
			{
				return this.countInstructions;
			}
		}

		public bool ProfileAddresses
		{
			get
			{
				return this.profileAddresses;
			}
		}

		private static bool GetBooleanValue(XElement root, string path)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? false : bool.Parse(value);
		}

		private static ushort GetUShortValue(XElement root, string path)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? (ushort)0 : ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static string GetStringValue(XElement root, string path)
		{
			var element = root.XPathSelectElement(path);
			return element == null ? string.Empty : element.Value;
		}
	}
}