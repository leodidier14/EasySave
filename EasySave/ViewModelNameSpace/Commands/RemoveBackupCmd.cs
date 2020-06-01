using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class RemoveBackupCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public RemoveBackupCmd(ViewModel viewModel)
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
            switch (parameter as string)
            {
                case "selected":
                    this.ViewModel.removeSelected();
                    break;
                case "finished":
                    this.ViewModel.removeFinished();
                    break;
                default:
                    break;
            }
            
        }
    }
}
