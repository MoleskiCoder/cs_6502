#define SUDOKU_ASSEMBLE
////#define EHBASIC
////#define TEST_SUITE1
////#define TEST_SUITE2

namespace Simulator
{
	using System;
	using System.Globalization;

	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
#if SUDOKU_ASSEMBLE
			System6502 processor = new System6502(0xe000, 0xe001);
#else
			system6502 processor = new system6502(0xf004, 0xf001);
#endif

			processor.Clear();

			var start = DateTime.Now;

#if TEST_SUITE1
			processor.LoadRom("C:\\github\\cpp\\cpp_6502\\AllSuiteA.bin", 0x4000);
			processor.Start(0x4000);
#endif

#if TEST_SUITE2
			// Test suite two: https://github.com/Klaus2m5/6502_65C02_functional_tests
			processor.LoadRom("C:\\github\\cpp\\cpp_6502\\6502_functional_test.bin", 0x0);
			processor.Start(0x400);
#endif

#if EHBASIC
			processor.LoadRom("C:\\github\\cpp\\cpp_6502\\ehbasic.bin", 0xc000);
			processor.Reset();
#endif

#if SUDOKU_ASSEMBLE
			processor.LoadRom("C:\\github\\cpp\\cpp_6502\\sudoku.bin", 0xf000);
			processor.Reset();
#endif

			processor.Run();

			var finish = DateTime.Now;

			var elapsedTime = finish - start;
			var elapsed = elapsedTime.TotalMilliseconds;
			var seconds = (elapsed / 1000.0) + ((elapsed % 1000) / 1000.0);
			var cyclesPerSecond = processor.Cycles / (ulong)seconds;
			var speedup = cyclesPerSecond / 2000000.0;

			Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nCycles used {0}\n", processor.Cycles));
			Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nTime taken {0} seconds\n", seconds));
			Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nCycles per second {0}\n", cyclesPerSecond));
			Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nSpeedup over 2Mhz 6502 {0}\n", speedup));
		}
	}
}
