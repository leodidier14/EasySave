using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class StopCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public StopCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.stop(parameter as string);
        }
    }
}
