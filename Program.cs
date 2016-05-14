// Test suite one: https://github.com/pmonta/FPGA-netlist-tools/tree/master/6502-test-code
// Test suite two: https://github.com/Klaus2m5/6502_65C02_functional_tests

////#define SUDOKU
////#define TEST_SUITE1
#define TEST_SUITE2
////#define EHBASIC

namespace Simulator
{
	using System;
	using System.Globalization;

	public static class Program
	{
		[STAThread]
		public static void Main()
		{
#if SUDOKU
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\sudoku.json");
#endif
#if TEST_SUITE1
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\test_suite_one.json");
#endif
#if TEST_SUITE2
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\test_suite_two.json");
#endif
#if EHBASIC
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\ehbasic.json");
#endif

			using (var controller = new Controller(configuration))
			{
				controller.Configure();
				controller.Start();

				var cycles = controller.Processor.Cycles;
				var start = controller.StartTime;
				var finish = controller.FinishTime;

				var elapsedTime = finish - start;
				var elapsed = elapsedTime.TotalMilliseconds;
				var seconds = (elapsed / 1000.0) + ((elapsed % 1000) / 1000.0);
				var cyclesPerSecond = cycles / seconds;
				var speedup = cyclesPerSecond / 2000000.0;

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nCycles used {0}\n", cycles));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nTime taken {0} seconds\n", seconds));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nCycles per second {0}\n", cyclesPerSecond));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nSpeedup over 2Mhz 6502 {0}\n", speedup));
			}
		}
	}
}
