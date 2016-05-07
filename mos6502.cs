using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public mos6502()
		{
			this.instructions = new List<Instruction>
			{
				// 0x0x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x1x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x2x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x3x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x4x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x5x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x6x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x7x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x8x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0x9x
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xax
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xbx
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xcx
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xdx
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xex
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

				// 0xfx
				new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },

                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
                new Instruction() { vector = BRK_implied, count = 7, display = "BRK", mode = AddressingMode.Implied },
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
            var details = this.instructions[instruction];

            var method = details.vector;
            var count = details.count;

            method();
            this.Cycles += count;

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

		uint PopWord()
		{
			var low = this.PopByte();
			var high = this.PopByte();
			return MakeWord(low, high);
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

		private void PHP_implied()
		{
			this.P |= StatusFlags.Break;
            this.PushByte((byte)this.P);
		}

		private void BRK_implied()
		{
            this.PushWord((ushort)(PC + 1));
            this.PHP_implied();
            this.P |= StatusFlags.Interrupt;
            this.PC = this.GetWord(IRQvector);
		}

		private List<Instruction> instructions;
	}
}

#if XXXX

/*
		uint16_t fetchWord_Indirect();

		//

		int8_t readByte_ImmediateDisplacement();

		uint8_t readByte_Immediate();
		uint8_t readByte_ZeroPage();
		uint8_t readByte_Absolute();
		uint8_t readByte_IndexedIndirectX();
		uint8_t readByte_IndirectIndexedY();
		uint8_t readByte_ZeroPageX();
		uint8_t readByte_ZeroPageY();
		uint8_t readByte_AbsoluteX();
		uint8_t readByte_AbsoluteY();

		void writeByte_ZeroPage(uint8_t value);
		void writeByte_Absolute(uint8_t value);
		void writeByte_IndexedIndirectX(uint8_t value);
		void writeByte_IndirectIndexedY(uint8_t value);
		void writeByte_ZeroPageX(uint8_t value);
		void writeByte_ZeroPageY(uint8_t value);
		void writeByte_AbsoluteX(uint8_t value);
		void writeByte_AbsoluteY(uint8_t value);




		private List<Instruction> instructions = new List<Instruction>
		{
			new Instruction() { vector = BRK_implied }
		};

*/

		//		0 					1					2					3					4					5					6					7					8					9					A					B					C					D					E					F
		/* 0 */	INS(BRK,imp, 7),    INS(ORA,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,zp, 4),     INS(ASL,zp, 5),     INS(_,_, 0),        INS(PHP,imp, 3),    INS(ORA,imm, 2),    INS(ASL,imp, 2),    INS(_,_, 0),        INS(_,_, 0),        INS(ORA,abs, 4),    INS(ASL,abs, 6),    INS(_,_, 0),
		/* 1 */	INS(BPL,rel, 2),    INS(ORA,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,zpx, 4),    INS(ASL,zpx, 6),    INS(_,_, 0),        INS(CLC,imp, 2),    INS(ORA,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,absx, 4),   INS(ASL,absx, 7),   INS(_,_, 0),
		/* 2 */	INS(JSR,abs, 6),    INS(AND,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(BIT,zp, 3),     INS(AND,zp, 3),     INS(ROL,zp, 5),     INS(_,_, 0),        INS(PLP,imp, 4),    INS(AND,imm, 2),    INS(ROL,imp, 2),    INS(_,_, 0),        INS(BIT,abs, 4),    INS(AND,abs, 4),    INS(ROL,abs, 6),    INS(_,_, 0),
		/* 3 */	INS(BMI,rel, 2),    INS(AND,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(AND,zpx, 4),    INS(ROL,zpx, 6),    INS(_,_, 0),        INS(SEC,imp, 2),    INS(AND,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(AND,absx, 4),   INS(ROL,absx, 7),   INS(_,_, 0),
		/* 4 */	INS(RTI,imp, 6),    INS(EOR,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,zp, 3),     INS(LSR,zp, 5),     INS(_,_, 0),        INS(PHA,imp, 3),    INS(EOR,imm, 2),    INS(LSR,imp, 2),    INS(_,_, 0),        INS(JMP,abs, 3),    INS(EOR,abs, 4),    INS(LSR,abs, 6),    INS(_,_, 0),
		/* 5 */	INS(BVC,rel, 2),    INS(EOR,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,zpx, 4),    INS(LSR,zpx, 6),    INS(_,_, 0),        INS(CLI,imp, 2),    INS(EOR,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,absx, 4),   INS(LSR,absx, 7),   INS(_,_, 0),
		/* 6 */	INS(RTS,imp, 6),    INS(ADC,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,zp, 3),     INS(ROR,zp, 5),     INS(_,_, 0),        INS(PLA,imp, 4),    INS(ADC,imm, 2),    INS(ROR,imp, 2),    INS(_,_, 0),        INS(JMP,ind, 5),    INS(ADC,abs, 4),    INS(ROR,abs, 6),    INS(_,_, 0),
		/* 7 */	INS(BVS,rel, 2),    INS(ADC,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,zpx, 4),    INS(ROR,zpx, 6),    INS(_,_, 0),        INS(SEI,imp, 2),    INS(ADC,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,absx, 4),   INS(ROR,absx, 7),   INS(_,_, 0),
		/* 8 */	INS(_,_, 0),        INS(STA,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(STY,zp, 3),     INS(STA,zp, 3),     INS(STX,zp, 3),     INS(_,_, 0),        INS(DEY,imp, 2),    INS(_,_, 0),        INS(TXA,imp, 2),    INS(_,_, 0),        INS(STY,abs, 4),    INS(STA,abs, 4),    INS(STX,abs, 4),    INS(_,_, 0),
		/* 9 */	INS(BCC,rel, 2),    INS(STA,indy, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(STY,zpx, 4),    INS(STA,zpx, 4),    INS(STX,zpy, 4),    INS(_,_, 0),        INS(TYA,imp, 2),    INS(STA,absy, 5),   INS(TXS,imp, 2),    INS(_,_, 0),        INS(_,_, 0),        INS(STA,absx, 5),   INS(_,_, 0),        INS(_,_, 0),
		/* A */	INS(LDY,imm, 2),    INS(LDA,xind, 6),   INS(LDX,imm, 2),    INS(_,_, 0),        INS(LDY,zp, 3),     INS(LDA,zp, 3),     INS(LDX,zp, 3),     INS(_,_, 0),        INS(TAY,imp, 2),    INS(LDA,imm, 2),    INS(TAX,imp, 2),    INS(_,_, 0),        INS(LDY,abs, 4),    INS(LDA,abs, 4),    INS(LDX,abs, 4),    INS(_,_, 0),
		/* B */	INS(BCS,rel, 2),    INS(LDA,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(LDY,zpx, 4),    INS(LDA,zpx, 4),    INS(LDX,zpy, 4),    INS(_,_, 0),        INS(CLV,imp, 2),    INS(LDA,absy, 4),   INS(TSX,imp, 2),    INS(_,_, 0),        INS(LDY,absx, 4),   INS(LDA,absx, 4),   INS(LDX,absy, 4),   INS(_,_, 0),
		/* C */	INS(CPY,imm, 2),    INS(CMP,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(CPY,zp, 3),     INS(CMP,zp, 3),     INS(DEC,zp, 5),     INS(_,_, 0),        INS(INY,imp, 2),    INS(CMP,imm, 2),    INS(DEX,imp, 2),    INS(_,_, 0),        INS(CPY,abs, 4),    INS(CMP,abs, 4),    INS(DEC,abs, 6),    INS(_,_, 0),
		/* D */	INS(BNE,rel, 2),    INS(CMP,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(CMP,zpx, 4),    INS(DEC,zpx, 6),    INS(_,_, 0),        INS(CLD,imp, 2),    INS(CMP,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(CMP,absx, 4),   INS(DEC,absx, 7),   INS(_,_, 0),
		/* E */	INS(CPX,imm, 2),    INS(SBC,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(CPX,zp, 3),     INS(SBC,zp, 3),     INS(INC,zp, 5),     INS(_,_, 0),        INS(INX,imp, 2),    INS(SBC,imm, 2),    INS(NOP,imp, 2),    INS(_,_, 0),        INS(CPX,abs, 4),    INS(SBC,abs, 4),    INS(INC,abs, 6),    INS(_,_, 0),
		/* F */	INS(BEQ,rel, 2),    INS(SBC,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(SBC,zpx, 4),    INS(INC,zpx, 6),    INS(_,_, 0),        INS(SED,imp, 2),    INS(SBC,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(SBC,absx, 4),   INS(INC,absx, 7),   INS(_,_, 0),
	};




	}
}

/*
class mos6502
{

void dump_nothing() { }

void dump_bytevalue(uint8_t value) { printf("%02x", value); }
void dump_byte(uint16_t address) { dump_bytevalue(getByte(address)); }
void dump_byte() { dump_byte(PC); }
void dump_dbyte() { dump_byte(PC); dump_byte(PC + 1); }

void dump_imp() { }
void dump_a() { printf("A"); }
void dump_imm() { printf("#$%02x", getByte(PC)); }
void dump_abs() { printf("$%04x", getWord(PC)); }
void dump_zp() { printf("$%02x", getByte(PC)); }
void dump_zpx() { printf("$%02x,X", getByte(PC)); }
void dump_zpy() { printf("$%02x,Y", getByte(PC)); }
void dump_absx() { printf("$%04x,X", getWord(PC)); }
void dump_absy() { printf("$%04x,Y", getWord(PC)); }
void dump_xind() { printf("($%02x,X)", getByte(PC)); }
void dump_indy() { printf("($%02x),Y", getByte(PC)); }
void dump_ind() { printf("($%04x)", getWord(PC)); }
void dump_rel() { printf("$%04x", 1 + PC + (int8_t)getByte(PC)); }

std::map<addressing_mode, std::pair<instruction_t, instruction_t>> addressingMode_Dumper =
{
		DMP(imp, nothing),
		DMP(xind, byte),
		DMP(zp, byte),
		DMP(imm, byte),
		DMP(abs, dbyte),
		DMP(indy, byte),
		DMP(zpx, byte),
		DMP(zpy, byte),
		DMP(absx, dbyte),
		DMP(absy, dbyte),
		DMP(rel, byte),
		DMP(ind, dbyte),
	};

struct instruction
{
	instruction_t vector;
	unsigned count;
	addressing_mode mode;
	std::string display;
};

std::vector<instruction> instructions =
{
		//		0 					1					2					3					4					5					6					7					8					9					A					B					C					D					E					F
		/* 0 */	INS(BRK,imp, 7),    INS(ORA,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,zp, 4),     INS(ASL,zp, 5),     INS(_,_, 0),        INS(PHP,imp, 3),    INS(ORA,imm, 2),    INS(ASL,imp, 2),    INS(_,_, 0),        INS(_,_, 0),        INS(ORA,abs, 4),    INS(ASL,abs, 6),    INS(_,_, 0),
		/* 1 */	INS(BPL,rel, 2),    INS(ORA,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,zpx, 4),    INS(ASL,zpx, 6),    INS(_,_, 0),        INS(CLC,imp, 2),    INS(ORA,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ORA,absx, 4),   INS(ASL,absx, 7),   INS(_,_, 0),
		/* 2 */	INS(JSR,abs, 6),    INS(AND,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(BIT,zp, 3),     INS(AND,zp, 3),     INS(ROL,zp, 5),     INS(_,_, 0),        INS(PLP,imp, 4),    INS(AND,imm, 2),    INS(ROL,imp, 2),    INS(_,_, 0),        INS(BIT,abs, 4),    INS(AND,abs, 4),    INS(ROL,abs, 6),    INS(_,_, 0),
		/* 3 */	INS(BMI,rel, 2),    INS(AND,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(AND,zpx, 4),    INS(ROL,zpx, 6),    INS(_,_, 0),        INS(SEC,imp, 2),    INS(AND,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(AND,absx, 4),   INS(ROL,absx, 7),   INS(_,_, 0),
		/* 4 */	INS(RTI,imp, 6),    INS(EOR,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,zp, 3),     INS(LSR,zp, 5),     INS(_,_, 0),        INS(PHA,imp, 3),    INS(EOR,imm, 2),    INS(LSR,imp, 2),    INS(_,_, 0),        INS(JMP,abs, 3),    INS(EOR,abs, 4),    INS(LSR,abs, 6),    INS(_,_, 0),
		/* 5 */	INS(BVC,rel, 2),    INS(EOR,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,zpx, 4),    INS(LSR,zpx, 6),    INS(_,_, 0),        INS(CLI,imp, 2),    INS(EOR,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(EOR,absx, 4),   INS(LSR,absx, 7),   INS(_,_, 0),
		/* 6 */	INS(RTS,imp, 6),    INS(ADC,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,zp, 3),     INS(ROR,zp, 5),     INS(_,_, 0),        INS(PLA,imp, 4),    INS(ADC,imm, 2),    INS(ROR,imp, 2),    INS(_,_, 0),        INS(JMP,ind, 5),    INS(ADC,abs, 4),    INS(ROR,abs, 6),    INS(_,_, 0),
		/* 7 */	INS(BVS,rel, 2),    INS(ADC,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,zpx, 4),    INS(ROR,zpx, 6),    INS(_,_, 0),        INS(SEI,imp, 2),    INS(ADC,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(ADC,absx, 4),   INS(ROR,absx, 7),   INS(_,_, 0),
		/* 8 */	INS(_,_, 0),        INS(STA,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(STY,zp, 3),     INS(STA,zp, 3),     INS(STX,zp, 3),     INS(_,_, 0),        INS(DEY,imp, 2),    INS(_,_, 0),        INS(TXA,imp, 2),    INS(_,_, 0),        INS(STY,abs, 4),    INS(STA,abs, 4),    INS(STX,abs, 4),    INS(_,_, 0),
		/* 9 */	INS(BCC,rel, 2),    INS(STA,indy, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(STY,zpx, 4),    INS(STA,zpx, 4),    INS(STX,zpy, 4),    INS(_,_, 0),        INS(TYA,imp, 2),    INS(STA,absy, 5),   INS(TXS,imp, 2),    INS(_,_, 0),        INS(_,_, 0),        INS(STA,absx, 5),   INS(_,_, 0),        INS(_,_, 0),
		/* A */	INS(LDY,imm, 2),    INS(LDA,xind, 6),   INS(LDX,imm, 2),    INS(_,_, 0),        INS(LDY,zp, 3),     INS(LDA,zp, 3),     INS(LDX,zp, 3),     INS(_,_, 0),        INS(TAY,imp, 2),    INS(LDA,imm, 2),    INS(TAX,imp, 2),    INS(_,_, 0),        INS(LDY,abs, 4),    INS(LDA,abs, 4),    INS(LDX,abs, 4),    INS(_,_, 0),
		/* B */	INS(BCS,rel, 2),    INS(LDA,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(LDY,zpx, 4),    INS(LDA,zpx, 4),    INS(LDX,zpy, 4),    INS(_,_, 0),        INS(CLV,imp, 2),    INS(LDA,absy, 4),   INS(TSX,imp, 2),    INS(_,_, 0),        INS(LDY,absx, 4),   INS(LDA,absx, 4),   INS(LDX,absy, 4),   INS(_,_, 0),
		/* C */	INS(CPY,imm, 2),    INS(CMP,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(CPY,zp, 3),     INS(CMP,zp, 3),     INS(DEC,zp, 5),     INS(_,_, 0),        INS(INY,imp, 2),    INS(CMP,imm, 2),    INS(DEX,imp, 2),    INS(_,_, 0),        INS(CPY,abs, 4),    INS(CMP,abs, 4),    INS(DEC,abs, 6),    INS(_,_, 0),
		/* D */	INS(BNE,rel, 2),    INS(CMP,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(CMP,zpx, 4),    INS(DEC,zpx, 6),    INS(_,_, 0),        INS(CLD,imp, 2),    INS(CMP,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(CMP,absx, 4),   INS(DEC,absx, 7),   INS(_,_, 0),
		/* E */	INS(CPX,imm, 2),    INS(SBC,xind, 6),   INS(_,_, 0),        INS(_,_, 0),        INS(CPX,zp, 3),     INS(SBC,zp, 3),     INS(INC,zp, 5),     INS(_,_, 0),        INS(INX,imp, 2),    INS(SBC,imm, 2),    INS(NOP,imp, 2),    INS(_,_, 0),        INS(CPX,abs, 4),    INS(SBC,abs, 4),    INS(INC,abs, 6),    INS(_,_, 0),
		/* F */	INS(BEQ,rel, 2),    INS(SBC,indy, 5),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(SBC,zpx, 4),    INS(INC,zpx, 6),    INS(_,_, 0),        INS(SED,imp, 2),    INS(SBC,absy, 4),   INS(_,_, 0),        INS(_,_, 0),        INS(_,_, 0),        INS(SBC,absx, 4),   INS(INC,absx, 7),   INS(_,_, 0),
	};


	BRANCH_DECLARATION(BCS)

	BRANCH_DECLARATION(BCC)


	BRANCH_DECLARATION(BMI)

	BRANCH_DECLARATION(BPL)


	BRANCH_DECLARATION(BEQ)

	BRANCH_DECLARATION(BNE)


	BRANCH_DECLARATION(BVS)

	BRANCH_DECLARATION(BVC)


	void branch(int8_t displacement);

void NOP_imp();

void CLV_imp();
void SEI_imp();
void CLI_imp();
void SED_imp();
void CLD_imp();
void SEC_imp();
void CLC_imp();

void BIT(uint8_t data);
void BIT_abs();
void BIT_zp();

void JMP_abs();
void JMP_ind();


	READER_GROUP_A_DECLARATIONS(ORA)

	READER_GROUP_A_DECLARATIONS(AND)

	READER_GROUP_A_DECLARATIONS(EOR)

	READER_GROUP_A_DECLARATIONS(ADC)

	READER_GROUP_A_DECLARATIONS(LDA)

	READER_GROUP_A_DECLARATIONS(CMP)

	READER_GROUP_A_DECLARATIONS(SBC)


	READER_GROUP_X_DECLARATIONS(LDX)

	READER_GROUP_Y_DECLARATIONS(LDY)


	READER_GROUP_CPXY_DECLARATIONS(CPX)

	READER_GROUP_CPXY_DECLARATIONS(CPY)


	WRITER_GROUP_A_DECLARATIONS(STA)

	WRITER_GROUP_X_DECLARATIONS(STX)

	WRITER_GROUP_Y_DECLARATIONS(STY)


	INCDEC_GROUP_A_DECLARATIONS(INC)

	INCDEC_GROUP_A_DECLARATIONS(DEC)


	ROTATION_GROUP_DECLARATIONS(ASL)

	ROTATION_GROUP_DECLARATIONS(ROL)

	ROTATION_GROUP_DECLARATIONS(LSR)

	ROTATION_GROUP_DECLARATIONS(ROR)


	void CMP(uint8_t first, uint8_t second);

void ADC(uint8_t data);
void ADC_d(uint8_t data);
void ADC_b(uint8_t data);

void SBC(uint8_t data);
void SBC_d(uint8_t data);
void SBC_b(uint8_t data);

void CPX(uint8_t data);
void CPY(uint8_t data);
void LDY(uint8_t data);
void ORA(uint8_t data);
void AND(uint8_t data);
void EOR(uint8_t data);
void LDA(uint8_t data);
void CMP(uint8_t data);
void LDX(uint8_t data);
void DEC(uint16_t offset);
void INC(uint16_t offset);

void JSR_abs();
void RTS_imp();
void PHA_imp();
void PLA_imp();
void PHP_imp();
void PLP_imp();
void DEX_imp();
void DEY_imp();
void INX_imp();
void INY_imp();
void TAX_imp();
void TXA_imp();
void TAY_imp();
void TYA_imp();
void TXS_imp();
void TSX_imp();
void BRK_imp();
void RTI_imp();

void ASL(uint16_t offset);
void ROL(uint16_t offset);
void LSR(uint16_t offset);
void ROR(uint16_t offset);

uint8_t ASL(uint8_t data);
uint8_t ROL(uint8_t data);
uint8_t LSR(uint8_t data);
uint8_t ROR(uint8_t data);

// get/set memory

virtual uint8_t getByte(uint16_t offset) = 0;
	virtual void setByte(uint16_t offset, uint8_t value) = 0;

	uint16_t getWord(uint16_t offset);

// Fetches increment their reference counter

uint8_t fetchByte(uint16_t& counter);
uint16_t fetchWord(uint16_t& counter);

// Fetches without a reference counter increment PC

uint8_t fetchByte();

uint16_t fetchWord();
uint16_t fetchWord_Indirect();

//

int8_t readByte_ImmediateDisplacement();

uint8_t readByte_Immediate();
uint8_t readByte_ZeroPage();
uint8_t readByte_Absolute();
uint8_t readByte_IndexedIndirectX();
uint8_t readByte_IndirectIndexedY();
uint8_t readByte_ZeroPageX();
uint8_t readByte_ZeroPageY();
uint8_t readByte_AbsoluteX();
uint8_t readByte_AbsoluteY();

void writeByte_ZeroPage(uint8_t value);
void writeByte_Absolute(uint8_t value);
void writeByte_IndexedIndirectX(uint8_t value);
void writeByte_IndirectIndexedY(uint8_t value);
void writeByte_ZeroPageX(uint8_t value);
void writeByte_ZeroPageY(uint8_t value);
void writeByte_AbsoluteX(uint8_t value);
void writeByte_AbsoluteY(uint8_t value);

//

bool updateFlag_Zero(uint8_t value)
{
	return !value ? P |= F_Z, true : false;
}

bool updateFlag_Negative(int8_t value)
{
	return value < 0 ? P |= F_N, true : false;
}

void updateFlags_ZeroNegative(uint8_t value)
{
	if (!updateFlag_Zero(value))
		updateFlag_Negative(value);
}

void reflectFlags_ZeroNegative(uint8_t value)
{
	P &= ~(F_N | F_Z);
	updateFlags_ZeroNegative(value);
}

//

static uint8_t lowByte(uint16_t value)
{
	return value & 0xff;
}

static uint8_t highByte(uint16_t value)
{
	return (value & ~0xff) >> 8;
}

static uint16_t makeWord(uint8_t low, uint8_t high)
{
	return (high << 8) + low;
}

//

void pushByte(uint8_t value)
{
	setByte(page_1 + S--, value);
}

uint8_t popByte()
{
	return getByte(page_1 + ++S);
}

void pushWord(uint16_t value)
{
	pushByte(highByte(value));
	pushByte(lowByte(value));
}

uint16_t popWord()
{
	auto low = popByte();
	auto high = popByte();
	return makeWord(low, high);
}
};
#endif