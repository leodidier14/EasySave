using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class RemovePriorityExtCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public RemovePriorityExtCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.removePriorityExt(parameter as string);
        }
    }
}
