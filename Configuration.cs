namespace Simulator
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization.Json;
	using System.Xml.Linq;
	using System.Xml.XPath;

	public class Configuration
	{
		private ProcessorType processorLevel = ProcessorType.cpu6502;

		private ushort inputAddress;
		private ushort outputAddress;

		private string romPath;
		private ushort loadAddress;

		private string bbcLanguageRomPath;
		private string bbcOSRomPath;

		private string disassemblyLogPath;
		private string debugFile;

		private ushort startAddress = 0;

		private bool resetStart = false;

		private bool stopWhenLoopDetected = false;
		private bool stopBreak = false;
		private byte breakInstruction = 0x0;
		private ushort stopAddress = 0;
		private bool stopAddressEnabled = false;

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

					this.processorLevel = GetProcessorTypeValue(root, "//CPU/level");

					this.inputAddress = GetUShortValue(root, "//IO/inputAddress");
					this.outputAddress = GetUShortValue(root, "//IO/outputAddress");

					this.romPath = GetStringValue(root, "//ROM/path");
					this.loadAddress = GetUShortValue(root, "//ROM/loadAddress");
					this.startAddress = GetUShortValue(root, "//ROM/startAddress");

					this.bbcLanguageRomPath = GetStringValue(root, "//BBC/language/path");
					this.bbcOSRomPath = GetStringValue(root, "//BBC/OS/path");

					this.resetStart = GetBooleanValue(root, "//run/resetStart");
					this.stopBreak = GetBooleanValue(root, "//run/stopBreak");
					this.stopWhenLoopDetected = GetBooleanValue(root, "//run/stopWhenLoopDetected");
					this.stopAddress = GetUShortValue(root, "//run/stopAddress");
					this.stopAddressEnabled = this.stopAddress != 0;

#if DEBUG
					this.disassemble = GetBooleanValue(root, "//debug/disassemble");
					this.disassemblyLogPath = GetStringValue(root, "//debug/disassemblyLogPath");
					this.debugFile = GetStringValue(root, "//debug/debugFile");
					this.countInstructions = GetBooleanValue(root, "//debug/countInstructions");
					this.profileAddresses = GetBooleanValue(root, "//debug/profileAddresses");
#else
					this.disassemble = GetBooleanValue(root, "//release/disassemble");
					this.disassemblyLogPath = GetStringValue(root, "//release/disassemblyLogPath");
					this.debugFile = GetStringValue(root, "//release/debugFile");
					this.countInstructions = GetBooleanValue(root, "//release/countInstructions");
					this.profileAddresses = GetBooleanValue(root, "//release/profileAddresses");
#endif
				}
			}
		}

		public ProcessorType ProcessorLevel
		{
			get
			{
				return this.processorLevel;
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

		public string BbcLanguageRomPath
		{
			get
			{
				return this.bbcLanguageRomPath;
			}
		}

		public string BbcOSRomPath
		{
			get
			{
				return this.bbcOSRomPath;
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
		
		public ushort StopAddress
		{
			get
			{
				return this.stopAddress;
			}
		}

		public bool StopAddressEnabled
		{
			get
			{
				return this.stopAddressEnabled;
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

		public string DisassemblyLogPath
		{
			get
			{
				return this.disassemblyLogPath;
			}
		}
		public string DebugFile
		{
			get
			{
				return this.debugFile;
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

		private static ProcessorType GetProcessorTypeValue(XElement root, string path)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? ProcessorType.cpu6502 : (ProcessorType)Enum.Parse(typeof(ProcessorType), value);
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