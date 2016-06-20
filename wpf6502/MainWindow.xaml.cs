namespace WPF6502
{
	using System;
	using System.Windows;

	public partial class MainWindow : Window, IDisposable
	{
		private readonly ProcessorViewModel viewModel;
		private bool disposed = false;

		public MainWindow()
		:	this(null)
		{
		}

		public MainWindow(string configurationPath)
		{
			this.InitializeComponent();
			this.viewModel = new ProcessorViewModel(configurationPath);
			this.DataContext = this.viewModel;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.viewModel != null)
					{
						this.viewModel.Dispose();
					}
				}

				this.disposed = true;
			}
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
