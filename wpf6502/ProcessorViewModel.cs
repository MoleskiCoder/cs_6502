namespace WPF6502
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Input;

	public class ProcessorViewModel : INotifyPropertyChanged, IDisposable
	{
		private readonly System.Timers.Timer timer = new System.Timers.Timer();

		private string configurationPath;
		private Model.Configuration configuration;
		private Model.Controller controller;
		private Task task;

		private double speed;
		private DateTime startTime;
		private DateTime finishTime;

		private ulong cycles;
		private ulong heldCycles;
		private bool proceed;
		private bool running;

		private byte a;
		private byte x;
		private byte y;
		private byte s;
		private Processor.StatusFlags p;
		private ushort pc;

		private bool processingTimer = false;

		private bool disposed = false;

		public ProcessorViewModel()
		:	this(null)
		{
		}

		public ProcessorViewModel(string configurationPath)
		{
			this.configurationPath = configurationPath;

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

		#region Controller properties

		public double Speed
		{
			get
			{
				this.CheckDisposed();
				return this.speed;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.speed)
				{
					this.speed = value;
					this.OnPropertyChanged("Speed");
				}
			}
		}

		public DateTime StartTime
		{
			get
			{
				this.CheckDisposed();
				return this.startTime;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.startTime)
				{
					this.startTime = value;
					this.OnPropertyChanged("StartTime");
				}
			}
		}

		public DateTime FinishTime
		{
			get
			{
				this.CheckDisposed();
				return this.finishTime;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.finishTime)
				{
					this.finishTime = value;
					this.OnPropertyChanged("FinishTime");
				}
			}
		}

		#endregion

		#region Processor properties

		public ulong Cycles
		{
			get
			{
				this.CheckDisposed();
				return this.cycles;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.cycles)
				{
					this.cycles = value;
					this.OnPropertyChanged("Cycles");
				}
			}
		}

		public ulong HeldCycles
		{
			get
			{
				this.CheckDisposed();
				return this.heldCycles;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.heldCycles)
				{
					this.heldCycles = value;
					this.OnPropertyChanged("HeldCycles");
				}
			}
		}

		public bool Proceed
		{
			get
			{
				this.CheckDisposed();
				return this.proceed;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.proceed)
				{
					this.proceed = value;
					this.OnPropertyChanged("Proceed");
				}
			}
		}

		public bool Running
		{
			get
			{
				this.CheckDisposed();
				return this.running;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.running)
				{
					this.running = value;
					this.OnPropertyChanged("Running");
				}
			}
		}

		#region Registers

		public byte A
		{
			get
			{
				this.CheckDisposed();
				return this.a;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.a)
				{
					this.a = value;
					this.OnPropertyChanged("A");
				}
			}
		}

		public byte X
		{
			get
			{
				this.CheckDisposed();
				return this.x;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.x)
				{
					this.x = value;
					this.OnPropertyChanged("X");
				}
			}
		}

		public byte Y
		{
			get
			{
				this.CheckDisposed();
				return this.y;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.y)
				{
					this.y = value;
					this.OnPropertyChanged("Y");
				}
			}
		}

		public byte S
		{
			get
			{
				this.CheckDisposed();
				return this.s;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.s)
				{
					this.s = value;
					this.OnPropertyChanged("S");
				}
			}
		}

		public Processor.StatusFlags P
		{
			get
			{
				this.CheckDisposed();
				return this.p;
			}

			set
			{
				this.CheckDisposed();
				//if (value != this.p)
				{
					this.p = value;
					this.OnPropertyChanged("P");
				}
			}
		}

		public ushort PC
		{
			get
			{
				this.CheckDisposed();
				return this.pc;
			}

			set
			{
				this.CheckDisposed();
				if (value != this.pc)
				{
					this.pc = value;
					this.OnPropertyChanged("PC");
				}
			}
		}

		#endregion

		#endregion

		#region Derived properties

		public double Hertz
		{
			get
			{
				this.CheckDisposed();
				return this.Speed * Processor.System6502.Mega;
			}
		}

		public TimeSpan ElapsedTime
		{
			get
			{
				this.CheckDisposed();
				var start = this.StartTime;
				var stop = this.Proceed ? DateTime.UtcNow : this.FinishTime; 
				return stop - start;
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

		#endregion

		#region Commands

		public ICommand StartCommand
		{
			get
			{
				return new RelayCommand(
					() =>
					{
						this.Start();
					},
					() =>
					{
						return !this.Running;
					});
			}
		}

		public ICommand StopCommand
		{
			get
			{
				return new RelayCommand(
					() =>
					{
						this.controller.Processor.Proceed = false;
					},
					() =>
					{
						return this.Running;
					});
			}
		}

		#endregion

		public void Start()
		{
			this.CheckDisposed();

			this.configuration = new Model.Configuration(this.configurationPath);
			this.controller = new Model.Controller(this.configuration);
			this.controller.Configure();

			this.controller.Processor.Starting += this.Processor_Starting;
			this.controller.Processor.Finished += this.Processor_Finished;

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

			switch (property)
			{
				case "Speed":
					this.OnPropertyChanged("Hertz");
					break;

				case "StartTime":
				case "FinishTime":
				case "Proceed":
					this.OnPropertyChanged("ElapsedTime");
					break;

				case "ElapsedTime":
					this.OnPropertyChanged("ElapsedSeconds");
					break;

				case "Cycles":
					this.OnPropertyChanged("CyclesPerSecond");
					this.OnPropertyChanged("SimulatedElapsed");
					this.OnPropertyChanged("CycleDifference");
					this.OnPropertyChanged("HoldProportion");
					break;

				case "ElapsedSeconds":
					this.OnPropertyChanged("CyclesPerSecond");
					break;

				case "Hertz":
					this.OnPropertyChanged("SimulatedElapsed");
					this.OnPropertyChanged("Speedup");
					break;

				case "CyclesPerSecond":
					this.OnPropertyChanged("Speedup");
					break;

				case "HeldCycles":
					this.OnPropertyChanged("CycleDifference");
					break;

				case "CycleDifference":
					this.OnPropertyChanged("HoldProportion");
					break;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.timer != null)
					{
						this.timer.Dispose();
					}

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
			this.CheckDisposed();
			if (!this.processingTimer)
			{
				this.processingTimer = true;
				try
				{
					this.Propagate();
				}
				finally
				{
					this.processingTimer = false;
				}
			}
		}

		private void Propagate()
		{
			if (this.controller != null)
			{
				this.Speed = this.controller.Speed;
				this.StartTime = this.controller.StartTime;
				this.FinishTime = this.controller.FinishTime;

				var processor = this.controller.Processor;
				if (processor != null)
				{
					this.Cycles = processor.Cycles;
					this.HeldCycles = processor.HeldCycles;
					this.Proceed = processor.Proceed;
					this.Running = processor.Running;

					this.A = processor.A;
					this.X = processor.X;
					this.Y = processor.Y;
					this.S = processor.S;
					this.P = processor.P;

					this.PC = processor.PC;
				}
			}
		}

		private void Processor_Finished(object sender, EventArgs e)
		{
			this.CheckDisposed();

			var startCommand = (RelayCommand)this.StartCommand;
			startCommand.RaiseCanExecuteChanged();

			var stopCommand = (RelayCommand)this.StopCommand;
			stopCommand.RaiseCanExecuteChanged();
		}

		private void Processor_Starting(object sender, EventArgs e)
		{
			this.CheckDisposed();

			var startCommand = (RelayCommand)this.StartCommand;
			startCommand.RaiseCanExecuteChanged();

			var stopCommand = (RelayCommand)this.StopCommand;
			stopCommand.RaiseCanExecuteChanged();
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