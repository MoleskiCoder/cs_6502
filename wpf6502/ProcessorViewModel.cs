namespace WPF6502
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;

	public class ProcessorViewModel : INotifyPropertyChanged, IDisposable
	{
		private readonly System.Timers.Timer timer;

		private string configurationPath;
		private Model.Configuration configuration;
		private Model.Controller controller;
		private Task task;

		private bool disposed = false;

		public ProcessorViewModel()
		:	this(null)
		{
		}

		public ProcessorViewModel(string configurationPath)
		{
			this.configurationPath = configurationPath;

			this.timer = new System.Timers.Timer();
			this.timer.Interval = 10;  // 1/100 second
			this.timer.Elapsed += this.Timer_Elapsed;
			this.timer.Start();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string ConfigurationPath
		{
			get
			{
				this.CheckDisposed();
				return this.configurationPath;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.configurationPath)
				{
					this.configurationPath = value;
					this.OnPropertyChanged("ConfigurationPath");
				}
			}
		}

		public double Hertz
		{
			get
			{
				this.CheckDisposed();
				return this.controller.Speed * Processor.System6502.Mega;
			}
		}

		public ulong Cycles
		{
			get
			{
				this.CheckDisposed();
				return this.controller.Processor.Cycles;
			}
		}

		public ulong HeldCycles
		{
			get
			{
				this.CheckDisposed();
				return this.controller.Processor.HeldCycles;
			}
		}

		public DateTime StartTime
		{
			get
			{
				this.CheckDisposed();
				return this.controller.StartTime;
			}
		}

		public DateTime FinishTime
		{
			get
			{
				this.CheckDisposed();
				var running = this.controller.Processor.Proceed;
				return running ? DateTime.UtcNow : this.controller.FinishTime;
			}
		}

		public TimeSpan ElapsedTime
		{
			get
			{
				this.CheckDisposed();
				return this.FinishTime - this.StartTime;
			}
		}

		public double ElapsedSeconds
		{
			get
			{
				this.CheckDisposed();
				return this.ElapsedTime.TotalSeconds;
			}
		}

		public double CyclesPerSecond
		{
			get
			{
				this.CheckDisposed();
				return this.Cycles / this.ElapsedSeconds;
			}
		}

		public double SimulatedElapsed
		{
			get
			{
				this.CheckDisposed();
				return this.Cycles / this.Hertz;
			}
		}

		public double Speedup
		{
			get
			{
				this.CheckDisposed();
				return this.CyclesPerSecond / this.Hertz;
			}
		}

		public ulong CycleDifference
		{
			get
			{
				this.CheckDisposed();
				return this.Cycles - this.HeldCycles;
			}
		}

		public double HoldProportion
		{
			get
			{
				this.CheckDisposed();
				return (double)this.Cycles / this.CycleDifference;
			}
		}

		public ICommand StartCommand
		{
			get
			{
				return new RelayCommand<Window>(window =>
				{
					this.Start();
				});
			}
		}

		public ICommand StopCommand
		{
			get
			{
				return new RelayCommand<Window>(window =>
				{
					this.controller.Processor.Proceed = false;
				});
			}
		}

		public byte A
		{
			get
			{
				return this.controller.Processor.A;
			}
		}

		public byte X
		{
			get
			{
				return this.controller.Processor.X;
			}
		}

		public byte Y
		{
			get
			{
				return this.controller.Processor.Y;
			}
		}

		public byte S
		{
			get
			{
				return this.controller.Processor.S;
			}
		}

		public ushort PC
		{
			get
			{
				return this.controller.Processor.PC;
			}
		}

		public Processor.StatusFlags P
		{
			get
			{
				return this.controller.Processor.P;
			}
		}

		public void Start()
		{
			this.CheckDisposed();

			this.configuration = new Model.Configuration(this.configurationPath);
			this.controller = new Model.Controller(this.configuration);
			this.controller.Configure();

			this.task = Task.Run(() =>
			{
				this.controller.Start();
			});
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void OnPropertyChanged(string property)
		{
			this.CheckDisposed();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.task != null)
					{
						this.task.Dispose();
					}

					if (this.controller != null)
					{
						this.controller.Dispose();
					}
				}

				this.disposed = true;
			}
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.OnPropertyChanged("A");
			this.OnPropertyChanged("X");
			this.OnPropertyChanged("Y");
			this.OnPropertyChanged("S");
			this.OnPropertyChanged("PC");
			this.OnPropertyChanged("P");

			this.OnPropertyChanged("Speedup");
			this.OnPropertyChanged("HoldProportion");
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