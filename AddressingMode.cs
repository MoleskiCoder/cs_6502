﻿namespace Simulator
{
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
		Indirect,
        ZeroPageIndirect
	}
}
