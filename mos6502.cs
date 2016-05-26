namespace Simulator
{
	using System;
	using System.ComponentModel;

	public abstract class MOS6502 : INotifyPropertyChanged
	{
		private const ushort PageOne = 0x100;
		private const ushort IRQvector = 0xfffe;
		private const ushort RSTvector = 0xfffc;
		private const ushort NMIvector = 0xfffa;

		private ushort pc;  // program counter
		private byte x;     // index register X
		private byte y;     // index register Y
		private byte a;     // accumulator
		private byte s;     // stack pointer

		private StatusFlags p;     // processor status

		private ulong cycles;

		private ProcessorType level;

		private Instruction[] instructions;
		private Instruction[] overlay6502;
		private Instruction[] overlay65sc02;
		private Instruction[] overlay65c02;

		protected MOS6502(ProcessorType level)
		{
			this.level = level;

			this.instructions = new Instruction[0x100];
			this.Install6502Instructions();
			this.Install65sc02Instructions();
			this.Install65c02Instructions();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler<EventArgs> Starting;

		public event EventHandler<EventArgs> Finished;

		public ulong Cycles
		{
			get
			{
				return this.cycles;
			}

			private set
			{
				this.cycles = value;
			}
		}

		public ushort PC
		{
			get
			{
				return this.pc;
			}

			private set
			{
				this.pc = value;
			}
		}

		public Instruction[] Instructions
		{
			get
			{
				return this.instructions;
			}
		}

		protected byte X
		{
			get
			{
				return this.x;
			}

			private set
			{
				this.x = value;
			}
		}

		protected byte Y
		{
			get
			{
				return this.y;
			}

			private set
			{
				this.y = value;
			}
		}

		protected byte A
		{
			get
			{
				return this.a;
			}

			private set
			{
				this.a = value;
			}
		}

		protected byte S
		{
			get
			{
				return this.s;
			}

			private set
			{
				this.s = value;
			}
		}

		protected StatusFlags P
		{
			get
			{
				return this.p;
			}

			private set
			{
				this.p = value;
			}
		}

		public virtual void Start(ushort address)
		{
			this.PC = address;
		}

		public virtual void Run()
		{
			this.OnStarting();
			try
			{
				this.Cycles = 0;
				while (this.Step())
				{
				}
			}
			finally
			{
				this.OnFinished();
			}
		}

		public virtual void Reset()
		{
			this.PC = this.GetWord(RSTvector);
		}

		public virtual void TriggerIRQ()
		{
			this.Interrupt(IRQvector);
		}

		public virtual void TriggerNMI()
		{
			this.Interrupt(NMIvector);
		}

		public virtual ushort GetWord(ushort offset)
		{
			var low = this.GetByte(offset);
			var high = this.GetByte((ushort)(offset + 1));
			return MakeWord(low, high);
		}

		public abstract byte GetByte(ushort offset);

		public abstract void SetByte(ushort offset, byte value);

		protected virtual void OnStarting()
		{
			var handler = this.Starting;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		protected virtual void OnFinished()
		{
			var handler = this.Finished;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		protected virtual void Interrupt(ushort vector)
		{
			this.PushWord(this.PC);
			this.PushByte(this.P);
			this.P.Interrupt = true;
			this.PC = this.GetWord(vector);
		}

		protected virtual bool Execute(byte instruction)
		{
			return this.Execute(this.Instructions[instruction]);
		}

		protected virtual bool Execute(Instruction instruction)
		{
			var method = instruction.Vector;

			method();
			this.Cycles += instruction.Count;

			return true;
		}

		protected virtual void ___()
		{
			if (this.level >= ProcessorType.cpu65sc02)
			{
				// Generally, missing instructions act as a one byte,
				// one cycle NOP instruction on 65c02 (ish) processors.
				this.NOP_imp();
				this.Cycles++;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		protected void ResetRegisters()
		{
			this.PC = 0x0000;
			this.X = 0x80;
			this.Y = 0x00;
			this.A = 0x00;

			this.P = new StatusFlags(0);
			this.P.Reserved = true;

			this.S = 0xff;
		}

		protected virtual bool Step()
		{
			return this.Execute(this.FetchByte());
		}

		private static Instruction INS(Implementation method, ulong cycles, AddressingMode addressing, string display)
		{
			return new Instruction() { Vector = method, Count = cycles, Display = display, Mode = addressing };
		}

		private static byte LowNybble(byte value)
		{
			return (byte)(value & 0xf);
		}

		private static byte HighNybble(byte value)
		{
			return DemoteNybble(value);
		}

		private static byte PromoteNybble(byte value)
		{
			return (byte)(value << 4);
		}

		private static byte DemoteNybble(byte value)
		{
			return (byte)(value >> 4);
		}

		private static byte LowByte(ushort value)
		{
			return (byte)(value & 0xff);
		}

		private static byte HighByte(ushort value)
		{
			return (byte)((value & ~0xff) >> 8);
		}

		private static ushort MakeWord(byte low, byte high)
		{
			return (ushort)((high << 8) + low);
		}

		////

		private void Install6502Instructions()
		{
			this.overlay6502 = new Instruction[]
			{
				////	0 														1														2														3													4														5														6														7													8															9															A														B													C														D															E															F
				/* 0 */	INS(this.BRK_imp, 7, AddressingMode.Implied, "BRK"),	INS(this.ORA_xind, 6, AddressingMode.XIndexed, "ORA"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_zp, 4, AddressingMode.ZeroPage, "ORA"),	INS(this.ASL_zp, 5, AddressingMode.ZeroPage, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PHP_imp, 3, AddressingMode.Implied, "PHP"),		INS(this.ORA_imm, 2, AddressingMode.Immediate, "ORA"),		INS(this.ASL_a, 2, AddressingMode.Accumulator, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_abs, 4, AddressingMode.Absolute, "ORA"),		INS(this.ASL_abs, 6, AddressingMode.Absolute, "ASL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 1 */	INS(this.BPL_rel, 2, AddressingMode.Relative, "BPL"),   INS(this.ORA_indy, 5, AddressingMode.IndexedY, "ORA"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_zpx, 4, AddressingMode.ZeroPageX, "ORA"),	INS(this.ASL_zpx, 6, AddressingMode.ZeroPageX, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CLC_imp, 2, AddressingMode.Implied, "CLC"),		INS(this.ORA_absy, 4, AddressingMode.AbsoluteY, "ORA"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_absx, 4, AddressingMode.AbsoluteX, "ORA"),		INS(this.ASL_absx, 7, AddressingMode.AbsoluteX, "ASL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 2 */	INS(this.JSR_abs, 6, AddressingMode.Absolute, "JSR"),   INS(this.AND_xind, 6, AddressingMode.XIndexed, "AND"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.BIT_zp, 3, AddressingMode.ZeroPage, "BIT"),	INS(this.AND_zp, 3, AddressingMode.ZeroPage, "AND"),	INS(this.ROL_zp, 5, AddressingMode.ZeroPage, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PLP_imp, 4, AddressingMode.Implied, "PLP"),		INS(this.AND_imm, 2, AddressingMode.Immediate, "AND"),		INS(this.ROL_a, 2, AddressingMode.Accumulator, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.BIT_abs, 4, AddressingMode.Absolute, "BIT"),	INS(this.AND_abs, 4, AddressingMode.Absolute, "AND"),		INS(this.ROL_abs, 6, AddressingMode.Absolute, "ROL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 3 */	INS(this.BMI_rel, 2, AddressingMode.Relative, "BMI"),   INS(this.AND_indy, 5, AddressingMode.IndexedY, "AND"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.AND_zpx, 4, AddressingMode.ZeroPageX, "AND"),	INS(this.ROL_zpx, 6, AddressingMode.ZeroPageX, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.SEC_imp, 2, AddressingMode.Implied, "SEC"),		INS(this.AND_absy, 4, AddressingMode.AbsoluteY, "AND"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.AND_absx, 4, AddressingMode.AbsoluteX, "AND"),		INS(this.ROL_absx, 7, AddressingMode.AbsoluteX, "ROL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 4 */	INS(this.RTI_imp, 6, AddressingMode.Implied, "RTI"),	INS(this.EOR_xind, 6, AddressingMode.XIndexed, "EOR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_zp, 3, AddressingMode.ZeroPage, "EOR"),	INS(this.LSR_zp, 5, AddressingMode.ZeroPage, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PHA_imp, 3, AddressingMode.Implied, "PHA"),		INS(this.EOR_imm, 2, AddressingMode.Immediate, "EOR"),		INS(this.LSR_a, 2, AddressingMode.Accumulator, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.JMP_abs, 3, AddressingMode.Absolute, "JMP"),	INS(this.EOR_abs, 4, AddressingMode.Absolute, "EOR"),		INS(this.LSR_abs, 6, AddressingMode.Absolute, "LSR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 5 */	INS(this.BVC_rel, 2, AddressingMode.Relative, "BVC"),   INS(this.EOR_indy, 5, AddressingMode.IndexedY, "EOR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_zpx, 4, AddressingMode.ZeroPageX, "EOR"),	INS(this.LSR_zpx, 6, AddressingMode.ZeroPageX, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.CLI_imp, 2, AddressingMode.Implied, "CLI"),		INS(this.EOR_absy, 4, AddressingMode.AbsoluteY, "EOR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_absx, 4, AddressingMode.AbsoluteX, "EOR"),		INS(this.LSR_absx, 7, AddressingMode.AbsoluteX, "LSR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 6 */	INS(this.RTS_imp, 6, AddressingMode.Implied, "RTS"),	INS(this.ADC_xind, 6, AddressingMode.XIndexed, "ADC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ADC_zp, 3, AddressingMode.ZeroPage, "ADC"),	INS(this.ROR_zp, 5, AddressingMode.ZeroPage, "ROR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.PLA_imp, 4, AddressingMode.Implied, "PLA"),		INS(this.ADC_imm, 2, AddressingMode.Immediate, "ADC"),		INS(this.ROR_a, 2, AddressingMode.Accumulator, "ROR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.JMP_ind, 5, AddressingMode.Indirect, "JMP"),	INS(this.ADC_abs, 4, AddressingMode.Absolute, "ADC"),		INS(this.ROR_abs, 6, AddressingMode.Absolute, "ROR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 7 */	INS(this.BVS_rel, 2, AddressingMode.Relative, "BVS"),	INS(this.ADC_indy, 5, AddressingMode.IndexedY, "ADC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ADC_zpx, 4, AddressingMode.ZeroPageX, "ADC"),	INS(this.ROR_zpx, 6, AddressingMode.ZeroPageX, "ROR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SEI_imp, 2, AddressingMode.Implied, "SEI"),		INS(this.ADC_absy, 4, AddressingMode.AbsoluteY, "ADC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ADC_absx, 4, AddressingMode.AbsoluteX, "ADC"),		INS(this.ROR_absx, 7, AddressingMode.AbsoluteX, "ROR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 8 */	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.STA_xind, 6, AddressingMode.XIndexed, "STA"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.STY_zp, 3, AddressingMode.ZeroPage, "STY"),	INS(this.STA_zp, 3, AddressingMode.ZeroPage, "STA"),	INS(this.STX_zp, 3, AddressingMode.ZeroPage, "STX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.DEY_imp, 2, AddressingMode.Implied, "DEY"),		INS(this.___, 0, AddressingMode.Illegal, "___"),			INS(this.TXA_imp, 2, AddressingMode.Implied, "TXA"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.STY_abs, 4, AddressingMode.Absolute, "STY"),	INS(this.STA_abs, 4, AddressingMode.Absolute, "STA"),		INS(this.STX_abs, 4, AddressingMode.Absolute, "STX"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 9 */	INS(this.BCC_rel, 2, AddressingMode.Relative, "BCC"),   INS(this.STA_indy, 6, AddressingMode.IndexedY, "STA"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.STY_zpx, 4, AddressingMode.ZeroPageX, "STY"),	INS(this.STA_zpx, 4, AddressingMode.ZeroPageX, "STA"),	INS(this.STX_zpy, 4, AddressingMode.ZeroPageY, "STX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TYA_imp, 2, AddressingMode.Implied, "TYA"),		INS(this.STA_absy, 5, AddressingMode.AbsoluteY, "STA"),		INS(this.TXS_imp, 2, AddressingMode.Implied, "TXS"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.STA_absx, 5, AddressingMode.AbsoluteX, "STA"),		INS(this.___, 0, AddressingMode.Illegal, "___"),			INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* A */	INS(this.LDY_imm, 2, AddressingMode.Immediate, "LDY"),  INS(this.LDA_xind, 6, AddressingMode.XIndexed, "LDA"),	INS(this.LDX_imm, 2, AddressingMode.Immediate, "LDX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.LDY_zp, 3, AddressingMode.ZeroPage, "LDY"),	INS(this.LDA_zp, 3, AddressingMode.ZeroPage, "LDA"),	INS(this.LDX_zp, 3, AddressingMode.ZeroPage, "LDX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TAY_imp, 2, AddressingMode.Implied, "TAY"),		INS(this.LDA_imm, 2, AddressingMode.Immediate, "LDA"),		INS(this.TAX_imp, 2, AddressingMode.Implied, "TAX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.LDY_abs, 4, AddressingMode.Absolute, "LDY"),	INS(this.LDA_abs, 4, AddressingMode.Absolute, "LDA"),		INS(this.LDX_abs, 4, AddressingMode.Absolute, "LDX"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* B */	INS(this.BCS_rel, 2, AddressingMode.Relative, "BCS"),	INS(this.LDA_indy, 5, AddressingMode.IndexedY, "LDA"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.LDY_zpx, 4, AddressingMode.ZeroPageX, "LDY"),	INS(this.LDA_zpx, 4, AddressingMode.ZeroPageX, "LDA"),	INS(this.LDX_zpy, 4, AddressingMode.ZeroPageY, "LDX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.CLV_imp, 2, AddressingMode.Implied, "CLV"),		INS(this.LDA_absy, 4, AddressingMode.AbsoluteY, "LDA"),		INS(this.TSX_imp, 2, AddressingMode.Implied, "TSX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.LDY_absx, 4, AddressingMode.AbsoluteX, "LDY"),	INS(this.LDA_absx, 4, AddressingMode.AbsoluteX, "LDA"),		INS(this.LDX_absy, 4, AddressingMode.AbsoluteY, "LDX"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* C */	INS(this.CPY_imm, 2, AddressingMode.Immediate, "CPY"),	INS(this.CMP_xind, 6, AddressingMode.XIndexed, "CMP"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CPY_zp, 3, AddressingMode.ZeroPage, "CPY"),	INS(this.CMP_zp, 3, AddressingMode.ZeroPage, "CMP"),	INS(this.DEC_zp, 5, AddressingMode.ZeroPage, "DEC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.INY_imp, 2, AddressingMode.Implied, "INY"),		INS(this.CMP_imm, 2, AddressingMode.Immediate, "CMP"),		INS(this.DEX_imp, 2, AddressingMode.Implied, "DEX"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CPY_abs, 4, AddressingMode.Absolute, "CPY"),	INS(this.CMP_abs, 4, AddressingMode.Absolute, "CMP"),		INS(this.DEC_abs, 6, AddressingMode.Absolute, "DEC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* D */	INS(this.BNE_rel, 2, AddressingMode.Relative, "BNE"),	INS(this.CMP_indy, 5, AddressingMode.IndexedY, "CMP"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.CMP_zpx, 4, AddressingMode.ZeroPageX, "CMP"),	INS(this.DEC_zpx, 6, AddressingMode.ZeroPageX, "DEC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.CLD_imp, 2, AddressingMode.Implied, "CLD"),		INS(this.CMP_absy, 4, AddressingMode.AbsoluteY, "CMP"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.CMP_absx, 4, AddressingMode.AbsoluteX, "CMP"),		INS(this.DEC_absx, 7, AddressingMode.AbsoluteX, "DEC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* E */	INS(this.CPX_imm, 2, AddressingMode.Immediate, "CPX"),	INS(this.SBC_xind, 6, AddressingMode.XIndexed, "SBC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CPX_zp, 3, AddressingMode.ZeroPage, "CPX"),	INS(this.SBC_zp, 3, AddressingMode.ZeroPage, "SBC"),	INS(this.INC_zp, 5, AddressingMode.ZeroPage, "INC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.INX_imp, 2, AddressingMode.Implied, "INX"),		INS(this.SBC_imm, 2, AddressingMode.Immediate, "SBC"),		INS(this.NOP_imp, 2, AddressingMode.Implied, "NOP"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CPX_abs, 4, AddressingMode.Absolute, "CPX"),	INS(this.SBC_abs, 4, AddressingMode.Absolute, "SBC"),		INS(this.INC_abs, 6, AddressingMode.Absolute, "INC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* F */	INS(this.BEQ_rel, 2, AddressingMode.Relative, "BEQ"),	INS(this.SBC_indy, 5, AddressingMode.IndexedY, "SBC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.SBC_zpx, 4, AddressingMode.ZeroPageX, "SBC"),	INS(this.INC_zpx, 6, AddressingMode.ZeroPageX, "INC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SED_imp, 2, AddressingMode.Implied, "SED"),		INS(this.SBC_absy, 4, AddressingMode.AbsoluteY, "SBC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.SBC_absx, 4, AddressingMode.AbsoluteX, "SBC"),		INS(this.INC_absx, 7, AddressingMode.AbsoluteX, "INC"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
			};

			this.InstallInstructionSet(this.overlay6502);
		}

		private void Install65sc02Instructions()
		{
			if (this.level >= ProcessorType.cpu65sc02)
			{
				this.overlay65sc02 = new Instruction[]
				{
					////	0 														1														2														        3													4														5														6														7													8														9															A														B													C														            D															E															F
					/* 0 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TSB_zp, 5, AddressingMode.ZeroPage, "TSB"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TSB_abs, 6, AddressingMode.Absolute, "TSB"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 1 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.ORA_zpind, 5, AddressingMode.ZeroPageIndirect, "ORA"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TRB_zp, 5, AddressingMode.ZeroPage, "TRB"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.INC_a, 2, AddressingMode.Accumulator, "INC"),  INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.TRB_abs, 6, AddressingMode.Absolute, "TRB"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 2 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 3 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.AND_zpind, 5, AddressingMode.ZeroPageIndirect, "AND"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.BIT_zpx, 4, AddressingMode.ZeroPageX, "BIT"),  INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.DEC_a, 2, AddressingMode.Accumulator, "DEC"),  INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.BIT_absx, 4, AddressingMode.AbsoluteX, "BIT"),             INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 4 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP2_imp, 3, AddressingMode.Implied, "___"),   INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 5 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.EOR_zpind, 5, AddressingMode.ZeroPageIndirect, "EOR"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP2_imp, 4, AddressingMode.Implied, "___"),   INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.PHY_imp, 2, AddressingMode.Implied, "PHY"),    INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP3_imp, 8, AddressingMode.Implied, "___"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 6 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.STZ_zp, 3, AddressingMode.ZeroPage, "STZ"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 7 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.ADC_zpind, 5, AddressingMode.ZeroPageIndirect, "ADC"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.STZ_zpx, 4, AddressingMode.ZeroPageX, "STZ"),  INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.PLY_imp, 2, AddressingMode.Implied, "PLY"),    INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.JMP_absxind, 6, AddressingMode.AbsoluteXIndirect, "JMP"),  INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 8 */	INS(this.BRA_rel, 2, AddressingMode.Relative, "BRA"),   INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.BIT_imm, 2, AddressingMode.Immediate, "BIT"),      INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* 9 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.STA_zpind, 5, AddressingMode.ZeroPageIndirect, "STA"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.STZ_abs, 4, AddressingMode.Absolute, "STZ"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.STZ_absx, 2, AddressingMode.AbsoluteX, "STZ"),     INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* A */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* B */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.LDA_zpind, 5, AddressingMode.ZeroPageIndirect, "LDA"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* C */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* D */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.CMP_zpind, 5, AddressingMode.ZeroPageIndirect, "CMP"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP2_imp, 4, AddressingMode.Implied, "___"),   INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.PHX_imp, 2, AddressingMode.Implied, "PHX"),    INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP3_imp, 4, AddressingMode.Implied, "___"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* E */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.NOP2_imp, 2, AddressingMode.Implied, "___"),           INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),                    INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
					/* F */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.SBC_zpind, 5, AddressingMode.ZeroPageIndirect, "SBC"), INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP2_imp, 4, AddressingMode.Implied, "___"),   INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.PLX_imp, 2, AddressingMode.Implied, "PLX"),    INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.NOP3_imp, 4, AddressingMode.Implied, "___"),               INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),
				};

				this.OverlayInstructionSet(this.overlay65sc02);
			}
		}

		private void Install65c02Instructions()
		{
			if (this.level >= ProcessorType.cpu65c02)
			{
				this.overlay65c02 = new Instruction[]
				{
					////	0 														1														2														        3													4														5														6													7                                                           8														9															A														B													    C														D															E															F
					/* 0 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB0_zp, 5, AddressingMode.ZeroPage, "RMB0"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR0_zprel, 5, AddressingMode.ZeroPageRelative, "BBR0"),
					/* 1 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB1_zp, 5, AddressingMode.ZeroPage, "RMB1"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR1_zprel, 5, AddressingMode.ZeroPageRelative, "BBR1"),
					/* 2 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB2_zp, 5, AddressingMode.ZeroPage, "RMB2"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR2_zprel, 5, AddressingMode.ZeroPageRelative, "BBR2"),
					/* 3 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB3_zp, 5, AddressingMode.ZeroPage, "RMB3"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR3_zprel, 5, AddressingMode.ZeroPageRelative, "BBR3"),
					/* 4 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB4_zp, 5, AddressingMode.ZeroPage, "RMB4"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR4_zprel, 5, AddressingMode.ZeroPageRelative, "BBR4"),
					/* 5 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB5_zp, 5, AddressingMode.ZeroPage, "RMB5"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR5_zprel, 5, AddressingMode.ZeroPageRelative, "BBR5"),
					/* 6 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB6_zp, 5, AddressingMode.ZeroPage, "RMB6"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR6_zprel, 5, AddressingMode.ZeroPageRelative, "BBR6"),
					/* 7 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.RMB7_zp, 5, AddressingMode.ZeroPage, "RMB7"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBR7_zprel, 5, AddressingMode.ZeroPageRelative, "BBR7"),
					/* 8 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB0_zp, 5, AddressingMode.ZeroPage, "SMB0"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS0_zprel, 5, AddressingMode.ZeroPageRelative, "BBS0"),
					/* 9 */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB1_zp, 5, AddressingMode.ZeroPage, "SMB1"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),            INS(this.BBS1_zprel, 5, AddressingMode.ZeroPageRelative, "BBS1"),
					/* A */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB2_zp, 5, AddressingMode.ZeroPage, "SMB2"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS2_zprel, 5, AddressingMode.ZeroPageRelative, "BBS2"),
					/* B */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB3_zp, 5, AddressingMode.ZeroPage, "SMB3"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS3_zprel, 5, AddressingMode.ZeroPageRelative, "BBS3"),
					/* C */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB4_zp, 5, AddressingMode.ZeroPage, "SMB4"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.WAI_imp, 3, AddressingMode.Implied, "WAI"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS4_zprel, 5, AddressingMode.ZeroPageRelative, "BBS4"),
					/* D */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB5_zp, 5, AddressingMode.ZeroPage, "SMB5"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.STP_imp, 3, AddressingMode.Implied, "STP"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS5_zprel, 5, AddressingMode.ZeroPageRelative, "BBS5"),
					/* E */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB6_zp, 5, AddressingMode.ZeroPage, "SMB6"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS6_zprel, 5, AddressingMode.ZeroPageRelative, "BBS6"),
					/* F */	INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),                INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.SMB7_zp, 5, AddressingMode.ZeroPage, "SMB7"),      INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 0, AddressingMode.Illegal, "___"),        INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.___, 2, AddressingMode.Illegal, "___"),            INS(this.BBS7_zprel, 5, AddressingMode.ZeroPageRelative, "BBS7"),
				};

				this.OverlayInstructionSet(this.overlay65c02);
			}
		}
			
		private void InstallInstructionSet(Instruction[] basis)
		{
			this.OverlayInstructionSet(basis, true);
		}

		private void OverlayInstructionSet(Instruction[] overlay)
		{
			this.OverlayInstructionSet(overlay, false);
		}

		private void OverlayInstructionSet(Instruction[] overlay, bool includeIllegal)
		{
			for (uint i = 0; i < 0x100; ++i)
			{
				var newInstruction = overlay[i];
				var illegal = newInstruction.Mode == AddressingMode.Illegal;
				if (includeIllegal || !illegal)
				{
					var oldInstruction = this.Instructions[i];
					if (oldInstruction.Mode != AddressingMode.Illegal)
					{
						throw new InvalidOperationException("Whoops: replacing a non-missing instruction.");
					}

					this.Instructions[i] = newInstruction;
				}
			}
		}

		////

		private void PushByte(byte value)
		{
			this.SetByte((ushort)(PageOne + this.S--), value);
		}

		private byte PopByte()
		{
			return this.GetByte((ushort)(PageOne + ++this.S));
		}

		private void PushWord(ushort value)
		{
			this.PushByte(HighByte(value));
			this.PushByte(LowByte(value));
		}

		private ushort PopWord()
		{
			var low = this.PopByte();
			var high = this.PopByte();
			return MakeWord(low, high);
		}

		private byte FetchByte(ref ushort counter)
		{
			return this.GetByte(counter++);
		}

		private ushort FetchWord(ref ushort counter)
		{
			var word = this.GetWord(counter);
			counter += 2;
			return word;
		}

		private byte FetchByte()
		{
			return this.FetchByte(ref this.pc);
		}

		private ushort FetchWord()
		{
			return this.FetchWord(ref this.pc);
		}

		////

		private ushort Address_ZeroPage()
		{
			return (ushort)this.FetchByte();
		}

		private ushort Address_ZeroPageX()
		{
			return LowByte((ushort)(this.FetchByte() + this.X));
		}

		private ushort Address_ZeroPageY()
		{
			return LowByte((ushort)(this.FetchByte() + this.Y));
		}

		private ushort Address_IndexedIndirectX()
		{
			return this.GetWord(this.Address_ZeroPageX());
		}

		private ushort Address_IndexedIndirectY_Read()
		{
			var indirection = this.GetWord(this.FetchByte());
			if (LowByte(indirection) == 0xff)
			{
				++this.Cycles;
			}

			return (ushort)(indirection + this.Y);
		}

		private ushort Address_IndexedIndirectY_Write()
		{
			return (ushort)(this.GetWord(this.FetchByte()) + this.Y);
		}

		private ushort Address_Absolute()
		{
			return this.FetchWord();
		}

		private ushort Address_AbsoluteXIndirect()
		{
			return this.GetWord((ushort)(this.FetchWord() + this.X));
		}

		private ushort Address_AbsoluteX_Read()
		{
			var address = this.FetchWord();
			var offset = (ushort)(address + this.X);
			if (LowByte(offset) == 0xff)
			{
				++this.Cycles;
			}

			return offset;
		}

		private ushort Address_AbsoluteX_Write()
		{
			return (ushort)(this.FetchWord() + this.X);
		}

		private ushort Address_AbsoluteY_Read()
		{
			var address = this.FetchWord();
			var offset = (ushort)(address + this.Y);
			if (LowByte(offset) == 0xff)
			{
				++this.Cycles;
			}

			return offset;
		}

		private ushort Address_AbsoluteY_Write()
		{
			return (ushort)(this.FetchWord() + this.Y);
		}

		private ushort Address_ZeroPageIndirect()
		{
			return this.GetWord((ushort)this.FetchByte());
		}

		////

		private byte ReadByte_Immediate()
		{
			return this.FetchByte();
		}

		private sbyte ReadByte_ImmediateDisplacement()
		{
			return (sbyte)this.FetchByte();
		}

		private byte ReadByte_ZeroPage()
		{
			return this.GetByte(this.Address_ZeroPage());
		}

		private byte ReadByte_ZeroPageX()
		{
			return this.GetByte(this.Address_ZeroPageX());
		}

		private byte ReadByte_ZeroPageY()
		{
			return this.GetByte(this.Address_ZeroPageY());
		}

		private byte ReadByte_Absolute()
		{
			return this.GetByte(this.Address_Absolute());
		}

		private byte ReadByte_AbsoluteX()
		{
			return this.GetByte(this.Address_AbsoluteX_Read());
		}

		private byte ReadByte_AbsoluteY()
		{
			return this.GetByte(this.Address_AbsoluteY_Read());
		}

		private byte ReadByte_IndexedIndirectX()
		{
			return this.GetByte(this.Address_IndexedIndirectX());
		}

		private byte ReadByte_IndirectIndexedY()
		{
			return this.GetByte(this.Address_IndexedIndirectY_Read());
		}

		private byte ReadByte_ZeroPageIndirect()
		{
			return this.GetByte(this.Address_ZeroPageIndirect());
		}

		////

		private void WriteByte_ZeroPage(byte value)
		{
			this.SetByte(this.Address_ZeroPage(), value);
		}

		private void WriteByte_Absolute(byte value)
		{
			this.SetByte(this.Address_Absolute(), value);
		}

		private void WriteByte_AbsoluteX(byte value)
		{
			this.SetByte(this.Address_AbsoluteX_Write(), value);
		}

		private void WriteByte_AbsoluteY(byte value)
		{
			this.SetByte(this.Address_AbsoluteY_Write(), value);
		}

		private void WriteByte_ZeroPageX(byte value)
		{
			this.SetByte(this.Address_ZeroPageX(), value);
		}

		private void WriteByte_ZeroPageY(byte value)
		{
			this.SetByte(this.Address_ZeroPageY(), value);
		}

		private void WriteByte_IndirectIndexedY(byte value)
		{
			this.SetByte(this.Address_IndexedIndirectY_Write(), value);
		}

		private void WriteByte_IndexedIndirectX(byte value)
		{
			this.SetByte(this.Address_IndexedIndirectX(), value);
		}

		private void WriteByte_ZeroPageIndirect(byte value)
		{
			this.SetByte(this.Address_ZeroPageIndirect(), value);
		}

		////

		private void DEC(ushort offset)
		{
			var content = (sbyte)this.GetByte(offset);
			this.SetByte(offset, (byte)(--content));
			this.P.Zero = (content == 0);
			this.P.Negative = (content < 0);
		}

		private byte ROR(byte data)
		{
			var carry = this.P.Carry;

			this.P.Carry = ((data & 1) != 0);

			var result = (byte)(data >> 1);
			if (carry)
			{
				result |= 0x80;
			}

			this.P.Zero = (result == 0);
			this.P.Negative = ((sbyte)result < 0);

			return result;
		}

		private void ROR(ushort offset)
		{
			this.SetByte(offset, this.ROR(this.GetByte(offset)));
		}

		private byte LSR(byte data)
		{
			this.P.Carry = ((data & 1) != 0);

			var result = (byte)(data >> 1);

			this.P.Zero = (result == 0);
			this.P.Negative = ((sbyte)result < 0);

			return result;
		}

		private void LSR(ushort offset)
		{
			this.SetByte(offset, this.LSR(this.GetByte(offset)));
		}

		private void BIT_immediate(byte data)
		{
			var result = (byte)(this.A & data);
			this.P.Zero = (result == 0);
		}

		private void BIT(byte data)
		{
			this.BIT_immediate(data);
			this.P.Negative = ((data & 0x80) != 0);
			this.P.Overflow = ((data & 0x40) != 0);
		}

		private void TSB(ushort address)
		{
			var content = this.GetByte(address);
			this.BIT_immediate(content);

			var result = (byte)(content | this.A);
			this.SetByte(address, result);
		}

		private void TRB(ushort address)
		{
			var content = this.GetByte(address);
			this.BIT_immediate(content);

			var result = (byte)(content & ~this.A);
			this.SetByte(address, result);
		}

		private void INC(ushort offset)
		{
			var content = this.GetByte(offset);
			this.SetByte(offset, ++content);
			this.P.Zero = (content == 0);
			this.P.Negative = ((sbyte)content < 0);
		}

		private void ROL(ushort offset)
		{
			this.SetByte(offset, this.ROL(this.GetByte(offset)));
		}

		private byte ROL(byte data)
		{
			var carry = this.P.Carry;

			this.P.Carry = ((data & 0x80) != 0);

			var result = (byte)(data << 1);

			if (carry)
			{
				result |= 0x01;
			}

			this.P.Zero = (result == 0);
			this.P.Negative = ((sbyte)result < 0);

			return result;
		}

		private void ASL(ushort offset)
		{
			this.SetByte(offset, this.ASL(this.GetByte(offset)));
		}

		private byte ASL(byte data)
		{
			var result = (byte)(data << 1);

			this.P.Zero = (result == 0);
			this.P.Negative = ((sbyte)result < 0);
			this.P.Carry = ((data & 0x80) != 0);

			return result;
		}

		private void ORA(byte data)
		{
			this.A |= data;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void AND(byte data)
		{
			this.A &= data;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void SBC(byte data)
		{
			if (this.P.Decimal)
			{
				this.SBC_d(data);
			}
			else
			{
				this.SBC_b(data);
			}
		}

		private void SBC_b(byte data)
		{
			var carry = (byte)(!this.P.Carry ? 1 : 0);
			var difference = (ushort)(this.A - data - carry);

			this.P.Zero = ((byte)difference == 0);
			this.P.Negative = ((sbyte)difference < 0);
			this.P.Overflow = (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0);
			this.P.Carry = (HighByte(difference) == 0);

			this.A = (byte)difference;
		}

		private void SBC_d(byte data)
		{
			var carry = (byte)(!this.P.Carry ? 1 : 0);
			var difference = (ushort)(this.A - data - carry);

			if (this.level < ProcessorType.cpu65sc02)
			{
				this.P.Zero = ((byte)difference == 0);
				this.P.Negative = ((sbyte)difference < 0);
			}

			this.P.Overflow = (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0);
			this.P.Carry = (HighByte(difference) == 0);

			var low = (byte)(LowNybble(this.A) - LowNybble(data) - carry);

			var lowNegative = (sbyte)low < 0;
			if (lowNegative)
			{
				low -= 6;
			}

			var high = (byte)(HighNybble(this.A) - HighNybble(data) - (lowNegative ? 1 : 0));

			if ((sbyte)high < 0)
			{
				high -= 6;
			}

			this.A = (byte)(PromoteNybble(high) | LowNybble(low));
			if (this.level >= ProcessorType.cpu65sc02)
			{
				this.P.Zero = (this.A == 0);
				this.P.Negative = ((sbyte)this.A < 0);
			}
		}

		private void EOR(byte data)
		{
			this.A ^= data;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void CPX(byte data)
		{
			this.CMP(this.X, data);
		}

		private void CPY(byte data)
		{
			this.CMP(this.Y, data);
		}

		private void CMP(byte data)
		{
			this.CMP(this.A, data);
		}

		private void CMP(byte first, byte second)
		{
			var result = (ushort)(first - second);

			this.P.Zero = ((byte)result == 0);
			this.P.Negative = ((sbyte)result < 0);
			this.P.Carry = (HighByte(result) == 0);
		}

		private void LDA(byte data)
		{
			this.A = data;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void LDY(byte data)
		{
			this.Y = data;
			this.P.Zero = (this.Y == 0);
			this.P.Negative = ((sbyte)this.Y < 0);
		}

		private void LDX(byte data)
		{
			this.X = data;
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void ADC(byte data)
		{
			if (this.P.Decimal)
			{
				this.ADC_d(data);
			}
			else
			{
				this.ADC_b(data);
			}
		}

		private void ADC_b(byte data)
		{
			var carry = (byte)(this.P.Carry ? 1 : 0);
			var sum = (ushort)(this.A + data + carry);

			this.P.Zero = ((byte)sum == 0);
			this.P.Negative = ((sbyte)sum < 0);
			this.P.Overflow = ((~(this.A ^ data) & (this.A ^ sum) & 0x80) != 0);
			this.P.Carry = (HighByte(sum) != 0);

			this.A = (byte)sum;
		}

		private void ADC_d(byte data)
		{
			var carry = (byte)(this.P.Carry ? 1 : 0);
			var sum = (ushort)(this.A + data + carry);

			if (this.level < ProcessorType.cpu65sc02)
			{
				this.P.Zero = ((byte)sum == 0);
				this.P.Negative = ((sbyte)sum < 0);
			}

			var low = (byte)(LowNybble(this.A) + LowNybble(data) + carry);
			if (low > 9)
			{
				low += 6;
			}

			var high = (byte)(HighNybble(this.A) + HighNybble(data) + (low > 0x0f ? 1 : 0));
			this.P.Overflow = ((~(this.A ^ data) & (this.A ^ PromoteNybble(high)) & 0x80) != 0);

			if (high > 9)
			{
				high += 6;
			}

			this.P.Carry = (high > 0x0f);

			this.A = (byte)(PromoteNybble(high) | LowNybble(low));
			if (this.level >= ProcessorType.cpu65sc02)
			{
				this.P.Zero = (this.A == 0);
				this.P.Negative = ((sbyte)this.A < 0);
			}
		}

		////

		private void RMB(ushort address, byte flag)
		{
			var data = this.GetByte(address);
			data &= (byte)~flag;
			this.SetByte(address, data);
		}

		private void SMB(ushort address, byte flag)
		{
			var data = this.GetByte(address);
			data |= flag;
			this.SetByte(address, data);
		}

		////

		#region Branch

		private void Branch(sbyte displacement)
		{
			++this.Cycles;
			var oldPage = HighByte(this.PC);
			this.PC += (ushort)((short)displacement);
			var newPage = HighByte(this.PC);
			if (oldPage != newPage)
			{
				this.Cycles += 2;
			}
		}

		private void Branch()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			this.Branch(displacement);
		}

		private void Branch(bool flag)
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if (flag)
			{
				this.Branch(displacement);
			}
		}

		private void BitBranch_Clear(byte check)
		{
			var zp = this.FetchByte();
			var contents = this.GetByte(zp);
			var displacement = (sbyte)this.FetchByte();
			if ((contents & check) == 0)
			{
				this.PC += (ushort)displacement;
			}
		}

		private void BitBranch_Set(byte check)
		{
			var zp = this.FetchByte();
			var contents = this.GetByte(zp);
			var displacement = (sbyte)this.FetchByte();
			if ((contents & check) != 0)
			{
				this.PC += (ushort)displacement;
			}
		}

		#endregion

		#region Instruction implementations

		private void NOP_imp()
		{
		}

		private void NOP2_imp()
		{
			this.FetchByte();
		}

		private void NOP3_imp()
		{
			this.FetchWord();
		}

		#region Readers

		#region ORA

		private void ORA_xind()
		{
			this.ORA(this.ReadByte_IndexedIndirectX());
		}

		private void ORA_zp()
		{
			this.ORA(this.ReadByte_ZeroPage());
		}

		private void ORA_imm()
		{
			this.ORA(this.ReadByte_Immediate());
		}

		private void ORA_abs()
		{
			this.ORA(this.ReadByte_Absolute());
		}

		private void ORA_absx()
		{
			this.ORA(this.ReadByte_AbsoluteX());
		}

		private void ORA_absy()
		{
			this.ORA(this.ReadByte_AbsoluteY());
		}

		private void ORA_zpx()
		{
			this.ORA(this.ReadByte_ZeroPageX());
		}

		private void ORA_indy()
		{
			this.ORA(this.ReadByte_IndirectIndexedY());
		}

		private void ORA_zpind()
		{
			this.ORA(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region AND

		private void AND_zpx()
		{
			this.AND(this.ReadByte_ZeroPageX());
		}

		private void AND_indy()
		{
			this.AND(this.ReadByte_IndirectIndexedY());
		}

		private void AND_zp()
		{
			this.AND(this.ReadByte_ZeroPage());
		}

		private void AND_absx()
		{
			this.AND(this.ReadByte_AbsoluteX());
		}

		private void AND_absy()
		{
			this.AND(this.ReadByte_AbsoluteY());
		}

		private void AND_imm()
		{
			this.AND(this.ReadByte_Immediate());
		}

		private void AND_xind()
		{
			this.AND(this.ReadByte_IndexedIndirectX());
		}

		private void AND_abs()
		{
			this.AND(this.ReadByte_Absolute());
		}

		private void AND_zpind()
		{
			this.AND(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region EOR

		private void EOR_absx()
		{
			this.EOR(this.ReadByte_AbsoluteX());
		}

		private void EOR_absy()
		{
			this.EOR(this.ReadByte_AbsoluteY());
		}

		private void EOR_zpx()
		{
			this.EOR(this.ReadByte_ZeroPageX());
		}

		private void EOR_indy()
		{
			this.EOR(this.ReadByte_IndirectIndexedY());
		}

		private void EOR_abs()
		{
			this.EOR(this.ReadByte_Absolute());
		}

		private void EOR_imm()
		{
			this.EOR(this.ReadByte_Immediate());
		}

		private void EOR_zp()
		{
			this.EOR(this.ReadByte_ZeroPage());
		}

		private void EOR_xind()
		{
			this.EOR(this.ReadByte_IndexedIndirectX());
		}

		private void EOR_zpind()
		{
			this.EOR(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region LDA

		private void LDA_absx()
		{
			this.LDA(this.ReadByte_AbsoluteX());
		}

		private void LDA_absy()
		{
			this.LDA(this.ReadByte_AbsoluteY());
		}

		private void LDA_zpx()
		{
			this.LDA(this.ReadByte_ZeroPageX());
		}

		private void LDA_indy()
		{
			this.LDA(this.ReadByte_IndirectIndexedY());
		}

		private void LDA_abs()
		{
			this.LDA(this.ReadByte_Absolute());
		}

		private void LDA_imm()
		{
			this.LDA(this.ReadByte_Immediate());
		}

		private void LDA_zp()
		{
			this.LDA(this.ReadByte_ZeroPage());
		}

		private void LDA_xind()
		{
			this.LDA(this.ReadByte_IndexedIndirectX());
		}

		private void LDA_zpind()
		{
			this.LDA(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region LDX

		private void LDX_imm()
		{
			this.LDX(this.ReadByte_Immediate());
		}

		private void LDX_zp()
		{
			this.LDX(this.ReadByte_ZeroPage());
		}

		private void LDX_abs()
		{
			this.LDX(this.ReadByte_Absolute());
		}

		private void LDX_zpy()
		{
			this.LDX(this.ReadByte_ZeroPageY());
		}

		private void LDX_absy()
		{
			this.LDX(this.ReadByte_AbsoluteY());
		}

		#endregion

		#region LDY

		private void LDY_imm()
		{
			this.LDY(this.ReadByte_Immediate());
		}

		private void LDY_zp()
		{
			this.LDY(this.ReadByte_ZeroPage());
		}

		private void LDY_abs()
		{
			this.LDY(this.ReadByte_Absolute());
		}

		private void LDY_zpx()
		{
			this.LDY(this.ReadByte_ZeroPageX());
		}

		private void LDY_absx()
		{
			this.LDY(this.ReadByte_AbsoluteX());
		}

		#endregion

		#region CMP

		private void CMP_absx()
		{
			this.CMP(this.ReadByte_AbsoluteX());
		}

		private void CMP_absy()
		{
			this.CMP(this.ReadByte_AbsoluteY());
		}

		private void CMP_zpx()
		{
			this.CMP(this.ReadByte_ZeroPageX());
		}

		private void CMP_indy()
		{
			this.CMP(this.ReadByte_IndirectIndexedY());
		}

		private void CMP_abs()
		{
			this.CMP(this.ReadByte_Absolute());
		}

		private void CMP_imm()
		{
			this.CMP(this.ReadByte_Immediate());
		}

		private void CMP_zp()
		{
			this.CMP(this.ReadByte_ZeroPage());
		}

		private void CMP_xind()
		{
			this.CMP(this.ReadByte_IndexedIndirectX());
		}

		private void CMP_zpind()
		{
			this.CMP(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region CPX

		private void CPX_abs()
		{
			this.CPX(this.ReadByte_Absolute());
		}

		private void CPX_zp()
		{
			this.CPX(this.ReadByte_ZeroPage());
		}

		private void CPX_imm()
		{
			this.CPX(this.ReadByte_Immediate());
		}

		#endregion

		#region CPY

		private void CPY_imm()
		{
			this.CPY(this.ReadByte_Immediate());
		}

		private void CPY_zp()
		{
			this.CPY(this.ReadByte_ZeroPage());
		}

		private void CPY_abs()
		{
			this.CPY(this.ReadByte_Absolute());
		}

		#endregion

		#region ADC

		private void ADC_zp()
		{
			this.ADC(this.ReadByte_ZeroPage());
		}

		private void ADC_xind()
		{
			this.ADC(this.ReadByte_IndexedIndirectX());
		}

		private void ADC_imm()
		{
			this.ADC(this.ReadByte_Immediate());
		}

		private void ADC_abs()
		{
			this.ADC(this.ReadByte_Absolute());
		}

		private void ADC_zpx()
		{
			this.ADC(this.ReadByte_ZeroPageX());
		}

		private void ADC_indy()
		{
			this.ADC(this.ReadByte_IndirectIndexedY());
		}

		private void ADC_absx()
		{
			this.ADC(this.ReadByte_AbsoluteX());
		}

		private void ADC_absy()
		{
			this.ADC(this.ReadByte_AbsoluteY());
		}

		private void ADC_zpind()
		{
			this.ADC(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region SBC

		private void SBC_xind()
		{
			this.SBC(this.ReadByte_IndexedIndirectX());
		}

		private void SBC_zp()
		{
			this.SBC(this.ReadByte_ZeroPage());
		}

		private void SBC_imm()
		{
			this.SBC(this.ReadByte_Immediate());
		}

		private void SBC_abs()
		{
			this.SBC(this.ReadByte_Absolute());
		}

		private void SBC_zpx()
		{
			this.SBC(this.ReadByte_ZeroPageX());
		}

		private void SBC_indy()
		{
			this.SBC(this.ReadByte_IndirectIndexedY());
		}

		private void SBC_absx()
		{
			this.SBC(this.ReadByte_AbsoluteX());
		}

		private void SBC_absy()
		{
			this.SBC(this.ReadByte_AbsoluteY());
		}

		private void SBC_zpind()
		{
			this.SBC(this.ReadByte_ZeroPageIndirect());
		}

		#endregion

		#region BIT

		private void BIT_imm()
		{
			this.BIT_immediate(this.ReadByte_Immediate());
		}

		private void BIT_zp()
		{
			this.BIT(this.ReadByte_ZeroPage());
		}

		private void BIT_zpx()
		{
			this.BIT(this.ReadByte_ZeroPageX());
		}

		private void BIT_abs()
		{
			this.BIT(this.ReadByte_Absolute());
		}

		private void BIT_absx()
		{
			this.BIT(this.ReadByte_AbsoluteX());
		}

		#endregion

		#endregion

		#region Increment and decrement

		#region DEC

		private void DEC_a()
		{
			this.A--;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void DEC_absx()
		{
			this.DEC(this.Address_AbsoluteX_Write());
		}

		private void DEC_zpx()
		{
			this.DEC(this.Address_ZeroPageX());
		}

		private void DEC_abs()
		{
			this.DEC(this.Address_Absolute());
		}

		private void DEC_zp()
		{
			this.DEC(this.Address_ZeroPage());
		}

		#region X/Y

		private void DEX_imp()
		{
			--this.X;
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void DEY_imp()
		{
			--this.Y;
			this.P.Zero = (this.Y == 0);
			this.P.Negative = ((sbyte)this.Y < 0);
		}

		#endregion

		#endregion

		#region INC

		private void INC_a()
		{
			++this.A;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void INC_zp()
		{
			this.INC(this.Address_ZeroPage());
		}

		private void INC_absx()
		{
			this.INC(this.Address_AbsoluteX_Write());
		}

		private void INC_zpx()
		{
			this.INC(this.Address_ZeroPageX());
		}

		private void INC_abs()
		{
			this.INC(this.Address_Absolute());
		}

		#region X/Y

		private void INX_imp()
		{
			++this.X;
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void INY_imp()
		{
			++this.Y;
			this.P.Zero = (this.Y == 0);
			this.P.Negative = ((sbyte)this.Y < 0);
		}

		#endregion

		#endregion

		#endregion

		#region Writers

		#region STX

		private void STX_zpy()
		{
			this.WriteByte_ZeroPageY(this.X);
		}

		private void STX_abs()
		{
			this.WriteByte_Absolute(this.X);
		}

		private void STX_zp()
		{
			this.WriteByte_ZeroPage(this.X);
		}

		#endregion

		#region STY

		private void STY_zpx()
		{
			this.WriteByte_ZeroPageX(this.Y);
		}

		private void STY_abs()
		{
			this.WriteByte_Absolute(this.Y);
		}

		private void STY_zp()
		{
			this.WriteByte_ZeroPage(this.Y);
		}

		#endregion

		#region STA

		private void STA_absx()
		{
			this.WriteByte_AbsoluteX(this.A);
		}

		private void STA_absy()
		{
			this.WriteByte_AbsoluteY(this.A);
		}

		private void STA_zpx()
		{
			this.WriteByte_ZeroPageX(this.A);
		}

		private void STA_indy()
		{
			this.WriteByte_IndirectIndexedY(this.A);
		}

		private void STA_abs()
		{
			this.WriteByte_Absolute(this.A);
		}

		private void STA_zp()
		{
			this.WriteByte_ZeroPage(this.A);
		}

		private void STA_xind()
		{
			this.WriteByte_IndexedIndirectX(this.A);
		}

		private void STA_zpind()
		{
			this.WriteByte_ZeroPageIndirect(this.A);
		}

		#endregion

		#region STZ

		private void STZ_zp()
		{
			this.WriteByte_ZeroPage((byte)0);
		}

		private void STZ_zpx()
		{
			this.WriteByte_ZeroPageX((byte)0);
		}

		private void STZ_abs()
		{
			this.WriteByte_Absolute((byte)0);
		}

		private void STZ_absx()
		{
			this.WriteByte_AbsoluteX((byte)0);
		}

		#endregion

		#endregion

		#region Transfers

		private void TSX_imp()
		{
			this.X = this.S;
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void TAX_imp()
		{
			this.X = this.A;
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void TAY_imp()
		{
			this.Y = this.A;
			this.P.Zero = (this.Y == 0);
			this.P.Negative = ((sbyte)this.Y < 0);
		}

		private void TXS_imp()
		{
			this.S = this.X;
		}

		private void TYA_imp()
		{
			this.A = this.Y;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void TXA_imp()
		{
			this.A = this.X;
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		#endregion

		#region Stack operations

		private void PHP_imp()
		{
			this.P.Break = true;
			this.PushByte(this.P);
		}

		private void PLP_imp()
		{
			this.P = new StatusFlags(this.PopByte());
			this.P.Reserved = true;
		}

		private void PLA_imp()
		{
			this.A = this.PopByte();
			this.P.Zero = (this.A == 0);
			this.P.Negative = ((sbyte)this.A < 0);
		}

		private void PHA_imp()
		{
			this.PushByte(this.A);
		}

		private void PHX_imp()
		{
			this.PushByte(this.X);
		}

		private void PHY_imp()
		{
			this.PushByte(this.Y);
		}

		private void PLX_imp()
		{
			this.X = this.PopByte();
			this.P.Zero = (this.X == 0);
			this.P.Negative = ((sbyte)this.X < 0);
		}

		private void PLY_imp()
		{
			this.Y = this.PopByte();
			this.P.Zero = (this.Y == 0);
			this.P.Negative = ((sbyte)this.Y < 0);
		}

		#endregion

		#region Shifts and rotations

		#region ASL

		private void ASL_a()
		{
			this.A = this.ASL(this.A);
		}

		private void ASL_zp()
		{
			this.ASL(this.Address_ZeroPage());
		}

		private void ASL_abs()
		{
			this.ASL(this.Address_Absolute());
		}

		private void ASL_absx()
		{
			this.ASL(this.Address_AbsoluteX_Write());
		}

		private void ASL_zpx()
		{
			this.ASL(this.Address_ZeroPageX());
		}

		#endregion

		#region LSR

		private void LSR_absx()
		{
			this.LSR(this.Address_AbsoluteX_Write());
		}

		private void LSR_zpx()
		{
			this.LSR(this.Address_ZeroPageX());
		}

		private void LSR_abs()
		{
			this.LSR(this.Address_Absolute());
		}

		private void LSR_a()
		{
			this.A = this.LSR(this.A);
		}

		private void LSR_zp()
		{
			this.LSR(this.Address_ZeroPage());
		}

		#endregion

		#region ROL

		private void ROL_absx()
		{
			this.ROL(this.Address_AbsoluteX_Write());
		}

		private void ROL_zpx()
		{
			this.ROL(this.Address_ZeroPageX());
		}

		private void ROL_abs()
		{
			this.ROL(this.Address_Absolute());
		}

		private void ROL_a()
		{
			this.A = this.ROL(this.A);
		}

		private void ROL_zp()
		{
			this.ROL(this.Address_ZeroPage());
		}

		#endregion

		#region ROR

		private void ROR_absx()
		{
			this.ROR(this.Address_AbsoluteX_Write());
		}

		private void ROR_zpx()
		{
			this.ROR(this.Address_ZeroPageX());
		}

		private void ROR_abs()
		{
			this.ROR(this.Address_Absolute());
		}

		private void ROR_a()
		{
			this.A = this.ROR(this.A);
		}

		private void ROR_zp()
		{
			this.ROR(this.Address_ZeroPage());
		}

		#endregion

		#endregion
		
		#region Test set/reset bits

		#region TSB

		private void TSB_zp()
		{
			this.TSB(this.Address_ZeroPage());
		}

		private void TSB_abs()
		{
			this.TSB(this.Address_Absolute());
		}

		#endregion

		#region TRB

		private void TRB_zp()
		{
			this.TRB(this.Address_ZeroPage());
		}

		private void TRB_abs()
		{
			this.TRB(this.Address_Absolute());
		}

		#endregion

		#endregion

		#region Set/reset bits

		#region RMBn

		private void RMB0_zp()
		{
			this.RMB(this.Address_ZeroPage(), 1);
		}

		private void RMB1_zp()
		{
			this.RMB(this.Address_ZeroPage(), 2);
		}

		private void RMB2_zp()
		{
			this.RMB(this.Address_ZeroPage(), 4);
		}

		private void RMB3_zp()
		{
			this.RMB(this.Address_ZeroPage(), 8);
		}

		private void RMB4_zp()
		{
			this.RMB(this.Address_ZeroPage(), 0x10);
		}

		private void RMB5_zp()
		{
			this.RMB(this.Address_ZeroPage(), 0x20);
		}

		private void RMB6_zp()
		{
			this.RMB(this.Address_ZeroPage(), 0x40);
		}

		private void RMB7_zp()
		{
			this.RMB(this.Address_ZeroPage(), 0x80);
		}

		#endregion

		#region SMBn

		private void SMB0_zp()
		{
			this.SMB(this.Address_ZeroPage(), 1);
		}

		private void SMB1_zp()
		{
			this.SMB(this.Address_ZeroPage(), 2);
		}

		private void SMB2_zp()
		{
			this.SMB(this.Address_ZeroPage(), 4);
		}

		private void SMB3_zp()
		{
			this.SMB(this.Address_ZeroPage(), 8);
		}

		private void SMB4_zp()
		{
			this.SMB(this.Address_ZeroPage(), 0x10);
		}

		private void SMB5_zp()
		{
			this.SMB(this.Address_ZeroPage(), 0x20);
		}

		private void SMB6_zp()
		{
			this.SMB(this.Address_ZeroPage(), 0x40);
		}

		private void SMB7_zp()
		{
			this.SMB(this.Address_ZeroPage(), 0x80);
		}

		#endregion

		#endregion

		#region Jumps and calls

		private void JSR_abs()
		{
			var destination = this.Address_Absolute();
			this.PushWord((ushort)(this.PC - 1));
			this.PC = destination;
		}

		private void RTI_imp()
		{
			this.PLP_imp();
			this.PC = this.PopWord();
		}

		private void RTS_imp()
		{
			this.PC = (ushort)(this.PopWord() + 1);
		}

		private void JMP_abs()
		{
			this.PC = this.Address_Absolute();
		}

		private void JMP_ind()
		{
			this.PC = this.GetWord(this.Address_Absolute());
		}

		private void JMP_absxind()
		{
			this.PC = this.Address_AbsoluteXIndirect();
		}

		private void BRK_imp()
		{
			this.PushWord((ushort)(this.PC + 1));
			this.PHP_imp();
			this.P.Interrupt = true;
			if (this.level >= ProcessorType.cpu65sc02)
			{
				this.P.Decimal = false;
			}

			this.PC = this.GetWord(IRQvector);
		}

		#endregion

		#region Halt and wait

		private void WAI_imp()
		{
			throw new NotImplementedException();
		}

		private void STP_imp()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Flags

		private void SED_imp()
		{
			this.P.Decimal = true;
		}

		private void CLD_imp()
		{
			this.P.Decimal = false;
		}

		private void CLV_imp()
		{
			this.P.Overflow = false;
		}

		private void SEI_imp()
		{
			this.P.Interrupt = true;
		}

		private void CLI_imp()
		{
			this.P.Interrupt = false;
		}

		private void CLC_imp()
		{
			this.P.Carry = false;
		}

		private void SEC_imp()
		{
			this.P.Carry = true;
		}

		#endregion

		#region Branches

		private void BMI_rel()
		{
			this.Branch(this.P.Negative);
		}

		private void BPL_rel()
		{
			this.Branch(!this.P.Negative);
		}

		private void BVC_rel()
		{
			this.Branch(!this.P.Overflow);
		}

		private void BVS_rel()
		{
			this.Branch(this.P.Overflow);
		}

		private void BCC_rel()
		{
			this.Branch(!this.P.Carry);
		}

		private void BCS_rel()
		{
			this.Branch(this.P.Carry);
		}

		private void BNE_rel()
		{
			this.Branch(!this.P.Zero);
		}

		private void BEQ_rel()
		{
			this.Branch(this.P.Zero);
		}

		private void BRA_rel()
		{
			this.Branch();
		}

		#region Bit branches

		private void BBR0_zprel()
		{
			this.BitBranch_Clear(0x1);
		}

		private void BBR1_zprel()
		{
			this.BitBranch_Clear(0x2);
		}

		private void BBR2_zprel()
		{
			this.BitBranch_Clear(0x4);
		}

		private void BBR3_zprel()
		{
			this.BitBranch_Clear(0x8);
		}

		private void BBR4_zprel()
		{
			this.BitBranch_Clear(0x10);
		}

		private void BBR5_zprel()
		{
			this.BitBranch_Clear(0x20);
		}

		private void BBR6_zprel()
		{
			this.BitBranch_Clear(0x40);
		}

		private void BBR7_zprel()
		{
			this.BitBranch_Clear(0x80);
		}

		private void BBS0_zprel()
		{
			this.BitBranch_Set(0x1);
		}

		private void BBS1_zprel()
		{
			this.BitBranch_Set(0x2);
		}

		private void BBS2_zprel()
		{
			this.BitBranch_Set(0x4);
		}

		private void BBS3_zprel()
		{
			this.BitBranch_Set(0x8);
		}

		private void BBS4_zprel()
		{
			this.BitBranch_Set(0x10);
		}

		private void BBS5_zprel()
		{
			this.BitBranch_Set(0x20);
		}

		private void BBS6_zprel()
		{
			this.BitBranch_Set(0x40);
		}

		private void BBS7_zprel()
		{
			this.BitBranch_Set(0x80);
		}

		#endregion

		#endregion

		#endregion
	}
}
