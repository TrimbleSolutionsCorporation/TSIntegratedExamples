namespace Listener.ViewModel
{
    using System;
    using System.Windows.Input;

    public class CommandWrapper : ICommand
    {
        private Action<object> _action;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public CommandWrapper(Action<object> action, Func<object, bool> canExecute)
        {
            this._action = action;
            this._canExecute = canExecute;
        }

        public CommandWrapper(Action<object> action)
        {
            this._action = action;
            this._canExecute = null;
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecute == null) return true;
            return this._canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            this._action.Invoke(parameter);
        }
    }
}
