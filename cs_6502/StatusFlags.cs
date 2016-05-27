namespace Simulator
{
	using System;

	public class StatusFlags
	{
		public StatusFlags(byte value)
		{
			this.Negative = (value & (byte)StatusBits.Negative) != 0;
			this.Overflow = (value & (byte)StatusBits.Overflow) != 0;
			this.Reserved = (value & (byte)StatusBits.Reserved) != 0;
			this.Break = (value & (byte)StatusBits.Break) != 0;
			this.Decimal = (value & (byte)StatusBits.Decimal) != 0;
			this.Interrupt = (value & (byte)StatusBits.Interrupt) != 0;
			this.Zero = (value & (byte)StatusBits.Zero) != 0;
			this.Carry = (value & (byte)StatusBits.Carry) != 0;
		}

		[Flags]
		private enum StatusBits
		{
			Negative = 0x80,    // N
			Overflow = 0x40,    // V
			Reserved = 0x20,    // ignored
			Break = 0x10,       // B
			Decimal = 0x08,     // D (use BCD for arithmetic)
			Interrupt = 0x04,   // I (IRQ disable)
			Zero = 0x02,        // Z
			Carry = 0x01,       // C
		}

		public bool Negative { get; set; }

		public bool Overflow { get; set; }

		public bool Reserved { get; set; }

		public bool Break { get; set; }

		public bool Decimal { get; set; }

		public bool Interrupt { get; set; }

		public bool Zero { get; set; }

		public bool Carry { get; set; }

		public static implicit operator byte(StatusFlags current)
		{
			return current == null ? (byte)0x0 : current.ToByte();
		}

		public static implicit operator string(StatusFlags current)
		{
			return current == null ? null : current.ToString();
		}

		public override string ToString()
		{
			var returned = string.Empty;
			returned += this.Negative ? "N" : "-";
			returned += this.Overflow ? "O" : "-";
			returned += this.Reserved ? "R" : "-";
			returned += this.Break ? "B" : "-";
			returned += this.Decimal ? "D" : "-";
			returned += this.Interrupt ? "I" : "-";
			returned += this.Zero ? "Z" : "-";
			returned += this.Carry ? "C" : "-";
			return returned;
		}

		private byte ToByte()
		{
			StatusBits flags = 0;

			if (this.Negative)
			{
				flags |= StatusBits.Negative;
			}

			if (this.Overflow)
			{
				flags |= StatusBits.Overflow;
			}

			if (this.Reserved)
			{
				flags |= StatusBits.Reserved;
			}

			if (this.Break)
			{
				flags |= StatusBits.Break;
			}

			if (this.Decimal)
			{
				flags |= StatusBits.Decimal;
			}

			if (this.Interrupt)
			{
				flags |= StatusBits.Interrupt;
			}

			if (this.Zero)
			{
				flags |= StatusBits.Zero;
			}

			if (this.Carry)
			{
				flags |= StatusBits.Carry;
			}

			return (byte)flags;
		}
	}
}
