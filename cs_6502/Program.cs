// Test suite one: https://github.com/pmonta/FPGA-netlist-tools/tree/master/6502-test-code
// Test suite two/65c02: https://github.com/Klaus2m5/6502_65C02_functional_tests

#define SUDOKU
////#define TEST_SUITE1
////#define TEST_SUITE2
////#define TEST_SUITE_65C02
////#define EHBASIC
////#define TALI_FORTH
////#define BBC_FORTH

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
#if TEST_SUITE_65C02
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\test_suite_65c02.json");
#endif
#if EHBASIC
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\ehbasic.json");
#endif
#if TALI_FORTH
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\tali.json");
#endif
#if BBC_FORTH
			var configuration = new Configuration("C:\\github\\cs\\cs_6502\\bbc_forth.json");
#endif

			using (var controller = new Controller(configuration))
			{
				controller.Configure();
				controller.Start();

				var hertz = controller.Speed * 1000000.0;

				var cycles = controller.Processor.Cycles;
				var start = controller.StartTime;
				var finish = controller.FinishTime;

				var elapsedTime = finish - start;
				var elapsed = elapsedTime.TotalMilliseconds;
				var seconds = (elapsed / 1000.0) + ((elapsed % 1000) / 1000.0);
				var cyclesPerSecond = cycles / seconds;
				var simulatedElapsed = cycles / hertz;
				var speedup = cyclesPerSecond / hertz;

				System.Console.Out.WriteLine("\n** Stopped PC={0:x4}", controller.Processor.PC);

#if TEST_SUITE1
				var test = controller.Processor.GetByte(0x0210);
				if (test == 0xff)
				{
					System.Console.Out.WriteLine("\n** success!!");
				}
				else
				{
					System.Console.Out.WriteLine("\n** {0} failed!!", test);
				}
#endif

#if TEST_SUITE2
				var test = controller.Processor.GetByte(0x0200);
				System.Console.Out.WriteLine("\n**** Test={0:x2}", test);
#endif

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nTime taken {0} seconds", seconds));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nCycles per second {0}", cyclesPerSecond));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nSpeedup over 2Mhz 6502 {0}", speedup));

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nSimulated cycles used {0}", cycles));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nSimulated time taken {0}\n\n", simulatedElapsed));
			}
		}
	}
}
