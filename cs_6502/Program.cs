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
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\sudoku.json");
#endif
#if TEST_SUITE1
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\test_suite_one.json");
#endif
#if TEST_SUITE2
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\test_suite_two.json");
#endif
#if TEST_SUITE_65C02
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\test_suite_65c02.json");
#endif
#if EHBASIC
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\ehbasic.json");
#endif
#if TALI_FORTH
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\tali.json");
#endif
#if BBC_FORTH
			var configuration = new Model.Configuration("C:\\github\\cs\\cs_6502\\bbc_forth.json");
#endif

			using (var controller = new Model.Controller(configuration))
			{
				controller.Configure();
				controller.Start();

				var hertz = controller.Speed * Processor.System6502.Mega;

				var cycles = controller.Processor.Cycles;
				var heldCycles = controller.Processor.HeldCycles;

				var start = controller.StartTime;
				var finish = controller.FinishTime;

				var elapsedTime = finish - start;
				var seconds = elapsedTime.TotalSeconds;
				var cyclesPerSecond = cycles / seconds;
				var simulatedElapsed = cycles / hertz;
				var speedup = cyclesPerSecond / hertz;

				var cycleDifference = cycles - heldCycles;
				var holdProportion = (double)cycles / cycleDifference;

				var hostHertz = controller.HostSpeed * Processor.System6502.Mega;
				var cyclesPerHostCycle = hostHertz / (cyclesPerSecond * holdProportion);

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
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nCycles per second {0:N}", cyclesPerSecond));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nSpeedup over {0:g}Mhz 6502 {1}", controller.Speed, speedup));

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\n\nSimulated cycles used {0:N}", cycles));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nHeld cycles {0:N}", heldCycles));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nHeld cycle difference {0:N}", cycleDifference));
				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nHeld proportion {0:g}", holdProportion));

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nCycles per host cycle (code efficiency!) {0:g}", cyclesPerHostCycle));

				Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "\nSimulated time taken {0}\n\n", simulatedElapsed));
			}
		}
	}
}
