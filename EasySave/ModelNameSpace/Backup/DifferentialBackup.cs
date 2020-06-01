using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.ModelNameSpace.Backup
{
    class DifferentialBackup : AbstractBackup
    {
        public DifferentialBackup(Dictionary<string, string> backupInformation)
        {
            this.Name = backupInformation["name"];
            this.Source = backupInformation["source"];
            this.Target = backupInformation["target"];
            this.Mirror = backupInformation["mirror"];
            this.LogsDir = backupInformation["logs"];

            //split the string to get a list of extensions or business softwares
            this.PriorityExtensions = backupInformation["priorityExt"].Split(';').ToList();
            this.BusinessSoftware = backupInformation["businessSoft"].Split(';').ToList();
            this.CryptExtensions = backupInformation["cryptExt"].Split(';').ToList();
        }

        //get the source files and copy them in target dir only if the files are not present or different in the mirror folder
        public override void save(string sourceDir, string targetDir, string mirrorDir, string logsDir, bool priorityFile)
        {
            //get info of source and mirror directories
            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
            DirectoryInfo mirrorDirInfo = new DirectoryInfo(mirrorDir);

            if (!sourceDirInfo.Exists)
            {
                return;
            }

            //copy the priority files
            if (priorityFile)
            {
                foreach (string fileExtension in this.PriorityExtensions)
                {
                    if (fileExtension != "")
                    {
                        //get the files in the directory and copy them to the new location.
                        foreach (FileInfo file in sourceDirInfo.GetFiles("*" + fileExtension))
                        {
                            //copy only elible files (differential)
                            if (this.fileCompare(file, mirrorDirInfo))
                            {
                                if (!Directory.Exists(targetDir))
                                {
                                    Directory.CreateDirectory(targetDir);
                                }
                                this.copy(file, targetDir);
                            }
                        }
                    }
                }
            }

            //copy the not priority files
            else
            {

                //get the files in the directory and copy them to the new location.
                foreach (FileInfo file in sourceDirInfo.GetFiles())
                {
                    //copy only elible files (differential)
                    if (this.fileCompare(file, mirrorDirInfo))
                    {
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        if (!(PriorityExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)))
                        {
                            this.copy(file, targetDir);
                        }
                    }
                }
            }

            this.subDir(sourceDirInfo, mirrorDir, targetDir, logsDir, priorityFile);
        }

        //check if a source file is existing in mirror directory and if yes, if it has been modified since the last copy
        public bool fileCompare(FileInfo sourceFile, DirectoryInfo mirrorDir)
        {
            FileInfo mirrorFile = new FileInfo(mirrorDir.FullName + @"\" + sourceFile.Name);

            //get the last writting time of the file
            DateTime lastWrittingSource = File.GetLastWriteTime(sourceFile.FullName);
            DateTime lastWrittingMirror = File.GetLastWriteTime(mirrorFile.FullName);

            //check the name
            if (!mirrorFile.Exists)
            {
                return true;
            }

            //check the last writting date
            if (lastWrittingSource != lastWrittingMirror)
            {
                return true;
            }

            return false;
        }

        //count the number of file to save and their total size
        public override int countFiles(string sourceDir, string mirrorDir)
        {
            int nbEligibleFiles = 0;

            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
            DirectoryInfo mirrorDirInfo = new DirectoryInfo(mirrorDir);

            /*increment the total size and the count of elilible files for each file, use fileCompare to only increment files 
            eligible for the differential backup*/
            foreach (FileInfo file in sourceDirInfo.GetFiles())
            {
                //Change jsonAttrributes only when files are eligible
                if (this.fileCompare(file, mirrorDirInfo))
                {
                    this.TotalSize += file.Length;
                    nbEligibleFiles++;
                }
            }

            /*increment the total size and the count of elilible files for each file in sub dirs, use fileCompare to only increment files 
            eligible for the differential backup*/
            foreach (DirectoryInfo diSourceSubDir in sourceDirInfo.GetDirectories())
            {
                int nbEligibleSubsFiles = 0;

                string tempPath = Path.Combine(mirrorDir, diSourceSubDir.Name);

                nbEligibleSubsFiles = this.countFiles(diSourceSubDir.FullName, tempPath);
                nbEligibleFiles = nbEligibleFiles + nbEligibleSubsFiles;
            }

            return nbEligibleFiles;
        }

    }

}
