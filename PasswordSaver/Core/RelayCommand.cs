using System;
using System.Windows.Input;

namespace PasswordSaver.Core
{
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        #endregion

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion 

        #region Events

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

        #endregion

        #region Methods
        public virtual bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public virtual void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion
    }
}