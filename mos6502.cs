using System;

namespace Simulator
{
	public abstract class mos6502
	{
		const ushort pageOne = 0x100;
		const ushort IRQvector = 0xfffe;
		const ushort RSTvector = 0xfffc;
		const ushort NMIvector = 0xfffa;

		private ushort pc;  // program counter
		private byte x;     // index register X
		private byte y;     // index register Y
		private byte a;     // accumulator
		private byte s;     // stack pointer

		private StatusFlags p;     // processor status

		private ulong cycles;

		private Instruction[] instructions;

		private static Instruction INS(Implementation method, ulong cycles, AddressingMode addressing, string display)
		{
			return new Instruction() { vector = method, count = cycles, display = display, mode = addressing };
		}

		protected mos6502()
		{
			this.instructions = new Instruction[]
			{
				//		0 													1													2													3												4													5													6													7												8													9													A													B												C													D													E													F
				/* 0 */	INS(BRK_imp, 7, AddressingMode.Implied, "BRK"),		INS(ORA_xind, 6, AddressingMode.XIndexed, "ORA"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ORA_zp, 4, AddressingMode.ZeroPage, "ORA"),		INS(ASL_zp, 5, AddressingMode.ZeroPage, "ASL"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(PHP_imp, 3, AddressingMode.Implied, "PHP"),		INS(ORA_imm, 2, AddressingMode.Immediate, "ORA"),	INS(ASL_imp, 2, AddressingMode.Implied, "ASL"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ORA_abs, 4, AddressingMode.Absolute, "ORA"),	INS(ASL_abs, 6, AddressingMode.Absolute, "ASL"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 1 */	INS(BPL_rel, 2, AddressingMode.Relative, "BPL"),    INS(ORA_indy, 5, AddressingMode.IndexedY, "ORA"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ORA_zpx, 4, AddressingMode.ZeroPageX, "ORA"),	INS(ASL_zpx, 6, AddressingMode.ZeroPageX, "ASL"),	INS(___, 0, AddressingMode.Illegal, "___"),		INS(CLC_imp, 2, AddressingMode.Implied, "CLC"),		INS(ORA_absy, 4, AddressingMode.AbsoluteY, "ORA"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ORA_absx, 4, AddressingMode.AbsoluteX, "ORA"),	INS(ASL_absx, 7, AddressingMode.AbsoluteX, "ASL"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 2 */	INS(JSR_abs, 6, AddressingMode.Absolute, "JSR"),    INS(AND_xind, 6, AddressingMode.XIndexed, "AND"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(BIT_zp, 3, AddressingMode.ZeroPage, "BIT"),		INS(AND_zp, 3, AddressingMode.ZeroPage, "AND"),		INS(ROL_zp, 5, AddressingMode.ZeroPage, "ROL"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(PLP_imp, 4, AddressingMode.Implied, "PLP"),		INS(AND_imm, 2, AddressingMode.Immediate, "AND"),	INS(ROL_imp, 2, AddressingMode.Implied, "ROL"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(BIT_abs, 4, AddressingMode.Absolute, "BIT"),	INS(AND_abs, 4, AddressingMode.Absolute, "AND"),	INS(ROL_abs, 6, AddressingMode.Absolute, "ROL"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 3 */	INS(BMI_rel, 2, AddressingMode.Relative, "BMI"),    INS(AND_indy, 5, AddressingMode.IndexedY, "AND"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(AND_zpx, 4, AddressingMode.ZeroPageX, "AND"),	INS(ROL_zpx, 6, AddressingMode.ZeroPageX, "ROL"),	INS(___, 0, AddressingMode.Illegal, "___"),		INS(SEC_imp, 2, AddressingMode.Implied, "SEC"),		INS(AND_absy, 4, AddressingMode.AbsoluteY, "AND"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(AND_absx, 4, AddressingMode.AbsoluteX, "AND"),	INS(ROL_absx, 7, AddressingMode.AbsoluteX, "ROL"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 4 */	INS(RTI_imp, 6, AddressingMode.Implied, "RTI"),		INS(EOR_xind, 6, AddressingMode.XIndexed, "EOR"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(EOR_zp, 3, AddressingMode.ZeroPage, "EOR"),		INS(LSR_zp, 5, AddressingMode.ZeroPage, "LSR"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(PHA_imp, 3, AddressingMode.Implied, "PHA"),		INS(EOR_imm, 2, AddressingMode.Immediate, "EOR"),	INS(LSR_imp, 2, AddressingMode.Implied, "LSR"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(JMP_abs, 3, AddressingMode.Absolute, "JMP"),	INS(EOR_abs, 4, AddressingMode.Absolute, "EOR"),	INS(LSR_abs, 6, AddressingMode.Absolute, "LSR"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 5 */	INS(BVC_rel, 2, AddressingMode.Relative, "BVC"),    INS(EOR_indy, 5, AddressingMode.IndexedY, "EOR"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(EOR_zpx, 4, AddressingMode.ZeroPageX, "EOR"),	INS(LSR_zpx, 6, AddressingMode.ZeroPageX, "LSR"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(CLI_imp, 2, AddressingMode.Implied, "CLI"),		INS(EOR_absy, 4, AddressingMode.AbsoluteY, "EOR"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(EOR_absx, 4, AddressingMode.AbsoluteX, "EOR"),	INS(LSR_absx, 7, AddressingMode.AbsoluteX, "LSR"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 6 */	INS(RTS_imp, 6, AddressingMode.Implied, "RTI"),		INS(ADC_xind, 6, AddressingMode.XIndexed, "ADC"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ADC_zp, 3, AddressingMode.ZeroPage, "ADC"),		INS(ROR_zp, 5, AddressingMode.ZeroPage, "ROR"),		INS(___, 0, AddressingMode.Illegal, "___"),     INS(PLA_imp, 4, AddressingMode.Implied, "PLA"),		INS(ADC_imm, 2, AddressingMode.Immediate, "ADC"),	INS(ROR_imp, 2, AddressingMode.Implied, "ROR"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(JMP_ind, 5, AddressingMode.Indirect, "JMP"),	INS(ADC_abs, 4, AddressingMode.Absolute, "ADC"),	INS(ROR_abs, 6, AddressingMode.Absolute, "ROR"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 7 */	INS(BVS_rel, 2, AddressingMode.Relative, "BVS"),	INS(ADC_indy, 5, AddressingMode.IndexedY, "ADC"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ADC_zpx, 4, AddressingMode.ZeroPageX, "ADC"),	INS(ROR_zpx, 6, AddressingMode.ZeroPageX, "ROR"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(SEI_imp, 2, AddressingMode.Implied, "SEI"),		INS(ADC_absy, 4, AddressingMode.AbsoluteY, "ADC"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(ADC_absx, 4, AddressingMode.AbsoluteX, "ADC"),	INS(ROR_absx, 7, AddressingMode.AbsoluteX, "ROR"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 8 */	INS(___, 0, AddressingMode.Illegal, "___"),			INS(STA_xind, 6, AddressingMode.XIndexed, "STA"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(STY_zp, 3, AddressingMode.ZeroPage, "STY"),		INS(STA_zp, 3, AddressingMode.ZeroPage, "STA"),		INS(STX_zp, 3, AddressingMode.ZeroPage, "STX"),		INS(___, 0, AddressingMode.Illegal, "___"),     INS(DEY_imp, 2, AddressingMode.Implied, "DEY"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(TXA_imp, 2, AddressingMode.Implied, "TXA"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(STY_abs, 4, AddressingMode.Absolute, "STY"),	INS(STA_abs, 4, AddressingMode.Absolute, "STA"),	INS(STX_abs, 4, AddressingMode.Absolute, "STX"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* 9 */	INS(BCC_rel, 2, AddressingMode.Relative, "BCC"),    INS(STA_indy, 6, AddressingMode.IndexedY, "STA"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(STY_zpx, 4, AddressingMode.ZeroPageX, "STY"),	INS(STA_zpx, 4, AddressingMode.ZeroPageX, "STA"),	INS(STX_zpy, 4, AddressingMode.ZeroPageY, "STX"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(TYA_imp, 2, AddressingMode.Implied, "TYA"),		INS(STA_absy, 5, AddressingMode.AbsoluteY, "STA"),	INS(TXS_imp, 2, AddressingMode.Implied, "TXS"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(STA_absx, 5, AddressingMode.AbsoluteX, "STA"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),
				/* A */	INS(LDY_imm, 2, AddressingMode.Immediate, "LDY"),   INS(LDA_xind, 6, AddressingMode.XIndexed, "LDA"),   INS(LDX_imm, 2, AddressingMode.Immediate, "LDX"),	INS(___, 0, AddressingMode.Illegal, "___"),		INS(LDY_zp, 3, AddressingMode.ZeroPage, "LDY"),		INS(LDA_zp, 3, AddressingMode.ZeroPage, "LDA"),     INS(LDX_zp, 3, AddressingMode.ZeroPage, "LDX"),		INS(___, 0, AddressingMode.Illegal, "___"),     INS(TAY_imp, 2, AddressingMode.Implied, "TAY"),		INS(LDA_imm, 2, AddressingMode.Immediate, "LDA"),	INS(TAX_imp, 2, AddressingMode.Implied, "TAX"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(LDY_abs, 4, AddressingMode.Absolute, "LDY"),	INS(LDA_abs, 4, AddressingMode.Absolute, "LDA"),	INS(LDX_abs, 4, AddressingMode.Absolute, "LDX"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* B */	INS(BCS_rel, 2, AddressingMode.Relative, "BCS"),	INS(LDA_indy, 5, AddressingMode.IndexedY, "LDA"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(LDY_zpx, 4, AddressingMode.ZeroPageX, "LDY"),	INS(LDA_zpx, 4, AddressingMode.ZeroPageX, "LDA"),	INS(LDX_zpy, 4, AddressingMode.ZeroPageY, "LDX"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(CLV_imp, 2, AddressingMode.Implied, "CLV"),		INS(LDA_absy, 4, AddressingMode.AbsoluteY, "LDA"),	INS(TSX_imp, 2, AddressingMode.Implied, "TSX"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(LDY_absx, 4, AddressingMode.AbsoluteX, "LDY"),	INS(LDA_absx, 4, AddressingMode.AbsoluteX, "LDA"),	INS(LDX_absy, 4, AddressingMode.AbsoluteY, "LDX"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* C */	INS(CPY_imm, 2, AddressingMode.Immediate, "CPY"),	INS(CMP_xind, 6, AddressingMode.XIndexed, "CMP"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(CPY_zp, 3, AddressingMode.ZeroPage, "CPY"),		INS(CMP_zp, 3, AddressingMode.ZeroPage, "CMP"),		INS(DEC_zp, 5, AddressingMode.ZeroPage, "DEC"),		INS(___, 0, AddressingMode.Illegal, "___"),     INS(INY_imp, 2, AddressingMode.Implied, "INY"),		INS(CMP_imm, 2, AddressingMode.Immediate, "CMP"),	INS(DEX_imp, 2, AddressingMode.Implied, "DEX"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(CPY_abs, 4, AddressingMode.Absolute, "CPY"),	INS(CMP_abs, 4, AddressingMode.Absolute, "CMP"),	INS(DEC_abs, 6, AddressingMode.Absolute, "DEC"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* D */	INS(BNE_rel, 2, AddressingMode.Relative, "BNE"),	INS(CMP_indy, 5, AddressingMode.IndexedY, "CMP"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(CMP_zpx, 4, AddressingMode.ZeroPageX, "CMP"),	INS(DEC_zpx, 6, AddressingMode.ZeroPageX, "DEC"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(CLD_imp, 2, AddressingMode.Implied, "CLD"),		INS(CMP_absy, 4, AddressingMode.AbsoluteY, "CMP"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(CMP_absx, 4, AddressingMode.AbsoluteX, "CMP"),	INS(DEC_absx, 7, AddressingMode.AbsoluteX, "DEC"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* E */	INS(CPX_imm, 2, AddressingMode.Immediate, "CPX"),	INS(SBC_xind, 6, AddressingMode.XIndexed, "SBC"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(CPX_zp, 3, AddressingMode.ZeroPage, "CPX"),		INS(SBC_zp, 3, AddressingMode.ZeroPage, "SBC"),		INS(INC_zp, 5, AddressingMode.ZeroPage, "INC"),		INS(___, 0, AddressingMode.Illegal, "___"),     INS(INX_imp, 2, AddressingMode.Implied, "INX"),		INS(SBC_imm, 2, AddressingMode.Immediate, "SBC"),	INS(NOP_imp, 2, AddressingMode.Implied, "NOP"),		INS(___, 0, AddressingMode.Illegal, "___"),		INS(CPX_abs, 4, AddressingMode.Absolute, "CPX"),	INS(SBC_abs, 4, AddressingMode.Absolute, "SBC"),	INS(INC_abs, 6, AddressingMode.Absolute, "INC"),	INS(___, 0, AddressingMode.Illegal, "___"),
				/* F */	INS(BEQ_rel, 2, AddressingMode.Relative, "BEQ"),	INS(SBC_indy, 5, AddressingMode.IndexedY, "SBC"),   INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(SBC_zpx, 4, AddressingMode.ZeroPageX, "SBC"),	INS(INC_zpx, 6, AddressingMode.ZeroPageX, "INC"),	INS(___, 0, AddressingMode.Illegal, "___"),     INS(SED_imp, 2, AddressingMode.Implied, "SED"),		INS(SBC_absy, 4, AddressingMode.AbsoluteY, "SBC"),	INS(___, 0, AddressingMode.Illegal, "___"),			INS(___, 0, AddressingMode.Illegal, "___"),		INS(___, 0, AddressingMode.Illegal, "___"),			INS(SBC_absx, 4, AddressingMode.AbsoluteX, "SBC"),	INS(INC_absx, 7, AddressingMode.AbsoluteX, "INC"),	INS(___, 0, AddressingMode.Illegal, "___"),
			};
		}

		public virtual void Start(ushort address)
		{
			this.PC = address;
		}

		public virtual void Run()
		{
			this.Cycles = 0;
			while (this.Step());
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

		protected virtual void Interrupt(ushort vector)
		{
			this.PushWord(this.PC);
			this.PushByte((byte)this.P);
			this.P |= StatusFlags.Interrupt;
			this.PC = GetWord(vector);
		}

		protected virtual bool Execute(byte instruction)
		{
			return this.Execute(this.instructions[instruction]);
		}

		protected virtual bool Execute(Instruction instruction)
		{
			var method = instruction.vector;

			method();
			this.Cycles += instruction.count;

			return true;
		}

		protected virtual void ___()
		{ }

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
			return Execute(FetchByte());
		}

		protected ushort PC
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

		protected abstract byte GetByte(ushort offset);
		protected abstract void SetByte(ushort offset, byte value);

		static ushort MakeWord(byte low, byte high)
		{
			return (ushort)((high << 8) + low);
		}

		protected void PushByte(byte value)
		{
			SetByte((ushort)(pageOne + S--), value);
		}

		protected byte PopByte()
		{
			return GetByte((ushort)(pageOne + ++S));
		}

		void PushWord(ushort value)
		{
			this.PushByte(HighByte(value));
			this.PushByte(LowByte(value));
		}

		ushort PopWord()
		{
			var low = this.PopByte();
			var high = this.PopByte();
			return MakeWord(low, high);
		}

		private static byte LowNybble(byte value)
		{
			return (byte)(value & 0xf);
		}

		private static byte HighNybble(byte value)
		{
			return (byte)(value >> 4);
		}

		static byte LowByte(ushort value)
		{
			return (byte)(value & 0xff);
		}

		static byte HighByte(ushort value)
		{
			return (byte)((value & ~0xff) >> 8);
		}

		private ushort GetWord(ushort offset)
		{
			var low = this.GetByte(offset);
			var high = this.GetByte((ushort)(offset + 1));
			return MakeWord(low, high);
		}

		byte FetchByte(ref ushort counter)
		{
			return this.GetByte(counter++);
		}

		ushort FetchWord(ref ushort counter)
		{
			var word = this.GetWord(counter);
			counter += 2;
			return word;
		}

		byte FetchByte()
		{
			return this.FetchByte(ref this.pc);
		}

		ushort FetchWord()
		{
			return this.FetchWord(ref this.pc);
		}

		//

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
			return this.GetByte(this.FetchByte());
		}

		private byte ReadByte_ZeroPageX()
		{
			return this.GetByte(LowByte((ushort)(this.FetchByte() + this.X)));
		}

		private byte ReadByte_Absolute()
		{
			return this.GetByte(this.FetchWord());
		}

		private byte ReadByte_AbsoluteX()
		{
			var address = this.FetchWord();
			var offset = (ushort)(address + X);
			if (LowByte(offset) == 0xff)
				++this.Cycles;
			return this.GetByte(offset);
		}

		private byte ReadByte_AbsoluteY()
		{
			var address = this.FetchWord();
			var offset = (ushort)(address + Y);
			if (LowByte(offset) == 0xff)
				++this.Cycles;
			return this.GetByte(offset);
		}

		private byte ReadByte_IndexedIndirectX()
		{
			return this.GetByte(this.GetWord(LowByte((ushort)(this.FetchByte() + X))));
		}

		private byte ReadByte_IndirectIndexedY()
		{
			var indirection = this.GetWord(this.FetchByte());
			if (LowByte(indirection) == 0xff)
				++this.Cycles;
			return this.GetByte((ushort)(indirection + Y));
		}

		//

		private void WriteByte_ZeroPage(byte value)
		{
			this.SetByte(this.FetchByte(), value);
		}

		private void WriteByte_Absolute(byte value)
		{
			this.SetByte(this.FetchWord(), value);
		}

		private void WriteByte_AbsoluteX(byte value)
		{
			this.SetByte((ushort)(this.FetchWord() + X), value);
		}

		private void WriteByte_AbsoluteY(byte value)
		{
			this.SetByte((ushort)(this.FetchWord() + Y), value);
		}

		private void WriteByte_ZeroPageX(byte value)
		{
			this.SetByte(LowByte((ushort)(this.FetchByte() + X)), value);
		}

		//

		private bool UpdateFlag_Zero(byte value)
		{
			if (value == 0)
			{
				this.P |= StatusFlags.Zero;
				return true;
			}

			return false;
		}

		private bool UpdateFlag_Negative(sbyte value)
		{
			if (value < 0)
			{
				this.P |= StatusFlags.Negative;
				return true;
			}

			return false;
		}

		private void UpdateFlags_ZeroNegative(byte value)
		{
			if (!this.UpdateFlag_Zero(value))
				this.UpdateFlag_Negative((sbyte)value);
		}

		private void ReflectFlags_ZeroNegative(byte value)
		{
			P &= ~(StatusFlags.Negative | StatusFlags.Zero);
			this.UpdateFlags_ZeroNegative(value);
		}

		//

		private void ROL(ushort offset)
		{
			this.SetByte(offset, this.ROL(this.GetByte(offset)));
		}

		private byte ROL(byte data)
		{
			var carry = (this.P & StatusFlags.Carry) != 0;

			this.P &= ~(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			if ((data & 0x80) != 0)
				this.P |= StatusFlags.Carry;

			var result = (byte)(data << 1);

			if (carry)
				result |= 0x01;

			this.UpdateFlags_ZeroNegative(result);

			return result;
		}

		private void ASL(ushort offset)
		{
			this.SetByte(offset, this.ASL(this.GetByte(offset)));
		}

		private byte ASL(byte data)
		{
			this.P &= ~(StatusFlags.Negative | StatusFlags.Zero| StatusFlags.Carry);

			byte result = (byte)(data << 1);
			this.UpdateFlags_ZeroNegative(result);

			if ((data & 0x80) != 0)
				this.P |= StatusFlags.Carry;

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
			if ((P & StatusFlags.Decimal) != 0)
				this.SBC_d(data);
			else
				this.SBC_b(data);
		}

		private void SBC_b(byte data)
		{
			var carry = (byte)((P & StatusFlags.Carry) == 0 ? 1 : 0);
			var difference = (ushort)(A - data - carry);

			this.P &= ~(StatusFlags.Zero | StatusFlags.Overflow | StatusFlags.Negative | StatusFlags.Carry);

			this.UpdateFlags_ZeroNegative((byte)difference);

			if (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0)
				this.P |= StatusFlags.Overflow;

			if ((difference & 0xff00) == 0)
				this.P |= StatusFlags.Carry;

			this.A = (byte)difference;
		}

		private void SBC_d(byte data)
		{
			var carry = (byte)((P & StatusFlags.Carry) == 0 ? 1 : 0);

			P &= ~(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero| StatusFlags.Carry);

			var difference = (ushort)(A - data - carry);

			var low = (byte)(LowNybble(A) - LowNybble(data) - carry);

			var lowNegative = (sbyte)low < 0;
			if (lowNegative)
				low -= 6;

			var high = (byte)(HighNybble(A) - HighNybble(data) - (lowNegative ? 1 : 0));

			this.UpdateFlags_ZeroNegative((byte)difference);

			if (((this.A ^ data) & (this.A ^ difference) & 0x80) != 0)
				this.P |= StatusFlags.Overflow;

			if ((difference & 0xff00) == 0)
				this.P |= StatusFlags.Carry;

			if ((sbyte)high < 0)
				high -= 6;

			this.A = (byte)((high << 4) | (low & 0x0f));
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
			this.P &= ~(StatusFlags.Negative | StatusFlags.Zero | StatusFlags.Carry);

			var result = (ushort)(first - second);

			this.UpdateFlags_ZeroNegative((byte)result);

			if ((result & 0xff00) == 0)
				this.P |= StatusFlags.Carry;
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
			if ((P & StatusFlags.Decimal) != 0)
				this.ADC_d(data);
			else
				this.ADC_b(data);
		}

		private void ADC_b(byte data)
		{
			var carry = (byte)((this.P & StatusFlags.Carry) == 0 ? 0 : 1);
			var sum = (ushort)(this.A + data + carry);
			this.P &= ~(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero | StatusFlags.Carry);

			this.UpdateFlags_ZeroNegative((byte)sum);

			if ((~(this.A ^ data) & (this.A ^ sum) & 0x80) != 0)
				this.P |= StatusFlags.Overflow;

			if (HighByte(sum) != 0)
				this.P |= StatusFlags.Carry;

			this.A = (byte)sum;
		}

		private void ADC_d(byte data)
		{
			var carry = (byte)((this.P & StatusFlags.Carry) == 0 ? 0 : 1);

			this.P &= ~(StatusFlags.Negative | StatusFlags.Overflow | StatusFlags.Zero | StatusFlags.Carry);

			var low = (byte)(LowNybble(A) + LowNybble(data) + carry);
			if (low > 9)
				low += 6;

			var high = (byte)(HighNybble(this.A) + HighNybble(data) + (low > 0x0f ? 1 : 0));

			if ((byte)(this.A + data + carry) == 0)
				this.P |= StatusFlags.Zero;
			else
				if ((high & 8) != 0)
					this.P |= StatusFlags.Negative;

			if ((~(this.A ^ data) & (this.A ^ (high << 4)) & 0x80) != 0)
				this.P |= StatusFlags.Overflow;

			if (high > 9)
				high += 6;
			if (high > 0x0f)
				this.P |= StatusFlags.Carry;

			this.A = (byte)((high << 4) | (low & 0x0f));
		}

		//

		private void Branch(sbyte displacement)
		{
			++this.Cycles;
			var oldPage = HighByte(this.PC);
			this.PC += (ushort)((short)displacement);
			var newPage = HighByte(this.PC);
			if (oldPage != newPage)
				this.Cycles += 2;
		}

		//

		//// Instructions...

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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

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
			throw new NotImplementedException();
		}

		private void LDY_zpx()
		{
			throw new NotImplementedException();
		}

		private void LDY_absx()
		{
			throw new NotImplementedException();
		}

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
			throw new NotImplementedException();
		}

		private void LDX_zpy()
		{
			throw new NotImplementedException();
		}

		private void LDX_absy()
		{
			throw new NotImplementedException();
		}

		private void CMP_absx()
		{
			throw new NotImplementedException();
		}

		private void CMP_absy()
		{
			throw new NotImplementedException();
		}

		private void CMP_zpx()
		{
			throw new NotImplementedException();
		}

		private void CMP_indy()
		{
			throw new NotImplementedException();
		}

		private void CMP_abs()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		private void CPY_imm()
		{
			this.CPY(this.ReadByte_Immediate());
		}

		private void CPY_zp()
		{
			throw new NotImplementedException();
		}

		private void CPY_abs()
		{
			throw new NotImplementedException();
		}

		private void CPX_abs()
		{
			throw new NotImplementedException();
		}

		private void CPX_zp()
		{
			throw new NotImplementedException();
		}

		private void CPX_imm()
		{
			this.CPX(this.ReadByte_Immediate());
		}

		private void DEC_absx()
		{
			throw new NotImplementedException();
		}

		private void DEC_zpx()
		{
			throw new NotImplementedException();
		}

		private void DEC_abs()
		{
			throw new NotImplementedException();
		}

		private void DEC_zp()
		{
			throw new NotImplementedException();
		}

		private void DEX_imp()
		{
			this.ReflectFlags_ZeroNegative(--this.X);
		}

		private void DEY_imp()
		{
			this.ReflectFlags_ZeroNegative(--this.Y);
		}

		private void INY_imp()
		{
			this.ReflectFlags_ZeroNegative(++this.Y);
		}

		private void INX_imp()
		{
			this.ReflectFlags_ZeroNegative(++this.X);
		}

		private void INC_zp()
		{
			throw new NotImplementedException();
		}

		private void INC_absx()
		{
			throw new NotImplementedException();
		}

		private void INC_zpx()
		{
			throw new NotImplementedException();
		}

		private void INC_abs()
		{
			throw new NotImplementedException();
		}

		private void STX_zpy()
		{
			throw new NotImplementedException();
		}

		private void STX_abs()
		{
			throw new NotImplementedException();
		}

		private void STX_zp()
		{
			this.WriteByte_ZeroPage(this.X);
		}

		private void STY_zpx()
		{
			throw new NotImplementedException();
		}

		private void STY_abs()
		{
			throw new NotImplementedException();
		}

		private void STY_zp()
		{
			this.WriteByte_ZeroPage(this.Y);
		}

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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		private void SBC_xind()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		private void SBC_zpx()
		{
			throw new NotImplementedException();
		}

		private void SBC_indy()
		{
			throw new NotImplementedException();
		}

		private void SBC_absx()
		{
			throw new NotImplementedException();
		}

		private void SBC_absy()
		{
			throw new NotImplementedException();
		}

		private void NOP_imp()
		{
			throw new NotImplementedException();
		}

		private void TSX_imp()
		{
			throw new NotImplementedException();
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

		private void PHP_imp()
		{
			this.P |= StatusFlags.Break;
			this.PushByte((byte)this.P);
		}

		private void PLP_imp()
		{
			throw new NotImplementedException();
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

		private void LSR_absx()
		{
			throw new NotImplementedException();
		}

		private void LSR_zpx()
		{
			throw new NotImplementedException();
		}

		private void LSR_abs()
		{
			throw new NotImplementedException();
		}

		private void LSR_imp()
		{
			throw new NotImplementedException();
		}

		private void LSR_zp()
		{
			throw new NotImplementedException();
		}

		private void EOR_absx()
		{
			throw new NotImplementedException();
		}

		private void EOR_absy()
		{
			throw new NotImplementedException();
		}

		private void EOR_zpx()
		{
			throw new NotImplementedException();
		}

		private void EOR_indy()
		{
			throw new NotImplementedException();
		}

		private void EOR_abs()
		{
			throw new NotImplementedException();
		}

		private void EOR_imm()
		{
			this.EOR(this.ReadByte_Immediate());
		}

		private void EOR_zp()
		{
			throw new NotImplementedException();
		}

		private void EOR_xind()
		{
			throw new NotImplementedException();
		}

		private void ROR_absx()
		{
			throw new NotImplementedException();
		}

		private void ROR_zpx()
		{
			throw new NotImplementedException();
		}

		private void ROR_abs()
		{
			throw new NotImplementedException();
		}

		private void ROR_imp()
		{
			throw new NotImplementedException();
		}

		private void ROR_zp()
		{
			throw new NotImplementedException();
		}

		private void ROL_absx()
		{
			throw new NotImplementedException();
		}

		private void ROL_zpx()
		{
			throw new NotImplementedException();
		}

		private void ROL_abs()
		{
			throw new NotImplementedException();
		}

		private void ROL_imp()
		{
			this.A = this.ROL(this.A);
		}

		private void ROL_zp()
		{
			this.ROL(this.ReadByte_ZeroPage());
		}

		private void BIT_zp()
		{
			throw new NotImplementedException();
		}

		private void BIT_abs()
		{
			throw new NotImplementedException();
		}

		private void AND_zpx()
		{
			throw new NotImplementedException();
		}

		private void AND_indy()
		{
			throw new NotImplementedException();
		}

		private void AND_zp()
		{
			throw new NotImplementedException();
		}

		private void AND_absx()
		{
			throw new NotImplementedException();
		}

		private void AND_absy()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		private void JSR_abs()
		{
			var destination = this.FetchWord();
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
			this.PC = this.FetchWord();
		}

		private void JMP_ind()
		{
			this.PC = this.GetWord(this.FetchWord());
		}

		private void BRK_imp()
		{
			this.PushWord((ushort)(PC + 1));
			this.PHP_imp();
			this.P |= StatusFlags.Interrupt;
			this.PC = this.GetWord(IRQvector);
		}

		private void ASL_imp()
		{
			throw new NotImplementedException();
		}

		private void ASL_zp()
		{
			this.ASL((ushort)this.FetchByte());
		}

		private void ASL_abs()
		{
			throw new NotImplementedException();
		}

		private void ASL_absx()
		{
			throw new NotImplementedException();
		}

		private void ASL_zpx()
		{
			throw new NotImplementedException();
		}

		private void ORA_xind()
		{
			throw new NotImplementedException();
		}

		private void ORA_zp()
		{
			throw new NotImplementedException();
		}

		private void ORA_imm()
		{
			this.ORA(this.ReadByte_Immediate());
		}

		private void ORA_abs()
		{
			throw new NotImplementedException();
		}

		private void ORA_absx()
		{
			throw new NotImplementedException();
		}

		private void ORA_absy()
		{
			throw new NotImplementedException();
		}

		private void ORA_zpx()
		{
			throw new NotImplementedException();
		}

		private void ORA_indy()
		{
			throw new NotImplementedException();
		}

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

		private void SED_imp()
		{
			this.P |= StatusFlags.Decimal;
		}

		private void CLD_imp()
		{
			this.P &= ~StatusFlags.Decimal;
		}

		private void CLV_imp()
		{
			this.P &= ~StatusFlags.Overflow;
		}

		private void SEI_imp()
		{
			this.P |= StatusFlags.Interrupt;
		}

		private void CLI_imp()
		{
			this.P &= ~StatusFlags.Interrupt;
		}

		private void CLC_imp()
		{
			this.P &= ~StatusFlags.Carry;
		}

		private void SEC_imp()
		{
			this.P |= StatusFlags.Carry;
		}

		private void BMI_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Negative) != 0)
				this.Branch(displacement);
		}

		private void BPL_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Negative) == 0)
				this.Branch(displacement);
		}

		private void BVC_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Overflow) == 0)
				this.Branch(displacement);
		}

		private void BVS_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Overflow) != 0)
				this.Branch(displacement);
		}

		private void BCC_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Carry) == 0)
				this.Branch(displacement);
		}

		private void BCS_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Carry) != 0)
				this.Branch(displacement);
		}

		private void BNE_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Zero) == 0)
				this.Branch(displacement);
		}

		private void BEQ_rel()
		{
			var displacement = this.ReadByte_ImmediateDisplacement();
			if ((P & StatusFlags.Zero) != 0)
				this.Branch(displacement);
		}
	}
}
