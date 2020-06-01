using System.IO;

namespace EasySave.ViewModelNameSpace
{
    class Checker
    {
        public ViewModel ViewModel { get; set; }

        public Checker(ViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public bool checkIfCanAdd()
        {
            //check if all fields are filled
            if(ViewModel.Visibility == "Hidden")
            {
                if (ViewModel.Name == "" || ViewModel.SourcePath == "" || ViewModel.TargetPath == "")
                {
                    ViewModel.messageConsole(ViewModel.TranslationList[30], false);
                    return false;
                }
            }
            else
            {
                if (ViewModel.Name == "" || ViewModel.SourcePath == "" || ViewModel.TargetPath == "" || ViewModel.MirrorPath == "")
                {
                    ViewModel.messageConsole(ViewModel.TranslationList[30], false);
                    return false;
                }
            }
            
            //check if fields are eligible  
            if (ViewModel.SourcePath == ViewModel.TargetPath)
            {
                ViewModel.messageConsole(ViewModel.TranslationList[50], false);
                return false;
            }

            if (ViewModel.MirrorPath != "" && ViewModel.TargetPath == ViewModel.MirrorPath)
            {
                ViewModel.messageConsole(ViewModel.TranslationList[51], false);
                return false;
            }

            //check if backup with a same folder path exist in list
            foreach (var backup in ViewModel.BackupList)
            {
                if (backup.Progress != 100)
                {
                    if (ViewModel.SourcePath == backup.Source)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[52], false);
                        return false;
                    }
                    if (ViewModel.TargetPath == backup.Target)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[53], false);
                        return false;
                    }
                    if (ViewModel.SourcePath == backup.Target)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[54], false);
                        return false;
                    }
                    if (ViewModel.TargetPath == backup.Source)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[55], false);
                        return false;
                    }
                    if (ViewModel.MirrorPath != "" && ViewModel.MirrorPath == backup.Target)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[56], false);
                        return false;
                    }
                    if(backup.Mirror != "" && ViewModel.TargetPath == backup.Mirror)
                    {
                        ViewModel.messageConsole(ViewModel.TranslationList[57], false);
                        return false;
                    }
                }
            }

            //check if the name of the backup is valid(to create the corresponding log file)
            if (ViewModel.Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ViewModel.messageConsole(ViewModel.TranslationList[31], false);
                return false;
            }

            //check if a backup with the same name already exists in the list or in the logs folder
            if (checkIfExist() || checkIfExistList())
            {
                return false;
            }

            return true;
        }

        //check if the name of the backup is valid(to create the corresponding log file)
        public bool checkIfExist()
        {
            DirectoryInfo logsDirectory = new DirectoryInfo(ViewModel.LogsPath + @"/RealTimeLogs/");

            if (!logsDirectory.Exists)
            {
                return false;
            }

            foreach (FileInfo file in logsDirectory.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(file.Name) == ViewModel.Name)
                {
                    ViewModel.messageConsole(ViewModel.TranslationList[33], false);
                    return true;
                }
            }

            return false;
        }

        //check if a backup with the same name already exists in the list or in the logs folder
        public bool checkIfExistList()
        {
            foreach (var backup in ViewModel.BackupList)
            {
                if (backup.Name == ViewModel.Name)
                {
                    ViewModel.messageConsole(ViewModel.TranslationList[34], false);
                    return true;
                }
            }
            return false;
        }

        //check if at least one backup can be started or deleted
        public bool checkIfBackupCanBeRunned()
        {
            foreach (var backup in ViewModel.BackupList)
            {
                if (backup.IsChecked)
                {
                    ViewModel.messageConsole(ViewModel.TranslationList[35], true);
                    return true;
                }
            }
            ViewModel.messageConsole(ViewModel.TranslationList[36], false);
            return false;
        }
    }
}
