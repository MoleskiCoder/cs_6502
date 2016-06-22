namespace Model
{
	using System;
	using Processor;

	public class Configuration : ConfigurationReader
	{
		private double hostSpeed;

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
		:	base(path)
		{
			this.hostSpeed = this.GetDoubleValue("//Host/speed", 2900.0);

			this.processorLevel = this.GetProcessorTypeValue("//CPU/level");
			this.speed = this.GetDoubleValue("//CPU/speed", 2.0);
			this.pollIntervalMilliseconds = this.GetIntValue("//CPU/pollIntervalMilliseconds", 10);

			this.inputAddress = this.GetUShortValue("//IO/inputAddress");
			this.outputAddress = this.GetUShortValue("//IO/outputAddress");

			this.romPath = this.GetStringValue("//ROM/path");
			this.romLoadAddress = this.GetUShortValue("//ROM/loadAddress");

			this.ramPath = this.GetStringValue("//RAM/path");
			this.ramLoadAddress = this.GetUShortValue("//RAM/loadAddress");

			this.bbcLanguageRomPath = this.GetStringValue("//BBC/language/path");
			this.bbcOSRomPath = this.GetStringValue("//BBC/OS/path");
			this.bbcVduEmulation = this.GetBooleanValue("//BBC/VDUEmulation");

			this.startAddress = this.GetUShortValue("//run/startAddress");
			this.resetStart = this.GetBooleanValue("//run/resetStart");
			this.stopBreak = this.GetBooleanValue("//run/stopBreak");
			this.breakInstruction = this.GetByteValue("//run/breakInstruction", 0x00);
			this.stopWhenLoopDetected = this.GetBooleanValue("//run/stopWhenLoopDetected");
			this.stopAddress = this.GetUShortValue("//run/stopAddress");
			this.stopAddressEnabled = this.stopAddress != 0;

#if DEBUG
			this.disassemble = this.GetBooleanValue("//debug/disassemble");
			this.disassemblyLogPath = this.GetStringValue("//debug/disassemblyLogPath");
			this.debugFile = this.GetStringValue("//debug/debugFile");
			this.countInstructions = this.GetBooleanValue("//debug/countInstructions");
			this.profileAddresses = this.GetBooleanValue("//debug/profileAddresses");
#else
			this.disassemble = this.GetBooleanValue("//release/disassemble");
			this.disassemblyLogPath = this.GetStringValue("//release/disassemblyLogPath");
			this.debugFile = this.GetStringValue("//release/debugFile");
			this.countInstructions = this.GetBooleanValue("//release/countInstructions");
			this.profileAddresses = this.GetBooleanValue("//release/profileAddresses");
#endif
		}

		public double HostSpeed
		{
			get
			{
				return this.hostSpeed;
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

		private ProcessorType GetProcessorTypeValue(string path, ProcessorType defaultValue)
		{
			var value = this.GetStringValue(path);
			return string.IsNullOrEmpty(value) ? defaultValue : (ProcessorType)Enum.Parse(typeof(ProcessorType), value);
		}

		private ProcessorType GetProcessorTypeValue(string path)
		{
			return this.GetProcessorTypeValue(path, ProcessorType.Cpu6502);
		}
	}
}