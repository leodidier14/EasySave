using System;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class AddCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ViewModel ViewModel { get; set; }

        public AddCmd(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch (parameter as string)
            {
                case "backup":
                    this.ViewModel.addBackup();
                    break;
                case "priority":
                    this.ViewModel.addPriorityExt();
                    break;
                case "crypt":
                    this.ViewModel.addCryptExt();
                    break;
                case "businesssoft":
                    this.ViewModel.addBusinessSoft();
                    break;
                case "maxfilesize":
                    this.ViewModel.addMaxFileSize();
                    break;                  
                default:
                    break;
            } 
        }
    }
}
