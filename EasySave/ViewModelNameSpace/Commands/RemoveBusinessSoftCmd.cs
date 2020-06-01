using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class RemoveBusinessSoftCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public RemoveBusinessSoftCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.removeBusinessSoft(parameter as string);
        }
    }
}

