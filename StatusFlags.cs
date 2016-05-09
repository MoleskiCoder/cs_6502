namespace Simulator
{
	using System;

	[Flags]
	public enum StatusFlags
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
}
