namespace SpreadsheetReinforcement.Tools
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
            _action = action;
            _canExecute = canExecute;
        }

        public CommandWrapper(Action<object> action)
        {
            _action = action;
            _canExecute = null;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _action.Invoke(parameter);
        }
    }
}