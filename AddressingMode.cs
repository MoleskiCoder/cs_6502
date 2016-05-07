﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
	public enum AddressingMode
	{
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
	};

}
