using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace EasySave.ViewModelNameSpace.Commands
{
    class OpenDirCmd : ICommand
    {
        public event EventHandler CanExecuteChanged;
        

        public ViewModel ViewModel { get; set; }


        public OpenDirCmd(ViewModel viewModel)
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
            FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == DialogResult.OK && CanExecute(openFolderDialog.SelectedPath))
            {
                switch (parameter as string)
                {
                    case "sourcePath":
                        ViewModel.SourcePath = openFolderDialog.SelectedPath;
                        break;
                    case "mirrorPath":
                        ViewModel.MirrorPath = openFolderDialog.SelectedPath;
                        break;
                    case "targetPath":
                        ViewModel.TargetPath = openFolderDialog.SelectedPath;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
