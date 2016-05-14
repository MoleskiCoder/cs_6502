namespace Simulator
{
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

		private Instruction[] instructions;

		protected MOS6502()
		{
			this.instructions = new Instruction[]
			{
				////	0 														1														2														3													4														5														6														7													8															9															A														B													C														D															E															F
				/* 0 */	INS(this.BRK_imp, 7, AddressingMode.Implied, "BRK"),	INS(this.ORA_xind, 6, AddressingMode.XIndexed, "ORA"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_zp, 4, AddressingMode.ZeroPage, "ORA"),	INS(this.ASL_zp, 5, AddressingMode.ZeroPage, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PHP_imp, 3, AddressingMode.Implied, "PHP"),		INS(this.ORA_imm, 2, AddressingMode.Immediate, "ORA"),		INS(this.ASL_imp, 2, AddressingMode.Implied, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_abs, 4, AddressingMode.Absolute, "ORA"),		INS(this.ASL_abs, 6, AddressingMode.Absolute, "ASL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 1 */	INS(this.BPL_rel, 2, AddressingMode.Relative, "BPL"),   INS(this.ORA_indy, 5, AddressingMode.IndexedY, "ORA"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_zpx, 4, AddressingMode.ZeroPageX, "ORA"),	INS(this.ASL_zpx, 6, AddressingMode.ZeroPageX, "ASL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.CLC_imp, 2, AddressingMode.Implied, "CLC"),		INS(this.ORA_absy, 4, AddressingMode.AbsoluteY, "ORA"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ORA_absx, 4, AddressingMode.AbsoluteX, "ORA"),		INS(this.ASL_absx, 7, AddressingMode.AbsoluteX, "ASL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 2 */	INS(this.JSR_abs, 6, AddressingMode.Absolute, "JSR"),   INS(this.AND_xind, 6, AddressingMode.XIndexed, "AND"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.BIT_zp, 3, AddressingMode.ZeroPage, "BIT"),	INS(this.AND_zp, 3, AddressingMode.ZeroPage, "AND"),	INS(this.ROL_zp, 5, AddressingMode.ZeroPage, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PLP_imp, 4, AddressingMode.Implied, "PLP"),		INS(this.AND_imm, 2, AddressingMode.Immediate, "AND"),		INS(this.ROL_imp, 2, AddressingMode.Implied, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.BIT_abs, 4, AddressingMode.Absolute, "BIT"),	INS(this.AND_abs, 4, AddressingMode.Absolute, "AND"),		INS(this.ROL_abs, 6, AddressingMode.Absolute, "ROL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 3 */	INS(this.BMI_rel, 2, AddressingMode.Relative, "BMI"),   INS(this.AND_indy, 5, AddressingMode.IndexedY, "AND"),  INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.AND_zpx, 4, AddressingMode.ZeroPageX, "AND"),	INS(this.ROL_zpx, 6, AddressingMode.ZeroPageX, "ROL"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.SEC_imp, 2, AddressingMode.Implied, "SEC"),		INS(this.AND_absy, 4, AddressingMode.AbsoluteY, "AND"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.AND_absx, 4, AddressingMode.AbsoluteX, "AND"),		INS(this.ROL_absx, 7, AddressingMode.AbsoluteX, "ROL"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 4 */	INS(this.RTI_imp, 6, AddressingMode.Implied, "RTI"),	INS(this.EOR_xind, 6, AddressingMode.XIndexed, "EOR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_zp, 3, AddressingMode.ZeroPage, "EOR"),	INS(this.LSR_zp, 5, AddressingMode.ZeroPage, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.PHA_imp, 3, AddressingMode.Implied, "PHA"),		INS(this.EOR_imm, 2, AddressingMode.Immediate, "EOR"),		INS(this.LSR_imp, 2, AddressingMode.Implied, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.JMP_abs, 3, AddressingMode.Absolute, "JMP"),	INS(this.EOR_abs, 4, AddressingMode.Absolute, "EOR"),		INS(this.LSR_abs, 6, AddressingMode.Absolute, "LSR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 5 */	INS(this.BVC_rel, 2, AddressingMode.Relative, "BVC"),   INS(this.EOR_indy, 5, AddressingMode.IndexedY, "EOR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_zpx, 4, AddressingMode.ZeroPageX, "EOR"),	INS(this.LSR_zpx, 6, AddressingMode.ZeroPageX, "LSR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.CLI_imp, 2, AddressingMode.Implied, "CLI"),		INS(this.EOR_absy, 4, AddressingMode.AbsoluteY, "EOR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.EOR_absx, 4, AddressingMode.AbsoluteX, "EOR"),		INS(this.LSR_absx, 7, AddressingMode.AbsoluteX, "LSR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
				/* 6 */	INS(this.RTS_imp, 6, AddressingMode.Implied, "RTS"),	INS(this.ADC_xind, 6, AddressingMode.XIndexed, "ADC"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.___, 0, AddressingMode.Illegal, "___"),		INS(this.ADC_zp, 3, AddressingMode.ZeroPage, "ADC"),	INS(this.ROR_zp, 5, AddressingMode.ZeroPage, "ROR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),    INS(this.PLA_imp, 4, AddressingMode.Implied, "PLA"),		INS(this.ADC_imm, 2, AddressingMode.Immediate, "ADC"),		INS(this.ROR_imp, 2, AddressingMode.Implied, "ROR"),	INS(this.___, 0, AddressingMode.Illegal, "___"),	INS(this.JMP_ind, 5, AddressingMode.Indirect, "JMP"),	INS(this.ADC_abs, 4, AddressingMode.Absolute, "ADC"),		INS(this.ROR_abs, 6, AddressingMode.Absolute, "ROR"),		INS(this.___, 0, AddressingMode.Illegal, "___"),
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
		}

		public event PropertyChangedEventHandler PropertyChanged;

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

		protected Instruction[] Instructions
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
				if (this.x != value)
				{
					this.x = value;
					this.OnPropertyChanged("X");
				}
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
				if (this.y != value)
				{
					this.y = value;
					this.OnPropertyChanged("Y");
				}
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
				if (this.a != value)
				{
					this.a = value;
					this.OnPropertyChanged("A");
				}
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
				if (this.s != value)
				{
					this.s = value;
					this.OnPropertyChanged("S");
				}
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
			this.Cycles = 0;
			while (this.Step())
			{
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

		public abstract byte GetByte(ushort offset);

		public abstract void SetByte(ushort offset, byte value);

		protected virtual void OnPropertyChanged(string propertyName)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void Interrupt(ushort vector)
		{
			this.PushWord(this.PC);
			this.PushByte((byte)this.P);
			this.SetFlag(StatusFlags.Interrupt);
			this.PC = this.GetWord(vector);
		}

		protected virtual bool Execute(byte instruction)
		{
			return this.Execute(this.instructions[instruction]);
		}

		protected virtual bool Execute(Instruction instruction)
		{
			var method = instruction.Vector;

			method();
			this.Cycles += instruction.Count;

			this.OnPropertyChanged("P");
			this.OnPropertyChanged("PC");
			this.OnPropertyChanged("Cycles");

			return true;
		}

		protected virtual void ___()
		{
		}

		protected void ResetRegisters()
		{
			this.PC = 0x0000;
			this.X = 0x80;
			this.Y = 0x00;
			this.A = 0x00;
			this.P = StatusFlags.Reserved;
			this.S = 0xff;
		}

		protected virtual bool Step()
		{
			return this.Execute(this.FetchByte());
		}

		protected virtual ushort GetWord(ushort offset)
		{
			var low = this.GetByte(offset);
			var high = this.GetByte((ushort)(offset + 1));
			return MakeWord(low, high);
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
			var returnValue = this.FetchByte(ref this.pc);
			this.OnPropertyChanged("PC");
			return returnValue;
		}

		private ushort FetchWord()
		{
			var returnValue = this.FetchWord(ref this.pc);
			this.OnPropertyChanged("PC");
			return returnValue;
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

		////

		private bool UpdateFlag_Zero(byte value)
		{
			if (value == 0)
			{
				this.SetFlag(StatusFlags.Zero);
				return true;
			}

			return false;
		}

		private void UpdateFlag_Negative(sbyte value)
		{
			if (value < 0)
			{
				this.SetFlag(StatusFlags.Negative);
			}
		}

		private void UpdateFlags_ZeroNegative(byte value)
		{
			if (!this.UpdateFlag_Zero(value))
			{
				this.UpdateFlag_Negative((sbyte)value);
			}
		}

		private void ReflectFlags_ZeroNegative(byte value)
		{
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero);
			this.UpdateFlags_ZeroNegative(value);
		}

		////

		private void DEC(ushort offset)
		{
			var content = (sbyte)this.GetByte(offset);
			this.SetByte(offset, (byte)(--content));
			this.ReflectFlags_ZeroNegative((byte)content);
		}

		private byte ROR(byte data)
		{
			bool carry = this.IsFlagSet(StatusFlags.Carry);
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			if ((data & 1) != 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			var result = (byte)(data >> 1);
			if (carry)
			{
				result |= 0x80;
			}

			this.UpdateFlags_ZeroNegative(result);

			return result;
		}

		private void ROR(ushort offset)
		{
			this.SetByte(offset, this.ROR(this.GetByte(offset)));
		}

		private byte LSR(byte data)
		{
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			if ((data & 1) != 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			var result = (byte)(data >> 1);

			if (result == 0)
			{
				this.SetFlag(StatusFlags.Zero);
			}

			return result;
		}

		private void LSR(ushort offset)
		{
			this.SetByte(offset, this.LSR(this.GetByte(offset)));
		}

		private void BIT(byte data)
		{
			this.ClearFlag(StatusFlags.Zero | StatusFlags.Overflow | StatusFlags.Negative);

			var result = (byte)(this.A & data);

			if (result == 0)
			{
				this.SetFlag(StatusFlags.Zero);
			}

			if ((data & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Negative);
			}

			if ((data & 0x40) != 0)
			{
				this.SetFlag(StatusFlags.Overflow);
			}
		}

		private void INC(ushort offset)
		{
			var content = this.GetByte(offset);
			this.SetByte(offset, ++content);
			this.ReflectFlags_ZeroNegative(content);
		}

		private void ROL(ushort offset)
		{
			this.SetByte(offset, this.ROL(this.GetByte(offset)));
		}

		private byte ROL(byte data)
		{
			var carry = this.IsFlagSet(StatusFlags.Carry);

			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			if ((data & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			var result = (byte)(data << 1);

			if (carry)
			{
				result |= 0x01;
			}

			this.UpdateFlags_ZeroNegative(result);

			return result;
		}

		private void ASL(ushort offset)
		{
			this.SetByte(offset, this.ASL(this.GetByte(offset)));
		}

		private byte ASL(byte data)
		{
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			byte result = (byte)(data << 1);
			this.UpdateFlags_ZeroNegative(result);

			if ((data & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			return result;
		}

		private void ORA(byte data)
		{
			this.A |= data;
			this.ReflectFlags_ZeroNegative(this.A);
		}

		private void AND(byte data)
		{
			this.A &= data;
			this.ReflectFlags_ZeroNegative(this.A);
		}

		private void SBC(byte data)
		{
			if ((this.P & StatusFlags.Decimal) != 0)
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
			var carry = (byte)(this.IsFlagClear(StatusFlags.Carry) ? 1 : 0);
			var difference = (ushort)(this.A - data - carry);

			this.ClearFlag(StatusFlags.Zero | StatusFlags.Overflow | StatusFlags.Negative | StatusFlags.Carry);
			this.UpdateFlags_ZeroNegative((byte)difference);

			if (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Overflow);
			}

			if ((difference & 0xff00) == 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			this.A = (byte)difference;
		}

		private void SBC_d(byte data)
		{
			var carry = (byte)(this.IsFlagClear(StatusFlags.Carry) ? 1 : 0);
			var difference = (ushort)(this.A - data - carry);

			this.ClearFlag(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero | StatusFlags.Carry);
			this.UpdateFlags_ZeroNegative((byte)difference);

			if (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Overflow);
			}

			if ((difference & 0xff00) == 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

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
		}

		private void EOR(byte data)
		{
			this.A ^= data;
			this.ReflectFlags_ZeroNegative(this.A);
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
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			var result = (ushort)(first - second);

			this.UpdateFlags_ZeroNegative((byte)result);

			if ((result & 0xff00) == 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}
		}

		private void LDA(byte data)
		{
			this.A = data;
			this.ReflectFlags_ZeroNegative(this.A);
		}

		private void LDY(byte data)
		{
			this.Y = data;
			this.ReflectFlags_ZeroNegative(this.Y);
		}

		private void LDX(byte data)
		{
			this.X = data;
			this.ReflectFlags_ZeroNegative(this.X);
		}

		private void ADC(byte data)
		{
			if ((this.P & StatusFlags.Decimal) != 0)
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
			var carry = (byte)(this.IsFlagSet(StatusFlags.Carry) ? 1 : 0);
			var sum = (ushort)(this.A + data + carry);
			this.ClearFlag(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero | StatusFlags.Carry);

			this.UpdateFlags_ZeroNegative((byte)sum);

			if ((~(this.A ^ data) & (this.A ^ sum) & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Overflow);
			}

			if (HighByte(sum) != 0)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			this.A = (byte)sum;
		}

		private void ADC_d(byte data)
		{
			var carry = (byte)(this.IsFlagSet(StatusFlags.Carry) ? 1 : 0);
			var sum = (ushort)(this.A + data + carry);

			this.ClearFlag(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero | StatusFlags.Carry);
			this.UpdateFlags_ZeroNegative((byte)sum);

			var low = (byte)(LowNybble(this.A) + LowNybble(data) + carry);
			if (low > 9)
			{
				low += 6;
			}

			var high = (byte)(HighNybble(this.A) + HighNybble(data) + (low > 0x0f ? 1 : 0));
			if ((~(this.A ^ data) & (this.A ^ (high << 4)) & 0x80) != 0)
			{
				this.SetFlag(StatusFlags.Overflow);
			}

			if (high > 9)
			{
				high += 6;
			}

			if (high > 0x0f)
			{
				this.SetFlag(StatusFlags.Carry);
			}

			this.A = (byte)(PromoteNybble(high) | LowNybble(low));
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

		private void Branch_False(StatusFlags flag)
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if (this.IsFlagClear(flag))
			{
				this.Branch(displacement);
			}
		}

		private void Branch_True(StatusFlags flag)
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if (this.IsFlagSet(flag))
			{
				this.Branch(displacement);
			}
		}

		#endregion

		#region Flag clear/set

		private void SetFlag(StatusFlags flag)
		{
			this.P |= flag;
		}

		private bool IsFlagSet(StatusFlags flag)
		{
			return (this.P & flag) != 0;
		}

		private void ClearFlag(StatusFlags flag)
		{
			this.P &= ~flag;
		}

		private bool IsFlagClear(StatusFlags flag)
		{
			return (this.P & flag) == 0;
		}

		#endregion

		#region Instruction implementations

		private void NOP_imp()
		{
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

		#endregion

		#region BIT

		private void BIT_zp()
		{
			this.BIT(this.ReadByte_ZeroPage());
		}

		private void BIT_abs()
		{
			this.BIT(this.ReadByte_Absolute());
		}

		#endregion

		#endregion

		#region Increment and decrement

		#region DEC

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
			this.ReflectFlags_ZeroNegative(--this.X);
		}

		private void DEY_imp()
		{
			this.ReflectFlags_ZeroNegative(--this.Y);
		}

		#endregion

		#endregion

		#region INC

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
			this.ReflectFlags_ZeroNegative(++this.X);
		}

		private void INY_imp()
		{
			this.ReflectFlags_ZeroNegative(++this.Y);
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

		#endregion

		#endregion

		#region Transfers

		private void TSX_imp()
		{
			this.X = this.S;
			this.ReflectFlags_ZeroNegative(this.X);
		}

		private void TAX_imp()
		{
			this.X = this.A;
			this.ReflectFlags_ZeroNegative(this.X);
		}

		private void TAY_imp()
		{
			this.Y = this.A;
			this.ReflectFlags_ZeroNegative(this.Y);
		}

		private void TXS_imp()
		{
			this.S = this.X;
		}

		private void TYA_imp()
		{
			this.A = this.Y;
			this.ReflectFlags_ZeroNegative(this.A);
		}

		private void TXA_imp()
		{
			this.A = this.X;
			this.ReflectFlags_ZeroNegative(this.A);
		}

		#endregion

		#region Stack operations

		private void PHP_imp()
		{
			this.SetFlag(StatusFlags.Break);
			this.PushByte((byte)this.P);
		}

		private void PLP_imp()
		{
			this.P = (StatusFlags)this.PopByte() | StatusFlags.Reserved;
		}

		private void PLA_imp()
		{
			this.A = this.PopByte();
			this.ReflectFlags_ZeroNegative(this.A);
		}

		private void PHA_imp()
		{
			this.PushByte(this.A);
		}

		#endregion

		#region Shifts and rotations

		#region ASL

		private void ASL_imp()
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

		private void LSR_imp()
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

		private void ROL_imp()
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

		private void ROR_imp()
		{
			this.A = this.ROR(this.A);
		}

		private void ROR_zp()
		{
			this.ROR(this.Address_ZeroPage());
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

		private void BRK_imp()
		{
			this.PushWord((ushort)(this.PC + 1));
			this.PHP_imp();
			this.SetFlag(StatusFlags.Interrupt);
			this.PC = this.GetWord(IRQvector);
		}

		#endregion

		#region Flags

		private void SED_imp()
		{
			this.SetFlag(StatusFlags.Decimal);
		}

		private void CLD_imp()
		{
			this.ClearFlag(StatusFlags.Decimal);
		}

		private void CLV_imp()
		{
			this.ClearFlag(StatusFlags.Overflow);
		}

		private void SEI_imp()
		{
			this.SetFlag(StatusFlags.Interrupt);
		}

		private void CLI_imp()
		{
			this.ClearFlag(StatusFlags.Interrupt);
		}

		private void CLC_imp()
		{
			this.ClearFlag(StatusFlags.Carry);
		}

		private void SEC_imp()
		{
			this.SetFlag(StatusFlags.Carry);
		}

		#endregion

		#region Branches

		private void BMI_rel()
		{
			this.Branch_True(StatusFlags.Negative);
		}

		private void BPL_rel()
		{
			this.Branch_False(StatusFlags.Negative);
		}

		private void BVC_rel()
		{
			this.Branch_False(StatusFlags.Overflow);
		}

		private void BVS_rel()
		{
			this.Branch_True(StatusFlags.Overflow);
		}

		private void BCC_rel()
		{
			this.Branch_False(StatusFlags.Carry);
		}

		private void BCS_rel()
		{
			this.Branch_True(StatusFlags.Carry);
		}

		private void BNE_rel()
		{
			this.Branch_False(StatusFlags.Zero);
		}

		private void BEQ_rel()
		{
			this.Branch_True(StatusFlags.Zero);
		}

		#endregion

		#endregion
	}
}
