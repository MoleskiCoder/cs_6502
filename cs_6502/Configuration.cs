namespace Simulator
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization.Json;
	using System.Xml.Linq;
	using System.Xml.XPath;

	using Processor;

	public class Configuration
	{
		private ProcessorType processorLevel;
		private double speed;
		private int pollIntervalMilliseconds;

		private ushort inputAddress;
		private ushort outputAddress;

		private string romPath;
		private ushort romLoadAddress;

		private string ramPath;
		private ushort ramLoadAddress;

		private string bbcLanguageRomPath;
		private string bbcOSRomPath;
		private bool bbcVduEmulation;

		private string disassemblyLogPath;
		private string debugFile;

		private ushort startAddress;

		private bool resetStart;

		private bool stopWhenLoopDetected;
		private bool stopBreak;
		private byte breakInstruction;
		private ushort stopAddress;
		private bool stopAddressEnabled;

		private bool disassemble;
		private bool countInstructions;
		private bool profileAddresses;

		public Configuration(string path)
		{
			using (var input = File.Open(path, FileMode.Open))
			{
				using (var reader = JsonReaderWriterFactory.CreateJsonReader(input, new System.Xml.XmlDictionaryReaderQuotas()))
				{
					var root = XElement.Load(reader);

					this.processorLevel = GetProcessorTypeValue(root, "//CPU/level");
					this.speed = GetDoubleValue(root, "//CPU/speed", 2.0);
					this.pollIntervalMilliseconds = GetIntValue(root, "//CPU/pollIntervalMilliseconds", 10);

					this.inputAddress = GetUShortValue(root, "//IO/inputAddress");
					this.outputAddress = GetUShortValue(root, "//IO/outputAddress");

					this.romPath = GetStringValue(root, "//ROM/path");
					this.romLoadAddress = GetUShortValue(root, "//ROM/loadAddress");

					this.ramPath = GetStringValue(root, "//RAM/path");
					this.ramLoadAddress = GetUShortValue(root, "//RAM/loadAddress");

					this.bbcLanguageRomPath = GetStringValue(root, "//BBC/language/path");
					this.bbcOSRomPath = GetStringValue(root, "//BBC/OS/path");
					this.bbcVduEmulation = GetBooleanValue(root, "//BBC/VDUEmulation");

					this.startAddress = GetUShortValue(root, "//run/startAddress");
					this.resetStart = GetBooleanValue(root, "//run/resetStart");
					this.stopBreak = GetBooleanValue(root, "//run/stopBreak");
					this.breakInstruction = GetByteValue(root, "//run/breakInstruction", 0x00);
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

		public double Speed
		{
			get
			{
				return this.speed;
			}
		}

		public int PollIntervalMilliseconds
		{
			get
			{
				return this.pollIntervalMilliseconds;
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

		public bool BbcVduEmulation
		{
			get
			{
				return this.bbcVduEmulation;
			}
		}

		public string RomPath
		{
			get
			{
				return this.romPath;
			}
		}

		public ushort RomLoadAddress
		{
			get
			{
				return this.romLoadAddress;
			}
		}

		public string RamPath
		{
			get
			{
				return this.ramPath;
			}
		}

		public ushort RamLoadAddress
		{
			get
			{
				return this.ramLoadAddress;
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

		private static ProcessorType GetProcessorTypeValue(XElement root, string path, ProcessorType defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : (ProcessorType)Enum.Parse(typeof(ProcessorType), value);
		}

		private static ProcessorType GetProcessorTypeValue(XElement root, string path)
		{
			return GetProcessorTypeValue(root, path, ProcessorType.cpu6502);
		}

		private static bool GetBooleanValue(XElement root, string path, bool defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : bool.Parse(value);
		}

		private static bool GetBooleanValue(XElement root, string path)
		{
			return GetBooleanValue(root, path, false);
		}

		private static byte GetByteValue(XElement root, string path, byte defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static byte GetByteValue(XElement root, string path)
		{
			return GetByteValue(root, path, 0);
		}

		private static ushort GetUShortValue(XElement root, string path, ushort defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static ushort GetUShortValue(XElement root, string path)
		{
			return GetUShortValue(root, path, 0);
		}

		private static int GetIntValue(XElement root, string path, int defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		private static int GetIntValue(XElement root, string path)
		{
			return GetIntValue(root, path, 0);
		}

		private static double GetDoubleValue(XElement root, string path, double defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		private static double GetDoubleValue(XElement root, string path)
		{
			return GetDoubleValue(root, path, 0.0);
		}

		private static string GetStringValue(XElement root, string path, string defaultValue)
		{
			var element = root.XPathSelectElement(path);
			return element == null ? defaultValue : element.Value;
		}

		private static string GetStringValue(XElement root, string path)
		{
			return GetStringValue(root, path, string.Empty);
		}
	}
}