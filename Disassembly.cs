namespace Simulator
{
	using System.Collections.Generic;
	using System.Globalization;

	public class Disassembly
	{
		private MOS6502 processor;
		private Symbols symbols;

		private Dictionary<AddressingMode, AddressingModeDumper> dumpers;

		////

		public Disassembly(MOS6502 processor, Symbols symbols)
		{
			this.processor = processor;
			this.symbols = symbols;

			this.dumpers = new Dictionary<AddressingMode, AddressingModeDumper>()
			{
				{ AddressingMode.Illegal, new AddressingModeDumper { ByteDumper = this.Dump_Nothing, DisassemblyDumper = this.Dump_Nothing } },
				{ AddressingMode.Implied, new AddressingModeDumper { ByteDumper = this.Dump_Nothing, DisassemblyDumper = this.Dump_Nothing } },
				{ AddressingMode.Accumulator, new AddressingModeDumper { ByteDumper = this.Dump_Nothing, DisassemblyDumper = this.Dump_A } },
				{ AddressingMode.Immediate, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_imm } },
				{ AddressingMode.Relative, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_rel } },
				{ AddressingMode.XIndexed, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_xind } },
				{ AddressingMode.IndexedY, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_indy } },
				{ AddressingMode.ZeroPageIndirect, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zpind } },
				{ AddressingMode.ZeroPage, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zp } },
				{ AddressingMode.ZeroPageX, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zpx } },
				{ AddressingMode.ZeroPageY, new AddressingModeDumper { ByteDumper = this.Dump_Byte, DisassemblyDumper = this.Dump_zpy } },
				{ AddressingMode.Absolute, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_abs } },
				{ AddressingMode.AbsoluteX, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_absx } },
				{ AddressingMode.AbsoluteY, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_absy } },
				{ AddressingMode.AbsoluteXIndirect, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_absxind } },
				{ AddressingMode.Indirect, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_ind } },
				{ AddressingMode.ZeroPageRelative, new AddressingModeDumper { ByteDumper = this.Dump_DByte, DisassemblyDumper = this.Dump_zprel } },
			};
		}

		public string Dump_ByteValue(byte value)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0:x2}", value);
		}
	
		////

		public string DumpBytes(AddressingMode mode, ushort current)
		{
			return this.dumpers[mode].ByteDumper(current);
		}

        public string Disassemble(ushort current)
        {
            var content = this.processor.GetByte(current);
            var instruction = this.processor.Instructions[content];

            var mode = instruction.Mode;
            var mnemomic = instruction.Display;

            var operand = this.DumpOperand(mode, (ushort)(current + 1));

            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", mnemomic, operand);
        }

		public string DumpOperand(AddressingMode mode, ushort current)
		{
			return this.dumpers[mode].DisassemblyDumper(current);
		}

		////

		private byte GetByte(ushort address)
		{
			return this.processor.GetByte(address);
		}

		private ushort GetWord(ushort address)
		{
			return this.processor.GetWord(address);
		}

		////

		private string Dump_Nothing(ushort unused)
		{
			return string.Empty;
		}

		private string Dump_Byte(ushort address)
		{
			return this.Dump_ByteValue(this.GetByte(address));
		}

		private string Dump_DByte(ushort address)
		{
			return this.Dump_Byte(address) + this.Dump_Byte((ushort)(address + 1));
		}

		////

		private string ConvertAddress(ushort address)
		{
			string label;
			if (this.symbols.Labels.TryGetValue(address, out label))
			{
				return label;
			}

			return string.Format(CultureInfo.InvariantCulture, "${0:x4}", address);
		}

		private string ConvertAddress(byte address)
		{
			string label;
			if (this.symbols.Labels.TryGetValue(address, out label))
			{
				return label;
			}

			return string.Format(CultureInfo.InvariantCulture, "${0:x2}", address);
		}

		private string ConvertConstant(ushort constant)
		{
			string label;
			if (this.symbols.Constants.TryGetValue(constant, out label))
			{
				return label;
			}

			return string.Format(CultureInfo.InvariantCulture, "${0:x4}", constant);
		}

		private string ConvertConstant(byte constant)
		{
			string label;
			if (this.symbols.Constants.TryGetValue(constant, out label))
			{
				return label;
			}

			return string.Format(CultureInfo.InvariantCulture, "${0:x2}", constant);
		}

		////

		private string Dump_A(ushort unused)
		{
			return "A";
		}

		private string Dump_imm(ushort current)
		{
			var immediate = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "#{0}", this.ConvertConstant(immediate));
		}

		private string Dump_abs(ushort current)
		{
			var address = this.GetWord(current);
			return string.Format(CultureInfo.InvariantCulture, "{0}", this.ConvertAddress(address));
		}

		private string Dump_zp(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "{0}", this.ConvertAddress(zp));
		}

		private string Dump_zpx(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "{0},X", this.ConvertAddress(zp));
		}

		private string Dump_zpy(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "{0},Y", this.ConvertAddress(zp));
		}

		private string Dump_absx(ushort current)
		{
			var address = this.GetWord(current);
			return string.Format(CultureInfo.InvariantCulture, "{0},X", this.ConvertAddress(address));
		}

		private string Dump_absy(ushort current)
		{
			var address = this.GetWord(current);
			return string.Format(CultureInfo.InvariantCulture, "{0},Y", this.ConvertAddress(address));
		}

		private string Dump_absxind(ushort current)
		{
			var address = this.GetWord(current);
			return string.Format(CultureInfo.InvariantCulture, "({0},X)", this.ConvertAddress(address));
		}

		private string Dump_xind(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "({0},X)", this.ConvertAddress(zp));
		}

		private string Dump_indy(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "({0}),Y", this.ConvertAddress(zp));
		}

		private string Dump_ind(ushort current)
		{
			var address = this.GetWord(current);
			return string.Format(CultureInfo.InvariantCulture, "({0})", this.ConvertAddress(address));
		}

		private string Dump_zpind(ushort current)
		{
			var zp = this.GetByte(current);
			return string.Format(CultureInfo.InvariantCulture, "({0})", this.ConvertAddress(zp));
		}

		private string Dump_rel(ushort current)
		{
			var relative = (ushort)(1 + current + (sbyte)this.GetByte(current));
			return string.Format(CultureInfo.InvariantCulture, "{0}", this.ConvertAddress(relative));
		}

		private string Dump_zprel(ushort current)
		{
			var zp = this.GetByte(current);
			var displacement = (sbyte)this.GetByte((ushort)(current + 1));
			var address = (ushort)(1 + current + displacement);
			return string.Format(CultureInfo.InvariantCulture, "{0},{1}", this.ConvertAddress(zp), this.ConvertAddress(address));
		}
	}
}
