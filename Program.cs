#define SUDOKU_ASSEMBLE
//#define EHBASIC
//#define TEST_SUITE1
//#define TEST_SUITE2

using System;

namespace Simulator
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if SUDOKU_ASSEMBLE
			system6502 processor = new system6502(0xe000, 0xe001);
#else
			system6502 processor = new system6502(0xf004, 0xf001);
#endif

			processor.Clear();

			var start = DateTime.Now;

#if SUDOKU_ASSEMBLE
			processor.LoadRom("C:\\github\\cpp\\cpp_6502\\sudoku.bin", 0xf000);
#endif

			processor.Reset();

			processor.Run();

			var finish = DateTime.Now;

			var elapsedTime = finish - start;

			/*
			std::cout << std::endl << std::endl << "Cycles used " << processor.getCycles() << std::endl;

			auto seconds = (elapsed % CLOCKS_PER_SEC) / double(CLOCKS_PER_SEC) + (elapsed / CLOCKS_PER_SEC);
			std::cout << std::endl << std::endl << "Time taken " << seconds << std::endl;

			auto cyclesPerSecond = processor.getCycles() / seconds;
			std::cout << std::endl << std::endl << "Cycles per second " << cyclesPerSecond << std::endl;

			auto speedup = cyclesPerSecond / 2000000;
			std::cout << std::endl << std::endl << "Speedup over 2Mhz 6502 " << speedup << std::endl;
			*/
		}
	}
}
