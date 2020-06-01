using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class SwitchLanguageCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public SwitchLanguageCmd(ViewModel viewModel)
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
               this.ViewModel.switchLanguage(parameter as string);
        }
    }
}
