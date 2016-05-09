namespace Simulator
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public enum AddressingMode
	{
		Illegal,
		Implied,
		Immediate,
		Relative,
		XIndexed,
		IndexedY,
		ZeroPage,
		ZeroPageX,
		ZeroPageY,
		Absolute,
		AbsoluteX,
		AbsoluteY,
		Indirect
	}
}
