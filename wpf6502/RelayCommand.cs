namespace WPF6502
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute = null;
        private readonly Predicate<T> canExecute = null;

        public RelayCommand(Action<T> execute)
		: this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
			{
				CommandManager.RequerySuggested += value;
			}

            remove
			{
				CommandManager.RequerySuggested -= value;
			}
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null ? true : this.canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            this.execute((T)parameter);
        }
    }
}