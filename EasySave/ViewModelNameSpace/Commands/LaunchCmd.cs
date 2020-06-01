using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class LaunchCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public LaunchCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.ViewModel.run();
        }
    }
}