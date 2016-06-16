// Test suite one: https://github.com/pmonta/FPGA-netlist-tools/tree/master/6502-test-code
// Test suite two/65c02: https://github.com/Klaus2m5/6502_65C02_functional_tests

#define SUDOKU
////#define TEST_SUITE1
////#define TEST_SUITE2
////#define TEST_SUITE_65C02
////#define EHBASIC
////#define TALI_FORTH
////#define BBC_FORTH

namespace WPF6502
{
	using System.Windows;

	public partial class App : Application
	{
		public void App_Startup(object sender, StartupEventArgs e)
		{
#if SUDOKU
			var configuration = "C:\\github\\cs\\cs_6502\\sudoku.json";
#endif
#if TEST_SUITE1
			var configuration = "C:\\github\\cs\\cs_6502\\test_suite_one.json";
#endif
#if TEST_SUITE2
			var configuration = "C:\\github\\cs\\cs_6502\\test_suite_two.json";
#endif
#if TEST_SUITE_65C02
			var configuration = "C:\\github\\cs\\cs_6502\\test_suite_65c02.json";
#endif
#if EHBASIC
			var configuration = "C:\\github\\cs\\cs_6502\\ehbasic.json";
#endif
#if TALI_FORTH
			var configuration = "C:\\github\\cs\\cs_6502\\tali.json";
#endif
#if BBC_FORTH
			var configuration = "C:\\github\\cs\\cs_6502\\bbc_forth.json";
#endif

			var window = new MainWindow(configuration);
			window.Show();


		}
	}
}
