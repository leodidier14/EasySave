using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class BackupModeCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public BackupModeCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                var s = parameter as string;
                if (string.IsNullOrEmpty(s))
                    return false;

                return true;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.backupMode(parameter as string);
        }
    }
}
